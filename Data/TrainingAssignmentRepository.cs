using System.Data.SqlClient;
using SCTMS.Models;
using SCTMS.Utilities;

namespace SCTMS.Data
{
    public class TrainingAssignmentRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public TrainingAssignmentRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<TrainingAssignment>> GetAllAssignmentsAsync()
        {
            const string sql = @"
                SELECT AssignmentID, UserID, TrainingType, AssignedDate, Status, 
                       CompletionDate, NextDueDate, CompletionCertificate, Notes, 
                       AssignedBy, ReminderSentDate, ReminderCount, IsRefresher
                FROM TrainingAssignments 
                ORDER BY AssignedDate DESC";

            return await _dbHelper.ExecuteReaderAsync(sql, MapTrainingAssignment);
        }

        public async Task<List<TrainingAssignment>> GetAssignmentsByUserAsync(int userId)
        {
            const string sql = @"
                SELECT AssignmentID, UserID, TrainingType, AssignedDate, Status, 
                       CompletionDate, NextDueDate, CompletionCertificate, Notes, 
                       AssignedBy, ReminderSentDate, ReminderCount, IsRefresher
                FROM TrainingAssignments 
                WHERE UserID = @UserID
                ORDER BY AssignedDate DESC";

            return await _dbHelper.ExecuteReaderAsync(sql, MapTrainingAssignment,
                _dbHelper.CreateParameter("@UserID", userId));
        }

        public async Task<List<TrainingAssignment>> GetOverdueAssignmentsAsync()
        {
            const string sql = @"
                SELECT AssignmentID, UserID, TrainingType, AssignedDate, Status, 
                       CompletionDate, NextDueDate, CompletionCertificate, Notes, 
                       AssignedBy, ReminderSentDate, ReminderCount, IsRefresher
                FROM TrainingAssignments 
                WHERE Status IN ('Assigned', 'InProgress') 
                  AND DATEDIFF(day, AssignedDate, GETDATE()) > 60
                ORDER BY AssignedDate";

            return await _dbHelper.ExecuteReaderAsync(sql, MapTrainingAssignment);
        }

        public async Task<List<TrainingAssignment>> GetDueForReminderAsync(int reminderIntervalDays)
        {
            const string sql = @"
                SELECT AssignmentID, UserID, TrainingType, AssignedDate, Status, 
                       CompletionDate, NextDueDate, CompletionCertificate, Notes, 
                       AssignedBy, ReminderSentDate, ReminderCount, IsRefresher
                FROM TrainingAssignments 
                WHERE Status IN ('Assigned', 'InProgress') 
                  AND (ReminderSentDate IS NULL OR DATEDIFF(day, ReminderSentDate, GETDATE()) >= @ReminderInterval)
                  AND DATEDIFF(day, AssignedDate, GETDATE()) > 0
                ORDER BY AssignedDate";

            return await _dbHelper.ExecuteReaderAsync(sql, MapTrainingAssignment,
                _dbHelper.CreateParameter("@ReminderInterval", reminderIntervalDays));
        }

        public async Task<List<TrainingAssignment>> GetDueForRefresherAsync(int refresherYears)
        {
            const string sql = @"
                SELECT AssignmentID, UserID, TrainingType, AssignedDate, Status, 
                       CompletionDate, NextDueDate, CompletionCertificate, Notes, 
                       AssignedBy, ReminderSentDate, ReminderCount, IsRefresher
                FROM TrainingAssignments 
                WHERE Status = 'Completed' 
                  AND NextDueDate <= GETDATE()
                  AND DATEDIFF(year, CompletionDate, GETDATE()) >= @RefresherYears
                ORDER BY NextDueDate";

            return await _dbHelper.ExecuteReaderAsync(sql, MapTrainingAssignment,
                _dbHelper.CreateParameter("@RefresherYears", refresherYears));
        }

        public async Task<TrainingAssignment?> GetAssignmentByIdAsync(int assignmentId)
        {
            const string sql = @"
                SELECT AssignmentID, UserID, TrainingType, AssignedDate, Status, 
                       CompletionDate, NextDueDate, CompletionCertificate, Notes, 
                       AssignedBy, ReminderSentDate, ReminderCount, IsRefresher
                FROM TrainingAssignments 
                WHERE AssignmentID = @AssignmentID";

            var assignments = await _dbHelper.ExecuteReaderAsync(sql, MapTrainingAssignment,
                _dbHelper.CreateParameter("@AssignmentID", assignmentId));

            return assignments.FirstOrDefault();
        }

        public async Task<int> CreateAssignmentAsync(TrainingAssignment assignment)
        {
            const string sql = @"
                INSERT INTO TrainingAssignments (UserID, TrainingType, AssignedDate, Status, 
                                               CompletionDate, NextDueDate, CompletionCertificate, 
                                               Notes, AssignedBy, ReminderCount, IsRefresher)
                VALUES (@UserID, @TrainingType, @AssignedDate, @Status, @CompletionDate, 
                        @NextDueDate, @CompletionCertificate, @Notes, @AssignedBy, 
                        @ReminderCount, @IsRefresher);
                SELECT SCOPE_IDENTITY();";

            var parameters = new[]
            {
                _dbHelper.CreateParameter("@UserID", assignment.UserID),
                _dbHelper.CreateParameter("@TrainingType", assignment.TrainingType),
                _dbHelper.CreateParameter("@AssignedDate", assignment.AssignedDate),
                _dbHelper.CreateParameter("@Status", assignment.Status),
                _dbHelper.CreateParameter("@CompletionDate", assignment.CompletionDate),
                _dbHelper.CreateParameter("@NextDueDate", assignment.NextDueDate),
                _dbHelper.CreateParameter("@CompletionCertificate", assignment.CompletionCertificate),
                _dbHelper.CreateParameter("@Notes", assignment.Notes),
                _dbHelper.CreateParameter("@AssignedBy", assignment.AssignedBy),
                _dbHelper.CreateParameter("@ReminderCount", assignment.ReminderCount),
                _dbHelper.CreateParameter("@IsRefresher", assignment.IsRefresher)
            };

            var result = await _dbHelper.ExecuteScalarAsync<decimal>(sql, parameters);
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAssignmentAsync(TrainingAssignment assignment)
        {
            const string sql = @"
                UPDATE TrainingAssignments 
                SET UserID = @UserID, 
                    TrainingType = @TrainingType, 
                    AssignedDate = @AssignedDate, 
                    Status = @Status,
                    CompletionDate = @CompletionDate, 
                    NextDueDate = @NextDueDate, 
                    CompletionCertificate = @CompletionCertificate, 
                    Notes = @Notes,
                    AssignedBy = @AssignedBy,
                    ReminderSentDate = @ReminderSentDate,
                    ReminderCount = @ReminderCount,
                    IsRefresher = @IsRefresher
                WHERE AssignmentID = @AssignmentID";

            var parameters = new[]
            {
                _dbHelper.CreateParameter("@AssignmentID", assignment.AssignmentID),
                _dbHelper.CreateParameter("@UserID", assignment.UserID),
                _dbHelper.CreateParameter("@TrainingType", assignment.TrainingType),
                _dbHelper.CreateParameter("@AssignedDate", assignment.AssignedDate),
                _dbHelper.CreateParameter("@Status", assignment.Status),
                _dbHelper.CreateParameter("@CompletionDate", assignment.CompletionDate),
                _dbHelper.CreateParameter("@NextDueDate", assignment.NextDueDate),
                _dbHelper.CreateParameter("@CompletionCertificate", assignment.CompletionCertificate),
                _dbHelper.CreateParameter("@Notes", assignment.Notes),
                _dbHelper.CreateParameter("@AssignedBy", assignment.AssignedBy),
                _dbHelper.CreateParameter("@ReminderSentDate", assignment.ReminderSentDate),
                _dbHelper.CreateParameter("@ReminderCount", assignment.ReminderCount),
                _dbHelper.CreateParameter("@IsRefresher", assignment.IsRefresher)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> CompleteAssignmentAsync(int assignmentId, string certificate, DateTime completionDate, int refresherYears)
        {
            const string sql = @"
                UPDATE TrainingAssignments 
                SET Status = 'Completed',
                    CompletionDate = @CompletionDate,
                    CompletionCertificate = @Certificate,
                    NextDueDate = DATEADD(year, @RefresherYears, @CompletionDate)
                WHERE AssignmentID = @AssignmentID";

            var parameters = new[]
            {
                _dbHelper.CreateParameter("@AssignmentID", assignmentId),
                _dbHelper.CreateParameter("@CompletionDate", completionDate),
                _dbHelper.CreateParameter("@Certificate", certificate),
                _dbHelper.CreateParameter("@RefresherYears", refresherYears)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateReminderSentAsync(int assignmentId)
        {
            const string sql = @"
                UPDATE TrainingAssignments 
                SET ReminderSentDate = @ReminderSentDate,
                    ReminderCount = ReminderCount + 1
                WHERE AssignmentID = @AssignmentID";

            var parameters = new[]
            {
                _dbHelper.CreateParameter("@AssignmentID", assignmentId),
                _dbHelper.CreateParameter("@ReminderSentDate", DateTime.Now)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> MarkOverdueAsync(List<int> assignmentIds)
        {
            if (!assignmentIds.Any()) return true;

            var sql = $"UPDATE TrainingAssignments SET Status = 'Overdue' WHERE AssignmentID IN ({string.Join(",", assignmentIds)})";
            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAssignmentAsync(int assignmentId)
        {
            const string sql = "DELETE FROM TrainingAssignments WHERE AssignmentID = @AssignmentID";
            
            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql,
                _dbHelper.CreateParameter("@AssignmentID", assignmentId));
            
            return rowsAffected > 0;
        }

        private static TrainingAssignment MapTrainingAssignment(SqlDataReader reader)
        {
            return new TrainingAssignment
            {
                AssignmentID = SafeConverter.GetInt(reader, "AssignmentID"),
                UserID = SafeConverter.GetInt(reader, "UserID"),
                TrainingType = SafeConverter.GetString(reader, "TrainingType"),
                AssignedDate = reader.GetDateTime("AssignedDate"),
                Status = SafeConverter.GetString(reader, "Status"),
                CompletionDate = SafeConverter.GetNullableDateTime(reader, "CompletionDate"),
                NextDueDate = SafeConverter.GetNullableDateTime(reader, "NextDueDate"),
                CompletionCertificate = SafeConverter.GetString(reader, "CompletionCertificate"),
                Notes = SafeConverter.GetString(reader, "Notes"),
                AssignedBy = SafeConverter.GetNullableInt(reader, "AssignedBy"),
                ReminderSentDate = SafeConverter.GetNullableDateTime(reader, "ReminderSentDate"),
                ReminderCount = SafeConverter.GetInt(reader, "ReminderCount"),
                IsRefresher = SafeConverter.GetBool(reader, "IsRefresher")
            };
        }
    }
} 