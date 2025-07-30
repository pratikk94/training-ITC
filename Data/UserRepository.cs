using System.Data;
using System.Data.SqlClient;
using SCTMS.Models;
using SCTMS.Utilities;

namespace SCTMS.Data
{
    public class UserRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public UserRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            const string sql = @"
                SELECT UserID, Name, EmployeeID, Level, Department, Status, 
                       ReportingManagerID, Role, CreatedDate, LastLoginDate, 
                       Email, WindowsUsername
                FROM Users 
                ORDER BY Name";

            return await _dbHelper.ExecuteReaderAsync(sql, MapUser);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            const string sql = @"
                SELECT UserID, Name, EmployeeID, Level, Department, Status, 
                       ReportingManagerID, Role, CreatedDate, LastLoginDate, 
                       Email, WindowsUsername
                FROM Users 
                WHERE UserID = @UserID";

            var users = await _dbHelper.ExecuteReaderAsync(sql, MapUser, 
                _dbHelper.CreateParameter("@UserID", userId));
            
            return users.FirstOrDefault();
        }

        public async Task<User?> GetUserByEmployeeIdAsync(string employeeId)
        {
            const string sql = @"
                SELECT UserID, Name, EmployeeID, Level, Department, Status, 
                       ReportingManagerID, Role, CreatedDate, LastLoginDate, 
                       Email, WindowsUsername
                FROM Users 
                WHERE EmployeeID = @EmployeeID";

            var users = await _dbHelper.ExecuteReaderAsync(sql, MapUser, 
                _dbHelper.CreateParameter("@EmployeeID", employeeId));
            
            return users.FirstOrDefault();
        }

        public async Task<User?> GetUserByWindowsUsernameAsync(string windowsUsername)
        {
            const string sql = @"
                SELECT UserID, Name, EmployeeID, Level, Department, Status, 
                       ReportingManagerID, Role, CreatedDate, LastLoginDate, 
                       Email, WindowsUsername
                FROM Users 
                WHERE WindowsUsername = @WindowsUsername";

            var users = await _dbHelper.ExecuteReaderAsync(sql, MapUser, 
                _dbHelper.CreateParameter("@WindowsUsername", windowsUsername));
            
            return users.FirstOrDefault();
        }

        public async Task<int> CreateUserAsync(User user)
        {
            const string sql = @"
                INSERT INTO Users (Name, EmployeeID, Level, Department, Status, 
                                 ReportingManagerID, Role, CreatedDate, Email, WindowsUsername)
                VALUES (@Name, @EmployeeID, @Level, @Department, @Status, 
                        @ReportingManagerID, @Role, @CreatedDate, @Email, @WindowsUsername);
                SELECT SCOPE_IDENTITY();";

            var parameters = new[]
            {
                _dbHelper.CreateParameter("@Name", user.Name),
                _dbHelper.CreateParameter("@EmployeeID", user.EmployeeID),
                _dbHelper.CreateParameter("@Level", user.Level),
                _dbHelper.CreateParameter("@Department", user.Department),
                _dbHelper.CreateParameter("@Status", user.Status),
                _dbHelper.CreateParameter("@ReportingManagerID", user.ReportingManagerID),
                _dbHelper.CreateParameter("@Role", user.Role),
                _dbHelper.CreateParameter("@CreatedDate", DateTime.Now),
                _dbHelper.CreateParameter("@Email", user.Email),
                _dbHelper.CreateParameter("@WindowsUsername", user.WindowsUsername)
            };

            var result = await _dbHelper.ExecuteScalarAsync<decimal>(sql, parameters);
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            const string sql = @"
                UPDATE Users 
                SET Name = @Name, 
                    EmployeeID = @EmployeeID, 
                    Level = @Level, 
                    Department = @Department, 
                    Status = @Status,
                    ReportingManagerID = @ReportingManagerID, 
                    Role = @Role, 
                    Email = @Email, 
                    WindowsUsername = @WindowsUsername
                WHERE UserID = @UserID";

            var parameters = new[]
            {
                _dbHelper.CreateParameter("@UserID", user.UserID),
                _dbHelper.CreateParameter("@Name", user.Name),
                _dbHelper.CreateParameter("@EmployeeID", user.EmployeeID),
                _dbHelper.CreateParameter("@Level", user.Level),
                _dbHelper.CreateParameter("@Department", user.Department),
                _dbHelper.CreateParameter("@Status", user.Status),
                _dbHelper.CreateParameter("@ReportingManagerID", user.ReportingManagerID),
                _dbHelper.CreateParameter("@Role", user.Role),
                _dbHelper.CreateParameter("@Email", user.Email),
                _dbHelper.CreateParameter("@WindowsUsername", user.WindowsUsername)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            const string sql = "UPDATE Users SET LastLoginDate = @LastLoginDate WHERE UserID = @UserID";
            
            var parameters = new[]
            {
                _dbHelper.CreateParameter("@UserID", userId),
                _dbHelper.CreateParameter("@LastLoginDate", DateTime.Now)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }

        public async Task<List<User>> GetUsersByManagerAsync(int managerId)
        {
            const string sql = @"
                SELECT UserID, Name, EmployeeID, Level, Department, Status, 
                       ReportingManagerID, Role, CreatedDate, LastLoginDate, 
                       Email, WindowsUsername
                FROM Users 
                WHERE ReportingManagerID = @ManagerID
                ORDER BY Name";

            return await _dbHelper.ExecuteReaderAsync(sql, MapUser, 
                _dbHelper.CreateParameter("@ManagerID", managerId));
        }

        public async Task<List<User>> GetNonCompliantUsersAsync(int nonComplianceDays)
        {
            const string sql = @"
                SELECT DISTINCT u.UserID, u.Name, u.EmployeeID, u.Level, u.Department, 
                       u.Status, u.ReportingManagerID, u.Role, u.CreatedDate, 
                       u.LastLoginDate, u.Email, u.WindowsUsername
                FROM Users u
                INNER JOIN TrainingAssignments ta ON u.UserID = ta.UserID
                WHERE ta.Status IN ('Assigned', 'Overdue') 
                  AND DATEDIFF(day, ta.AssignedDate, GETDATE()) > @NonComplianceDays
                  AND u.Status = 'Active'
                ORDER BY u.Name";

            return await _dbHelper.ExecuteReaderAsync(sql, MapUser, 
                _dbHelper.CreateParameter("@NonComplianceDays", nonComplianceDays));
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            const string sql = "DELETE FROM Users WHERE UserID = @UserID";
            
            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql, 
                _dbHelper.CreateParameter("@UserID", userId));
            
            return rowsAffected > 0;
        }

        private static User MapUser(SqlDataReader reader)
        {
            return new User
            {
                UserID = SafeConverter.GetInt(reader, "UserID"),
                Name = SafeConverter.GetString(reader, "Name"),
                EmployeeID = SafeConverter.GetString(reader, "EmployeeID"),
                Level = SafeConverter.GetString(reader, "Level"),
                Department = SafeConverter.GetString(reader, "Department"),
                Status = SafeConverter.GetString(reader, "Status"),
                ReportingManagerID = SafeConverter.GetNullableInt(reader, "ReportingManagerID"),
                Role = SafeConverter.GetString(reader, "Role"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                LastLoginDate = SafeConverter.GetNullableDateTime(reader, "LastLoginDate"),
                Email = SafeConverter.GetString(reader, "Email"),
                WindowsUsername = SafeConverter.GetString(reader, "WindowsUsername")
            };
        }
    }
} 