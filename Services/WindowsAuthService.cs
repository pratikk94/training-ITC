using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using Microsoft.Extensions.Logging;
using SCTMS.Data;
using SCTMS.Models;

namespace SCTMS.Services
{
    public class WindowsAuthService
    {
        private readonly UserRepository _userRepository;
        private readonly ILogger<WindowsAuthService> _logger;

        public WindowsAuthService(UserRepository userRepository, ILogger<WindowsAuthService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User?> AuthenticateCurrentUserAsync()
        {
            try
            {
                // Get current Windows user
                var windowsIdentity = WindowsIdentity.GetCurrent();
                if (windowsIdentity == null || string.IsNullOrEmpty(windowsIdentity.Name))
                {
                    _logger.LogWarning("Unable to get current Windows user identity");
                    return null;
                }

                var username = windowsIdentity.Name;
                _logger.LogInformation("Authenticating Windows user: {Username}", username);

                // Extract just the username part (remove domain)
                var domainUser = username.Split('\\');
                var simpleUsername = domainUser.Length > 1 ? domainUser[1] : domainUser[0];

                // Try to find user in database
                var user = await _userRepository.GetUserByWindowsUsernameAsync(simpleUsername);
                
                if (user == null)
                {
                    // User not found in database, try to get from Active Directory
                    user = await GetUserFromActiveDirectoryAsync(simpleUsername);
                    
                    if (user != null)
                    {
                        // Create new user in database
                        user.UserID = await _userRepository.CreateUserAsync(user);
                        _logger.LogInformation("Created new user from AD: {Username}", simpleUsername);
                    }
                }

                if (user != null)
                {
                    // Update last login time
                    await _userRepository.UpdateLastLoginAsync(user.UserID);
                    _logger.LogInformation("User authenticated successfully: {UserId} - {Name}", user.UserID, user.Name);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Windows authentication");
                return null;
            }
        }

        public async Task<User?> GetUserFromActiveDirectoryAsync(string username)
        {
            try
            {
                using var context = new PrincipalContext(ContextType.Domain);
                using var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
                
                if (userPrincipal == null)
                {
                    _logger.LogWarning("User not found in Active Directory: {Username}", username);
                    return null;
                }

                // Get additional properties using DirectoryEntry
                var directoryEntry = (DirectoryEntry)userPrincipal.GetUnderlyingObject();
                
                var user = new User
                {
                    Name = GetPropertyValue(directoryEntry, "displayName") ?? GetPropertyValue(directoryEntry, "cn") ?? username,
                    EmployeeID = GetPropertyValue(directoryEntry, "employeeID") ?? username,
                    Email = GetPropertyValue(directoryEntry, "mail") ?? $"{username}@company.com",
                    WindowsUsername = username,
                    Department = GetPropertyValue(directoryEntry, "department") ?? "Unknown",
                    Level = GetPropertyValue(directoryEntry, "title") ?? "Employee",
                    Status = "Active",
                    Role = DetermineUserRole(directoryEntry),
                    CreatedDate = DateTime.Now
                };

                // Try to get reporting manager
                var managerDn = GetPropertyValue(directoryEntry, "manager");
                if (!string.IsNullOrEmpty(managerDn))
                {
                    var manager = await GetManagerFromDistinguishedNameAsync(managerDn);
                    if (manager != null)
                    {
                        user.ReportingManagerID = manager.UserID;
                    }
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user from Active Directory: {Username}", username);
                return null;
            }
        }

        public bool IsUserInRole(string username, string roleName)
        {
            try
            {
                using var context = new PrincipalContext(ContextType.Domain);
                using var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
                
                if (user == null) return false;

                using var group = GroupPrincipal.FindByIdentity(context, roleName);
                return group != null && user.IsMemberOf(group);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user role: {Username} - {Role}", username, roleName);
                return false;
            }
        }

        public async Task<List<User>> GetUsersFromActiveDirectoryByGroupAsync(string groupName)
        {
            var users = new List<User>();
            
            try
            {
                using var context = new PrincipalContext(ContextType.Domain);
                using var group = GroupPrincipal.FindByIdentity(context, groupName);
                
                if (group == null)
                {
                    _logger.LogWarning("Group not found in Active Directory: {GroupName}", groupName);
                    return users;
                }

                foreach (var member in group.GetMembers())
                {
                    if (member is UserPrincipal userPrincipal && !string.IsNullOrEmpty(userPrincipal.SamAccountName))
                    {
                        var user = await GetUserFromActiveDirectoryAsync(userPrincipal.SamAccountName);
                        if (user != null)
                        {
                            users.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users from AD group: {GroupName}", groupName);
            }

            return users;
        }

        public async Task<bool> SyncUsersFromActiveDirectoryAsync()
        {
            try
            {
                _logger.LogInformation("Starting Active Directory user synchronization");

                // Get all users from AD (you might want to filter by specific OUs or groups)
                var adUsers = await GetAllUsersFromActiveDirectoryAsync();
                var dbUsers = await _userRepository.GetAllUsersAsync();

                int created = 0, updated = 0;

                foreach (var adUser in adUsers)
                {
                    var existingUser = dbUsers.FirstOrDefault(u => 
                        u.WindowsUsername.Equals(adUser.WindowsUsername, StringComparison.OrdinalIgnoreCase));

                    if (existingUser == null)
                    {
                        // Create new user
                        await _userRepository.CreateUserAsync(adUser);
                        created++;
                    }
                    else if (HasUserChanged(existingUser, adUser))
                    {
                        // Update existing user
                        adUser.UserID = existingUser.UserID;
                        adUser.CreatedDate = existingUser.CreatedDate;
                        adUser.LastLoginDate = existingUser.LastLoginDate;
                        
                        await _userRepository.UpdateUserAsync(adUser);
                        updated++;
                    }
                }

                _logger.LogInformation("AD sync completed. Created: {Created}, Updated: {Updated}", created, updated);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Active Directory synchronization");
                return false;
            }
        }

        private async Task<List<User>> GetAllUsersFromActiveDirectoryAsync()
        {
            var users = new List<User>();
            
            try
            {
                using var context = new PrincipalContext(ContextType.Domain);
                using var searcher = new PrincipalSearcher(new UserPrincipal(context));
                
                foreach (var result in searcher.FindAll())
                {
                    if (result is UserPrincipal userPrincipal && 
                        !string.IsNullOrEmpty(userPrincipal.SamAccountName) &&
                        userPrincipal.Enabled == true)
                    {
                        var user = await GetUserFromActiveDirectoryAsync(userPrincipal.SamAccountName);
                        if (user != null)
                        {
                            users.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users from Active Directory");
            }

            return users;
        }

        private async Task<User?> GetManagerFromDistinguishedNameAsync(string managerDn)
        {
            try
            {
                using var directoryEntry = new DirectoryEntry($"LDAP://{managerDn}");
                var samAccountName = GetPropertyValue(directoryEntry, "sAMAccountName");
                
                if (!string.IsNullOrEmpty(samAccountName))
                {
                    var manager = await _userRepository.GetUserByWindowsUsernameAsync(samAccountName);
                    if (manager == null)
                    {
                        // Manager not in database, try to get from AD
                        manager = await GetUserFromActiveDirectoryAsync(samAccountName);
                        if (manager != null)
                        {
                            manager.UserID = await _userRepository.CreateUserAsync(manager);
                        }
                    }
                    return manager;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting manager from DN: {ManagerDN}", managerDn);
            }

            return null;
        }

        private string DetermineUserRole(DirectoryEntry directoryEntry)
        {
            try
            {
                var title = GetPropertyValue(directoryEntry, "title")?.ToLower() ?? "";
                var department = GetPropertyValue(directoryEntry, "department")?.ToLower() ?? "";
                
                if (title.Contains("manager") || title.Contains("supervisor") || title.Contains("lead"))
                    return UserRole.Manager.ToString();
                    
                if (department.Contains("hr") || department.Contains("human resource"))
                    return UserRole.HR.ToString();
                    
                if (department.Contains("safety") || title.Contains("safety"))
                    return UserRole.Safety.ToString();
                    
                if (title.Contains("admin") || title.Contains("administrator"))
                    return UserRole.Admin.ToString();
                    
                return UserRole.Manager.ToString(); // Default role
            }
            catch
            {
                return UserRole.Manager.ToString();
            }
        }

        private string? GetPropertyValue(DirectoryEntry directoryEntry, string propertyName)
        {
            try
            {
                if (directoryEntry.Properties.Contains(propertyName) && 
                    directoryEntry.Properties[propertyName].Count > 0)
                {
                    return directoryEntry.Properties[propertyName][0]?.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error getting property {Property} from directory entry", propertyName);
            }

            return null;
        }

        private bool HasUserChanged(User dbUser, User adUser)
        {
            return dbUser.Name != adUser.Name ||
                   dbUser.Email != adUser.Email ||
                   dbUser.Department != adUser.Department ||
                   dbUser.Level != adUser.Level ||
                   dbUser.EmployeeID != adUser.EmployeeID;
        }
    }
} 