using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SCTMS.Data;
using SCTMS.Models;
using System.Data.SqlClient;

namespace SCTMS.Services
{
    public class ComplianceService
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly UserRepository _userRepository;
        private readonly TrainingAssignmentRepository _trainingRepository;
        private readonly NotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ComplianceService> _logger;

        public ComplianceService(
            DatabaseHelper dbHelper,
            UserRepository userRepository,
            TrainingAssignmentRepository trainingRepository,
            NotificationService notificationService,
            IConfiguration configuration,
            ILogger<ComplianceService> logger)
        {
            _dbHelper = dbHelper;
            _userRepository = userRepository;
            _trainingRepository = trainingRepository;
            _notificationService = notificationService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginAccess?> CheckLoginAccessAsync(int userId)
        {
            const string sql = @"
                SELECT LoginAccessID, UserID, IsBlocked, BlockDate, BlockReason, 
                       UnblockRequestedBy, UnblockRequestDate, UnblockApproved, 
                       UnblockApprovedBy, UnblockApprovedDate, UnblockNotes, 
                       LastLoginAttempt, FailedLoginAttempts
                FROM LoginAccess 
                WHERE UserID = @UserID";

            var loginAccess = await _dbHelper.ExecuteReaderAsync(sql, MapLoginAccess,
                _dbHelper.CreateParameter("@UserID", userId));

            return loginAccess.FirstOrDefault();
        }

        public async Task<List<TrainingAssignment>> GetOverdueAssignmentsAsync(int userId)
        {
            var nonComplianceDays = int.Parse(_configuration["AppSettings:NonComplianceDays"] ?? "60");
            
            const string sql = @"
                SELECT AssignmentID, UserID, TrainingType, AssignedDate, Status, 
                       CompletionDate, NextDueDate, CompletionCertificate, Notes, 
                       AssignedBy, ReminderSentDate, ReminderCount, IsRefresher
                FROM TrainingAssignments 
                WHERE UserID = @UserID 
                  AND Status IN ('Assigned', 'InProgress') 
                  AND DATEDIFF(day, AssignedDate, GETDATE()) > @NonComplianceDays
                ORDER BY AssignedDate";

            return await _dbHelper.ExecuteReaderAsync(sql, MapTrainingAssignment,
                _dbHelper.CreateParameter("@UserID", userId),
                _dbHelper.CreateParameter("@NonComplianceDays", nonComplianceDays));
        }

        public async Task<List<User>> GetNonCompliantUsersAsync()
        {
            var nonComplianceDays = int.Parse(_configuration["AppSettings:NonComplianceDays"] ?? "60");
            return await _userRepository.GetNonCompliantUsersAsync(nonComplianceDays);
        }

        public async Task<bool> BlockUserForNonComplianceAsync(int userId, string reason = "NonCompliance")
        {
            try
            {
                // Check if already blocked
                var existingAccess = await CheckLoginAccessAsync(userId);
                if (existingAccess != null && existingAccess.IsBlocked)
                {
                    _logger.LogWarning("User {UserId} is already blocked", userId);
                    return false;
                }

                // Block the user
                const string sql = @"
                    MERGE LoginAccess AS target
                    USING (SELECT @UserID as UserID) AS source ON target.UserID = source.UserID
                    WHEN MATCHED THEN
                        UPDATE SET 
                            IsBlocked = 1,
                            BlockDate = GETDATE(),
                            BlockReason = @BlockReason
                    WHEN NOT MATCHED THEN
                        INSERT (UserID, IsBlocked, BlockDate, BlockReason)
                        VALUES (@UserID, 1, GETDATE(), @BlockReason);";

                var parameters = new[]
                {
                    _dbHelper.CreateParameter("@UserID", userId),
                    _dbHelper.CreateParameter("@BlockReason", reason)
                };

                await _dbHelper.ExecuteNonQueryAsync(sql, parameters);

                // Update user status
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user != null)
                {
                    user.Status = "NonCompliant";
                    await _userRepository.UpdateUserAsync(user);

                    // Send notification
                    await _notificationService.SendLoginBlockNotificationAsync(user, reason);
                    
                    _logger.LogInformation("User {UserId} blocked for {Reason}", userId, reason);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UnblockUserAsync(int userId, int approvedBy, string notes = "")
        {
            try
            {
                const string sql = @"
                    UPDATE LoginAccess 
                    SET IsBlocked = 0,
                        UnblockApproved = 1,
                        UnblockApprovedBy = @ApprovedBy,
                        UnblockApprovedDate = GETDATE(),
                        UnblockNotes = @Notes
                    WHERE UserID = @UserID AND IsBlocked = 1";

                var parameters = new[]
                {
                    _dbHelper.CreateParameter("@UserID", userId),
                    _dbHelper.CreateParameter("@ApprovedBy", approvedBy),
                    _dbHelper.CreateParameter("@Notes", notes)
                };

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);

                if (rowsAffected > 0)
                {
                    // Update user status back to Active
                    var user = await _userRepository.GetUserByIdAsync(userId);
                    if (user != null)
                    {
                        user.Status = "Active";
                        await _userRepository.UpdateUserAsync(user);
                    }

                    _logger.LogInformation("User {UserId} unblocked by {ApprovedBy}", userId, approvedBy);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unblocking user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> RequestUnblockAsync(int userId, int requestedBy)
        {
            try
            {
                const string sql = @"
                    UPDATE LoginAccess 
                    SET UnblockRequestedBy = @RequestedBy,
                        UnblockRequestDate = GETDATE()
                    WHERE UserID = @UserID AND IsBlocked = 1";

                var parameters = new[]
                {
                    _dbHelper.CreateParameter("@UserID", userId),
                    _dbHelper.CreateParameter("@RequestedBy", requestedBy)
                };

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);

                if (rowsAffected > 0)
                {
                    // Send notification to admin
                    var blockedUser = await _userRepository.GetUserByIdAsync(userId);
                    var requestingManager = await _userRepository.GetUserByIdAsync(requestedBy);

                    if (blockedUser != null && requestingManager != null)
                    {
                        await _notificationService.SendUnblockRequestNotificationAsync(requestingManager, blockedUser);
                    }

                    _logger.LogInformation("Unblock requested for user {UserId} by {RequestedBy}", userId, requestedBy);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting unblock for user {UserId}", userId);
                return false;
            }
        }

        public async Task<List<LoginAccess>> GetPendingUnblockRequestsAsync()
        {
            const string sql = @"
                SELECT la.LoginAccessID, la.UserID, la.IsBlocked, la.BlockDate, la.BlockReason, 
                       la.UnblockRequestedBy, la.UnblockRequestDate, la.UnblockApproved, 
                       la.UnblockApprovedBy, la.UnblockApprovedDate, la.UnblockNotes, 
                       la.LastLoginAttempt, la.FailedLoginAttempts
                FROM LoginAccess la
                WHERE la.IsBlocked = 1 
                  AND la.UnblockRequestedBy IS NOT NULL 
                  AND la.UnblockApproved = 0
                ORDER BY la.UnblockRequestDate";

            return await _dbHelper.ExecuteReaderAsync(sql, MapLoginAccess);
        }

        public async Task<ComplianceSummary> GetComplianceSummaryAsync()
        {
            const string sql = @"
                SELECT 
                    COUNT(*) as TotalUsers,
                    COUNT(CASE WHEN u.Status = 'Active' THEN 1 END) as ActiveUsers,
                    COUNT(CASE WHEN u.Status = 'NonCompliant' THEN 1 END) as NonCompliantUsers,
                    COUNT(CASE WHEN la.IsBlocked = 1 THEN 1 END) as BlockedUsers,
                    COUNT(DISTINCT CASE WHEN ta.Status IN ('Assigned', 'InProgress') AND DATEDIFF(day, ta.AssignedDate, GETDATE()) > 60 THEN ta.UserID END) as UsersWithOverdueTraining,
                    COUNT(CASE WHEN ta.Status = 'Completed' THEN 1 END) as CompletedTrainings,
                    COUNT(CASE WHEN ta.Status IN ('Assigned', 'InProgress') THEN 1 END) as PendingTrainings,
                    COUNT(CASE WHEN ta.Status = 'Overdue' THEN 1 END) as OverdueTrainings
                FROM Users u
                LEFT JOIN LoginAccess la ON u.UserID = la.UserID
                LEFT JOIN TrainingAssignments ta ON u.UserID = ta.UserID";

            var result = await _dbHelper.ExecuteReaderAsync(sql, reader => new ComplianceSummary
            {
                TotalUsers = reader.GetInt32(0),
                ActiveUsers = reader.GetInt32(1),
                NonCompliantUsers = reader.GetInt32(2),
                BlockedUsers = reader.GetInt32(3),
                UsersWithOverdueTraining = reader.GetInt32(4),
                CompletedTrainings = reader.GetInt32(5),
                PendingTrainings = reader.GetInt32(6),
                OverdueTrainings = reader.GetInt32(7)
            });

            return result.FirstOrDefault() ?? new ComplianceSummary();
        }

        public async Task<int> ProcessAutomaticComplianceAsync()
        {
            try
            {
                var nonComplianceDays = int.Parse(_configuration["AppSettings:NonComplianceDays"] ?? "60");
                var autoBlockEnabled = bool.Parse(_configuration["AppSettings:AutoBlockEnabled"] ?? "true");

                if (!autoBlockEnabled)
                {
                    _logger.LogInformation("Automatic compliance blocking is disabled");
                    return 0;
                }

                // Get all non-compliant users
                var nonCompliantUsers = await GetNonCompliantUsersAsync();
                int blockedCount = 0;

                foreach (var user in nonCompliantUsers)
                {
                    // Check if already blocked
                    var loginAccess = await CheckLoginAccessAsync(user.UserID);
                    if (loginAccess == null || !loginAccess.IsBlocked)
                    {
                        // Get overdue assignments for notifications
                        var overdueAssignments = await GetOverdueAssignmentsAsync(user.UserID);
                        
                        // Send compliance alert before blocking
                        await _notificationService.SendComplianceAlertAsync(user, overdueAssignments);

                        // Send manager escalation
                        if (user.ReportingManagerID.HasValue)
                        {
                            var manager = await _userRepository.GetUserByIdAsync(user.ReportingManagerID.Value);
                            if (manager != null)
                            {
                                await _notificationService.SendManagerEscalationAsync(manager, user, overdueAssignments);
                            }
                        }

                        // Block the user
                        if (await BlockUserForNonComplianceAsync(user.UserID))
                        {
                            blockedCount++;
                        }
                    }
                }

                _logger.LogInformation("Automatic compliance processing completed. {BlockedCount} users blocked", blockedCount);
                return blockedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during automatic compliance processing");
                return 0;
            }
        }

        public async Task<List<ComplianceReport>> GetDepartmentComplianceReportAsync()
        {
            const string sql = @"
                SELECT 
                    u.Department,
                    COUNT(*) as TotalUsers,
                    COUNT(CASE WHEN u.Status = 'Active' THEN 1 END) as ActiveUsers,
                    COUNT(CASE WHEN u.Status = 'NonCompliant' THEN 1 END) as NonCompliantUsers,
                    COUNT(CASE WHEN la.IsBlocked = 1 THEN 1 END) as BlockedUsers,
                    COUNT(CASE WHEN ta.Status = 'Completed' THEN 1 END) as CompletedTrainings,
                    COUNT(CASE WHEN ta.Status IN ('Assigned', 'InProgress') THEN 1 END) as PendingTrainings,
                    COUNT(CASE WHEN ta.Status = 'Overdue' THEN 1 END) as OverdueTrainings,
                    CAST(COUNT(CASE WHEN ta.Status = 'Completed' THEN 1 END) * 100.0 / NULLIF(COUNT(ta.AssignmentID), 0) AS DECIMAL(5,2)) as CompliancePercentage
                FROM Users u
                LEFT JOIN LoginAccess la ON u.UserID = la.UserID
                LEFT JOIN TrainingAssignments ta ON u.UserID = ta.UserID
                WHERE u.Status = 'Active'
                GROUP BY u.Department
                ORDER BY u.Department";

            return await _dbHelper.ExecuteReaderAsync(sql, reader => new ComplianceReport
            {
                Department = reader.GetString("Department"),
                TotalUsers = reader.GetInt32("TotalUsers"),
                ActiveUsers = reader.GetInt32("ActiveUsers"),
                NonCompliantUsers = reader.GetInt32("NonCompliantUsers"),
                BlockedUsers = reader.GetInt32("BlockedUsers"),
                CompletedTrainings = reader.GetInt32("CompletedTrainings"),
                PendingTrainings = reader.GetInt32("PendingTrainings"),
                OverdueTrainings = reader.GetInt32("OverdueTrainings"),
                CompliancePercentage = reader.IsDBNull("CompliancePercentage") ? 0 : reader.GetDecimal("CompliancePercentage")
            });
        }

        private static LoginAccess MapLoginAccess(SqlDataReader reader)
        {
            return new LoginAccess
            {
                LoginAccessID = reader.GetInt32("LoginAccessID"),
                UserID = reader.GetInt32("UserID"),
                IsBlocked = reader.GetBoolean("IsBlocked"),
                BlockDate = reader.IsDBNull("BlockDate") ? null : reader.GetDateTime("BlockDate"),
                BlockReason = reader.IsDBNull("BlockReason") ? string.Empty : reader.GetString("BlockReason"),
                UnblockRequestedBy = reader.IsDBNull("UnblockRequestedBy") ? null : reader.GetInt32("UnblockRequestedBy"),
                UnblockRequestDate = reader.IsDBNull("UnblockRequestDate") ? null : reader.GetDateTime("UnblockRequestDate"),
                UnblockApproved = reader.GetBoolean("UnblockApproved"),
                UnblockApprovedBy = reader.IsDBNull("UnblockApprovedBy") ? null : reader.GetInt32("UnblockApprovedBy"),
                UnblockApprovedDate = reader.IsDBNull("UnblockApprovedDate") ? null : reader.GetDateTime("UnblockApprovedDate"),
                UnblockNotes = reader.IsDBNull("UnblockNotes") ? string.Empty : reader.GetString("UnblockNotes"),
                LastLoginAttempt = reader.IsDBNull("LastLoginAttempt") ? null : reader.GetDateTime("LastLoginAttempt"),
                FailedLoginAttempts = reader.GetInt32("FailedLoginAttempts")
            };
        }

        private static TrainingAssignment MapTrainingAssignment(SqlDataReader reader)
        {
            return new TrainingAssignment
            {
                AssignmentID = reader.GetInt32("AssignmentID"),
                UserID = reader.GetInt32("UserID"),
                TrainingType = reader.GetString("TrainingType"),
                AssignedDate = reader.GetDateTime("AssignedDate"),
                Status = reader.GetString("Status"),
                CompletionDate = reader.IsDBNull("CompletionDate") ? null : reader.GetDateTime("CompletionDate"),
                NextDueDate = reader.IsDBNull("NextDueDate") ? null : reader.GetDateTime("NextDueDate"),
                CompletionCertificate = reader.IsDBNull("CompletionCertificate") ? string.Empty : reader.GetString("CompletionCertificate"),
                Notes = reader.IsDBNull("Notes") ? string.Empty : reader.GetString("Notes"),
                AssignedBy = reader.IsDBNull("AssignedBy") ? null : reader.GetInt32("AssignedBy"),
                ReminderSentDate = reader.IsDBNull("ReminderSentDate") ? null : reader.GetDateTime("ReminderSentDate"),
                ReminderCount = reader.GetInt32("ReminderCount"),
                IsRefresher = reader.GetBoolean("IsRefresher")
            };
        }
    }

    public class ComplianceSummary
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NonCompliantUsers { get; set; }
        public int BlockedUsers { get; set; }
        public int UsersWithOverdueTraining { get; set; }
        public int CompletedTrainings { get; set; }
        public int PendingTrainings { get; set; }
        public int OverdueTrainings { get; set; }
        public decimal CompliancePercentage => TotalUsers > 0 ? ((decimal)(TotalUsers - NonCompliantUsers) / TotalUsers) * 100 : 0;
    }

    public class ComplianceReport
    {
        public string Department { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NonCompliantUsers { get; set; }
        public int BlockedUsers { get; set; }
        public int CompletedTrainings { get; set; }
        public int PendingTrainings { get; set; }
        public int OverdueTrainings { get; set; }
        public decimal CompliancePercentage { get; set; }
    }
} 