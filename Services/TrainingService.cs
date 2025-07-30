using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SCTMS.Data;
using SCTMS.Models;
using SCTMS.Utilities;

namespace SCTMS.Services
{
    public class TrainingService
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly UserRepository _userRepository;
        private readonly TrainingAssignmentRepository _trainingRepository;
        private readonly NotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TrainingService> _logger;

        public TrainingService(
            DatabaseHelper dbHelper,
            UserRepository userRepository,
            TrainingAssignmentRepository trainingRepository,
            NotificationService notificationService,
            IConfiguration configuration,
            ILogger<TrainingService> logger)
        {
            _dbHelper = dbHelper;
            _userRepository = userRepository;
            _trainingRepository = trainingRepository;
            _notificationService = notificationService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<int> AssignTrainingToUserAsync(int userId, string trainingType, int assignedBy, bool isRefresher = false)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for training assignment", userId);
                    return 0;
                }

                var assignment = new TrainingAssignment
                {
                    UserID = userId,
                    TrainingType = trainingType,
                    AssignedDate = DateTime.Now,
                    Status = "Assigned",
                    AssignedBy = assignedBy,
                    IsRefresher = isRefresher
                };

                var assignmentId = await _trainingRepository.CreateAssignmentAsync(assignment);
                
                if (assignmentId > 0)
                {
                    // Send initial notification
                    assignment.AssignmentID = assignmentId;
                    await _notificationService.SendTrainingReminderAsync(user, assignment);
                    
                    _logger.LogInformation("Training {TrainingType} assigned to user {UserId} by {AssignedBy}", 
                        trainingType, userId, assignedBy);
                }

                return assignmentId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning training {TrainingType} to user {UserId}", trainingType, userId);
                return 0;
            }
        }

        public async Task<bool> CompleteTrainingAsync(int assignmentId, string certificate, DateTime completionDate)
        {
            try
            {
                var refresherYears = SafeConverter.ToInt(_configuration["AppSettings:RefresherCycleYears"], 3);
                var success = await _trainingRepository.CompleteAssignmentAsync(assignmentId, certificate, completionDate, refresherYears);
                
                if (success)
                {
                    _logger.LogInformation("Training assignment {AssignmentId} completed on {CompletionDate}", 
                        assignmentId, completionDate);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing training assignment {AssignmentId}", assignmentId);
                return false;
            }
        }

        public async Task<int> ProcessAutomaticRemindersAsync()
        {
            try
            {
                var reminderInterval = SafeConverter.ToInt(_configuration["AppSettings:ReminderIntervalDays"], 10);
                var assignments = await _trainingRepository.GetDueForReminderAsync(reminderInterval);
                
                int remindersSent = 0;
                
                foreach (var assignment in assignments)
                {
                    var user = await _userRepository.GetUserByIdAsync(assignment.UserID);
                    if (user != null)
                    {
                        var success = await _notificationService.SendTrainingReminderAsync(user, assignment);
                        if (success)
                        {
                            await _trainingRepository.UpdateReminderSentAsync(assignment.AssignmentID);
                            remindersSent++;
                        }
                    }
                }

                _logger.LogInformation("Processed automatic reminders. {Count} reminders sent", remindersSent);
                return remindersSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing automatic reminders");
                return 0;
            }
        }

        public async Task<int> CreateRefresherTrainingsAsync()
        {
            try
            {
                var refresherYears = SafeConverter.ToInt(_configuration["AppSettings:RefresherCycleYears"], 3);
                var assignments = await _trainingRepository.GetDueForRefresherAsync(refresherYears);
                
                int refreshersCreated = 0;
                
                foreach (var oldAssignment in assignments)
                {
                    // Create new refresher assignment
                    var newAssignmentId = await AssignTrainingToUserAsync(
                        oldAssignment.UserID, 
                        oldAssignment.TrainingType, 
                        oldAssignment.AssignedBy ?? 1, // Default to system user
                        true); // Mark as refresher
                    
                    if (newAssignmentId > 0)
                    {
                        refreshersCreated++;
                    }
                }

                _logger.LogInformation("Created {Count} refresher training assignments", refreshersCreated);
                return refreshersCreated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refresher trainings");
                return 0;
            }
        }

        public async Task<int> MarkOverdueTrainingsAsync()
        {
            try
            {
                var nonComplianceDays = SafeConverter.ToInt(_configuration["AppSettings:NonComplianceDays"], 60);
                var overdueAssignments = await _trainingRepository.GetOverdueAssignmentsAsync();
                
                var overdueIds = overdueAssignments
                    .Where(a => (DateTime.Now - a.AssignedDate).Days > nonComplianceDays && a.Status != "Overdue")
                    .Select(a => a.AssignmentID)
                    .ToList();

                if (overdueIds.Any())
                {
                    await _trainingRepository.MarkOverdueAsync(overdueIds);
                    _logger.LogInformation("Marked {Count} training assignments as overdue", overdueIds.Count);
                }

                return overdueIds.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking overdue trainings");
                return 0;
            }
        }

        public async Task<List<TrainingAssignment>> GetUserTrainingHistoryAsync(int userId)
        {
            try
            {
                return await _trainingRepository.GetAssignmentsByUserAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting training history for user {UserId}", userId);
                return new List<TrainingAssignment>();
            }
        }

        public async Task<TrainingSummary> GetTrainingSummaryAsync()
        {
            try
            {
                const string sql = @"
                    SELECT 
                        COUNT(*) as TotalAssignments,
                        COUNT(CASE WHEN Status = 'Completed' THEN 1 END) as CompletedAssignments,
                        COUNT(CASE WHEN Status IN ('Assigned', 'InProgress') THEN 1 END) as PendingAssignments,
                        COUNT(CASE WHEN Status = 'Overdue' THEN 1 END) as OverdueAssignments,
                        COUNT(CASE WHEN IsRefresher = 1 THEN 1 END) as RefresherAssignments,
                        COUNT(CASE WHEN TrainingType = 'TwoWheeler' THEN 1 END) as TwoWheelerTrainings,
                        COUNT(CASE WHEN TrainingType = 'FourWheeler' THEN 1 END) as FourWheelerTrainings,
                        COUNT(CASE WHEN TrainingType = 'Mandatory' THEN 1 END) as MandatoryTrainings
                    FROM TrainingAssignments";

                var result = await _dbHelper.ExecuteReaderAsync(sql, reader => new TrainingSummary
                {
                    TotalAssignments = reader.GetInt32(0),
                    CompletedAssignments = reader.GetInt32(1),
                    PendingAssignments = reader.GetInt32(2),
                    OverdueAssignments = reader.GetInt32(3),
                    RefresherAssignments = reader.GetInt32(4),
                    TwoWheelerTrainings = reader.GetInt32(5),
                    FourWheelerTrainings = reader.GetInt32(6),
                    MandatoryTrainings = reader.GetInt32(7)
                });

                return result.FirstOrDefault() ?? new TrainingSummary();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting training summary");
                return new TrainingSummary();
            }
        }

        public async Task<bool> BulkAssignTrainingAsync(string trainingType, List<int> userIds, int assignedBy)
        {
            try
            {
                int successCount = 0;
                
                foreach (var userId in userIds)
                {
                    var assignmentId = await AssignTrainingToUserAsync(userId, trainingType, assignedBy);
                    if (assignmentId > 0)
                    {
                        successCount++;
                    }
                }

                _logger.LogInformation("Bulk assignment completed. {SuccessCount}/{TotalCount} assignments created", 
                    successCount, userIds.Count);
                
                return successCount == userIds.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk training assignment");
                return false;
            }
        }

        public async Task<List<TrainingReport>> GetTrainingReportByDepartmentAsync()
        {
            try
            {
                const string sql = @"
                    SELECT 
                        u.Department,
                        ta.TrainingType,
                        COUNT(*) as TotalAssignments,
                        COUNT(CASE WHEN ta.Status = 'Completed' THEN 1 END) as CompletedCount,
                        COUNT(CASE WHEN ta.Status IN ('Assigned', 'InProgress') THEN 1 END) as PendingCount,
                        COUNT(CASE WHEN ta.Status = 'Overdue' THEN 1 END) as OverdueCount,
                        AVG(CASE WHEN ta.Status = 'Completed' THEN DATEDIFF(day, ta.AssignedDate, ta.CompletionDate) END) as AvgCompletionDays
                    FROM TrainingAssignments ta
                    INNER JOIN Users u ON ta.UserID = u.UserID
                    WHERE u.Status = 'Active'
                    GROUP BY u.Department, ta.TrainingType
                    ORDER BY u.Department, ta.TrainingType";

                return await _dbHelper.ExecuteReaderAsync(sql, reader => new TrainingReport
                {
                    Department = reader.GetString("Department"),
                    TrainingType = reader.GetString("TrainingType"),
                    TotalAssignments = reader.GetInt32("TotalAssignments"),
                    CompletedCount = reader.GetInt32("CompletedCount"),
                    PendingCount = reader.GetInt32("PendingCount"),
                    OverdueCount = reader.GetInt32("OverdueCount"),
                    AverageCompletionDays = reader.IsDBNull("AvgCompletionDays") ? 0 : reader.GetDouble("AvgCompletionDays")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting training report by department");
                return new List<TrainingReport>();
            }
        }
    }

    public class TrainingSummary
    {
        public int TotalAssignments { get; set; }
        public int CompletedAssignments { get; set; }
        public int PendingAssignments { get; set; }
        public int OverdueAssignments { get; set; }
        public int RefresherAssignments { get; set; }
        public int TwoWheelerTrainings { get; set; }
        public int FourWheelerTrainings { get; set; }
        public int MandatoryTrainings { get; set; }
        public decimal CompletionRate => TotalAssignments > 0 ? ((decimal)CompletedAssignments / TotalAssignments) * 100 : 0;
    }

    public class TrainingReport
    {
        public string Department { get; set; } = string.Empty;
        public string TrainingType { get; set; } = string.Empty;
        public int TotalAssignments { get; set; }
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }
        public int OverdueCount { get; set; }
        public double AverageCompletionDays { get; set; }
        public decimal CompletionPercentage => TotalAssignments > 0 ? ((decimal)CompletedCount / TotalAssignments) * 100 : 0;
    }
} 