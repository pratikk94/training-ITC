using ClosedXML.Excel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SCTMS.Data;
using SCTMS.Models;
using System.Data;

namespace SCTMS.Services
{
    public class ReportingService
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly UserRepository _userRepository;
        private readonly TrainingAssignmentRepository _trainingRepository;
        private readonly ComplianceService _complianceService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReportingService> _logger;

        public ReportingService(
            DatabaseHelper dbHelper,
            UserRepository userRepository,
            TrainingAssignmentRepository trainingRepository,
            ComplianceService complianceService,
            IConfiguration configuration,
            ILogger<ReportingService> logger)
        {
            _dbHelper = dbHelper;
            _userRepository = userRepository;
            _trainingRepository = trainingRepository;
            _complianceService = complianceService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GenerateComplianceReportAsync(string filePath)
        {
            try
            {
                var workbook = new XLWorkbook();

                // Sheet 1: Overall Compliance Summary
                await CreateComplianceSummarySheet(workbook);

                // Sheet 2: Department-wise Compliance
                await CreateDepartmentComplianceSheet(workbook);

                // Sheet 3: User Compliance Details
                await CreateUserComplianceSheet(workbook);

                // Sheet 4: Training Assignment Details
                await CreateTrainingAssignmentSheet(workbook);

                workbook.SaveAs(filePath);
                _logger.LogInformation("Compliance report generated: {FilePath}", filePath);
                
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating compliance report");
                throw;
            }
        }

        public async Task<string> GenerateTrainingReportAsync(string filePath, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var workbook = new XLWorkbook();

                // Sheet 1: Training Summary
                await CreateTrainingSummarySheet(workbook, fromDate, toDate);

                // Sheet 2: Training by Department
                await CreateTrainingByDepartmentSheet(workbook, fromDate, toDate);

                // Sheet 3: Training Completion Details
                await CreateTrainingCompletionSheet(workbook, fromDate, toDate);

                // Sheet 4: Overdue Trainings
                await CreateOverdueTrainingSheet(workbook);

                workbook.SaveAs(filePath);
                _logger.LogInformation("Training report generated: {FilePath}", filePath);
                
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating training report");
                throw;
            }
        }

        public async Task<string> GenerateUserReportAsync(string filePath)
        {
            try
            {
                var workbook = new XLWorkbook();

                // Sheet 1: User Summary
                await CreateUserSummarySheet(workbook);

                // Sheet 2: User Details
                await CreateUserDetailsSheet(workbook);

                // Sheet 3: Blocked Users
                await CreateBlockedUsersSheet(workbook);

                workbook.SaveAs(filePath);
                _logger.LogInformation("User report generated: {FilePath}", filePath);
                
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating user report");
                throw;
            }
        }

        private async Task CreateComplianceSummarySheet(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Compliance Summary");
            
            // Headers
            worksheet.Cell(1, 1).Value = "Compliance Summary Report";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            
            worksheet.Cell(3, 1).Value = "Generated On:";
            worksheet.Cell(3, 2).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            
            // Get compliance summary
            var summary = await _complianceService.GetComplianceSummaryAsync();
            
            // Summary data
            var row = 5;
            worksheet.Cell(row, 1).Value = "Metric";
            worksheet.Cell(row, 2).Value = "Count";
            worksheet.Range(row, 1, row, 2).Style.Font.Bold = true;
            worksheet.Range(row, 1, row, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
            
            row++;
            worksheet.Cell(row, 1).Value = "Total Users";
            worksheet.Cell(row, 2).Value = summary.TotalUsers;
            
            row++;
            worksheet.Cell(row, 1).Value = "Active Users";
            worksheet.Cell(row, 2).Value = summary.ActiveUsers;
            
            row++;
            worksheet.Cell(row, 1).Value = "Non-Compliant Users";
            worksheet.Cell(row, 2).Value = summary.NonCompliantUsers;
            
            row++;
            worksheet.Cell(row, 1).Value = "Blocked Users";
            worksheet.Cell(row, 2).Value = summary.BlockedUsers;
            
            row++;
            worksheet.Cell(row, 1).Value = "Users with Overdue Training";
            worksheet.Cell(row, 2).Value = summary.UsersWithOverdueTraining;
            
            row++;
            worksheet.Cell(row, 1).Value = "Overall Compliance %";
            worksheet.Cell(row, 2).Value = $"{summary.CompliancePercentage:F2}%";
            
            worksheet.Columns().AdjustToContents();
        }

        private async Task CreateDepartmentComplianceSheet(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Department Compliance");
            
            var reports = await _complianceService.GetDepartmentComplianceReportAsync();
            
            // Headers
            worksheet.Cell(1, 1).Value = "Department";
            worksheet.Cell(1, 2).Value = "Total Users";
            worksheet.Cell(1, 3).Value = "Active Users";
            worksheet.Cell(1, 4).Value = "Non-Compliant";
            worksheet.Cell(1, 5).Value = "Blocked Users";
            worksheet.Cell(1, 6).Value = "Completed Trainings";
            worksheet.Cell(1, 7).Value = "Pending Trainings";
            worksheet.Cell(1, 8).Value = "Overdue Trainings";
            worksheet.Cell(1, 9).Value = "Compliance %";
            
            var headerRange = worksheet.Range(1, 1, 1, 9);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            
            // Data
            var row = 2;
            foreach (var report in reports)
            {
                worksheet.Cell(row, 1).Value = report.Department;
                worksheet.Cell(row, 2).Value = report.TotalUsers;
                worksheet.Cell(row, 3).Value = report.ActiveUsers;
                worksheet.Cell(row, 4).Value = report.NonCompliantUsers;
                worksheet.Cell(row, 5).Value = report.BlockedUsers;
                worksheet.Cell(row, 6).Value = report.CompletedTrainings;
                worksheet.Cell(row, 7).Value = report.PendingTrainings;
                worksheet.Cell(row, 8).Value = report.OverdueTrainings;
                worksheet.Cell(row, 9).Value = $"{report.CompliancePercentage:F2}%";
                row++;
            }
            
            worksheet.Columns().AdjustToContents();
        }

        private async Task CreateUserComplianceSheet(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("User Compliance");
            
            const string sql = @"
                SELECT u.Name, u.EmployeeID, u.Department, u.Status, u.Role,
                       COUNT(ta.AssignmentID) as TotalAssignments,
                       COUNT(CASE WHEN ta.Status = 'Completed' THEN 1 END) as CompletedAssignments,
                       COUNT(CASE WHEN ta.Status IN ('Assigned', 'InProgress') THEN 1 END) as PendingAssignments,
                       COUNT(CASE WHEN ta.Status = 'Overdue' THEN 1 END) as OverdueAssignments,
                       MAX(ta.CompletionDate) as LastCompletionDate,
                       la.IsBlocked
                FROM Users u
                LEFT JOIN TrainingAssignments ta ON u.UserID = ta.UserID
                LEFT JOIN LoginAccess la ON u.UserID = la.UserID
                WHERE u.Status = 'Active'
                GROUP BY u.UserID, u.Name, u.EmployeeID, u.Department, u.Status, u.Role, la.IsBlocked
                ORDER BY u.Department, u.Name";
            
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql);
            
            // Headers
            var headers = new[] { "Name", "Employee ID", "Department", "Status", "Role", 
                                "Total Assignments", "Completed", "Pending", "Overdue", 
                                "Last Completion", "Blocked" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }
            
            var headerRange = worksheet.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            
            // Data
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = i + 2;
                var dataRow = dataTable.Rows[i];
                
                worksheet.Cell(row, 1).Value = dataRow["Name"].ToString();
                worksheet.Cell(row, 2).Value = dataRow["EmployeeID"].ToString();
                worksheet.Cell(row, 3).Value = dataRow["Department"].ToString();
                worksheet.Cell(row, 4).Value = dataRow["Status"].ToString();
                worksheet.Cell(row, 5).Value = dataRow["Role"].ToString();
                worksheet.Cell(row, 6).Value = Convert.ToInt32(dataRow["TotalAssignments"]);
                worksheet.Cell(row, 7).Value = Convert.ToInt32(dataRow["CompletedAssignments"]);
                worksheet.Cell(row, 8).Value = Convert.ToInt32(dataRow["PendingAssignments"]);
                worksheet.Cell(row, 9).Value = Convert.ToInt32(dataRow["OverdueAssignments"]);
                worksheet.Cell(row, 10).Value = dataRow["LastCompletionDate"] != DBNull.Value ? 
                    Convert.ToDateTime(dataRow["LastCompletionDate"]).ToString("dd/MM/yyyy") : "";
                worksheet.Cell(row, 11).Value = dataRow["IsBlocked"] != DBNull.Value && 
                    Convert.ToBoolean(dataRow["IsBlocked"]) ? "Yes" : "No";
                
                // Color code based on compliance
                var overdueCount = Convert.ToInt32(dataRow["OverdueAssignments"]);
                if (overdueCount > 0)
                {
                    worksheet.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.LightPink;
                }
            }
            
            worksheet.Columns().AdjustToContents();
        }

        private async Task CreateTrainingAssignmentSheet(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Training Assignments");
            
            const string sql = @"
                SELECT u.Name, u.EmployeeID, u.Department, ta.TrainingType, 
                       ta.AssignedDate, ta.Status, ta.CompletionDate, ta.IsRefresher,
                       ab.Name as AssignedBy, ta.ReminderCount
                FROM TrainingAssignments ta
                INNER JOIN Users u ON ta.UserID = u.UserID
                LEFT JOIN Users ab ON ta.AssignedBy = ab.UserID
                ORDER BY ta.AssignedDate DESC";
            
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql);
            
            // Headers
            var headers = new[] { "Employee Name", "Employee ID", "Department", "Training Type", 
                                "Assigned Date", "Status", "Completion Date", "Is Refresher", 
                                "Assigned By", "Reminder Count" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }
            
            var headerRange = worksheet.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            
            // Data
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = i + 2;
                var dataRow = dataTable.Rows[i];
                
                worksheet.Cell(row, 1).Value = dataRow["Name"].ToString();
                worksheet.Cell(row, 2).Value = dataRow["EmployeeID"].ToString();
                worksheet.Cell(row, 3).Value = dataRow["Department"].ToString();
                worksheet.Cell(row, 4).Value = dataRow["TrainingType"].ToString();
                worksheet.Cell(row, 5).Value = Convert.ToDateTime(dataRow["AssignedDate"]).ToString("dd/MM/yyyy");
                worksheet.Cell(row, 6).Value = dataRow["Status"].ToString();
                worksheet.Cell(row, 7).Value = dataRow["CompletionDate"] != DBNull.Value ? 
                    Convert.ToDateTime(dataRow["CompletionDate"]).ToString("dd/MM/yyyy") : "";
                worksheet.Cell(row, 8).Value = Convert.ToBoolean(dataRow["IsRefresher"]) ? "Yes" : "No";
                worksheet.Cell(row, 9).Value = dataRow["AssignedBy"].ToString();
                worksheet.Cell(row, 10).Value = Convert.ToInt32(dataRow["ReminderCount"]);
                
                // Color code based on status
                var status = dataRow["Status"].ToString();
                if (status == "Overdue")
                {
                    worksheet.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.LightPink;
                }
                else if (status == "Completed")
                {
                    worksheet.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.LightGreen;
                }
            }
            
            worksheet.Columns().AdjustToContents();
        }

        private async Task CreateTrainingSummarySheet(XLWorkbook workbook, DateTime fromDate, DateTime toDate)
        {
            var worksheet = workbook.Worksheets.Add("Training Summary");
            
            worksheet.Cell(1, 1).Value = "Training Summary Report";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            
            worksheet.Cell(3, 1).Value = "Period:";
            worksheet.Cell(3, 2).Value = $"{fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}";
            
            const string sql = @"
                SELECT TrainingType,
                       COUNT(*) as TotalAssignments,
                       COUNT(CASE WHEN Status = 'Completed' THEN 1 END) as CompletedCount,
                       COUNT(CASE WHEN Status IN ('Assigned', 'InProgress') THEN 1 END) as PendingCount,
                       COUNT(CASE WHEN Status = 'Overdue' THEN 1 END) as OverdueCount
                FROM TrainingAssignments
                WHERE AssignedDate BETWEEN @FromDate AND @ToDate
                GROUP BY TrainingType";
            
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql,
                _dbHelper.CreateParameter("@FromDate", fromDate),
                _dbHelper.CreateParameter("@ToDate", toDate));
            
            var row = 5;
            worksheet.Cell(row, 1).Value = "Training Type";
            worksheet.Cell(row, 2).Value = "Total";
            worksheet.Cell(row, 3).Value = "Completed";
            worksheet.Cell(row, 4).Value = "Pending";
            worksheet.Cell(row, 5).Value = "Overdue";
            worksheet.Cell(row, 6).Value = "Completion %";
            
            var headerRange = worksheet.Range(row, 1, row, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            
            row++;
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var total = Convert.ToInt32(dataRow["TotalAssignments"]);
                var completed = Convert.ToInt32(dataRow["CompletedCount"]);
                var completionRate = total > 0 ? (decimal)completed / total * 100 : 0;
                
                worksheet.Cell(row, 1).Value = dataRow["TrainingType"].ToString();
                worksheet.Cell(row, 2).Value = total;
                worksheet.Cell(row, 3).Value = completed;
                worksheet.Cell(row, 4).Value = Convert.ToInt32(dataRow["PendingCount"]);
                worksheet.Cell(row, 5).Value = Convert.ToInt32(dataRow["OverdueCount"]);
                worksheet.Cell(row, 6).Value = $"{completionRate:F2}%";
                row++;
            }
            
            worksheet.Columns().AdjustToContents();
        }

        private async Task CreateTrainingByDepartmentSheet(XLWorkbook workbook, DateTime fromDate, DateTime toDate)
        {
            var worksheet = workbook.Worksheets.Add("Training by Department");
            
            const string sql = @"
                SELECT u.Department, ta.TrainingType,
                       COUNT(*) as TotalAssignments,
                       COUNT(CASE WHEN ta.Status = 'Completed' THEN 1 END) as CompletedCount,
                       AVG(CASE WHEN ta.Status = 'Completed' THEN DATEDIFF(day, ta.AssignedDate, ta.CompletionDate) END) as AvgDays
                FROM TrainingAssignments ta
                INNER JOIN Users u ON ta.UserID = u.UserID
                WHERE ta.AssignedDate BETWEEN @FromDate AND @ToDate
                GROUP BY u.Department, ta.TrainingType
                ORDER BY u.Department, ta.TrainingType";
            
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql,
                _dbHelper.CreateParameter("@FromDate", fromDate),
                _dbHelper.CreateParameter("@ToDate", toDate));
            
            // Headers
            var headers = new[] { "Department", "Training Type", "Total Assignments", 
                                "Completed", "Avg. Completion Days" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }
            
            var headerRange = worksheet.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            
            // Data
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = i + 2;
                var dataRow = dataTable.Rows[i];
                
                worksheet.Cell(row, 1).Value = dataRow["Department"].ToString();
                worksheet.Cell(row, 2).Value = dataRow["TrainingType"].ToString();
                worksheet.Cell(row, 3).Value = Convert.ToInt32(dataRow["TotalAssignments"]);
                worksheet.Cell(row, 4).Value = Convert.ToInt32(dataRow["CompletedCount"]);
                worksheet.Cell(row, 5).Value = dataRow["AvgDays"] != DBNull.Value ? 
                    $"{Convert.ToDouble(dataRow["AvgDays"]):F1}" : "N/A";
            }
            
            worksheet.Columns().AdjustToContents();
        }

        private async Task CreateTrainingCompletionSheet(XLWorkbook workbook, DateTime fromDate, DateTime toDate)
        {
            var worksheet = workbook.Worksheets.Add("Completed Trainings");
            
            const string sql = @"
                SELECT u.Name, u.EmployeeID, u.Department, ta.TrainingType,
                       ta.AssignedDate, ta.CompletionDate, ta.CompletionCertificate,
                       DATEDIFF(day, ta.AssignedDate, ta.CompletionDate) as CompletionDays
                FROM TrainingAssignments ta
                INNER JOIN Users u ON ta.UserID = u.UserID
                WHERE ta.Status = 'Completed' 
                  AND ta.CompletionDate BETWEEN @FromDate AND @ToDate
                ORDER BY ta.CompletionDate DESC";
            
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql,
                _dbHelper.CreateParameter("@FromDate", fromDate),
                _dbHelper.CreateParameter("@ToDate", toDate));
            
            // Headers
            var headers = new[] { "Employee Name", "Employee ID", "Department", "Training Type", 
                                "Assigned Date", "Completion Date", "Certificate", "Days to Complete" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }
            
            var headerRange = worksheet.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            
            // Data
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = i + 2;
                var dataRow = dataTable.Rows[i];
                
                worksheet.Cell(row, 1).Value = dataRow["Name"].ToString();
                worksheet.Cell(row, 2).Value = dataRow["EmployeeID"].ToString();
                worksheet.Cell(row, 3).Value = dataRow["Department"].ToString();
                worksheet.Cell(row, 4).Value = dataRow["TrainingType"].ToString();
                worksheet.Cell(row, 5).Value = Convert.ToDateTime(dataRow["AssignedDate"]).ToString("dd/MM/yyyy");
                worksheet.Cell(row, 6).Value = Convert.ToDateTime(dataRow["CompletionDate"]).ToString("dd/MM/yyyy");
                worksheet.Cell(row, 7).Value = dataRow["CompletionCertificate"].ToString();
                worksheet.Cell(row, 8).Value = Convert.ToInt32(dataRow["CompletionDays"]);
            }
            
            worksheet.Columns().AdjustToContents();
        }

        private async Task CreateOverdueTrainingSheet(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Overdue Trainings");
            
            const string sql = @"
                SELECT u.Name, u.EmployeeID, u.Department, ta.TrainingType,
                       ta.AssignedDate, ta.Status, ta.ReminderCount,
                       DATEDIFF(day, ta.AssignedDate, GETDATE()) as DaysOverdue,
                       m.Name as ManagerName, m.Email as ManagerEmail
                FROM TrainingAssignments ta
                INNER JOIN Users u ON ta.UserID = u.UserID
                LEFT JOIN Users m ON u.ReportingManagerID = m.UserID
                WHERE ta.Status IN ('Assigned', 'InProgress', 'Overdue')
                  AND DATEDIFF(day, ta.AssignedDate, GETDATE()) > 60
                ORDER BY DATEDIFF(day, ta.AssignedDate, GETDATE()) DESC";
            
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql);
            
            // Headers
            var headers = new[] { "Employee Name", "Employee ID", "Department", "Training Type", 
                                "Assigned Date", "Status", "Days Overdue", "Reminders Sent", 
                                "Manager Name", "Manager Email" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }
            
            var headerRange = worksheet.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            
            // Data
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = i + 2;
                var dataRow = dataTable.Rows[i];
                
                worksheet.Cell(row, 1).Value = dataRow["Name"].ToString();
                worksheet.Cell(row, 2).Value = dataRow["EmployeeID"].ToString();
                worksheet.Cell(row, 3).Value = dataRow["Department"].ToString();
                worksheet.Cell(row, 4).Value = dataRow["TrainingType"].ToString();
                worksheet.Cell(row, 5).Value = Convert.ToDateTime(dataRow["AssignedDate"]).ToString("dd/MM/yyyy");
                worksheet.Cell(row, 6).Value = dataRow["Status"].ToString();
                worksheet.Cell(row, 7).Value = Convert.ToInt32(dataRow["DaysOverdue"]);
                worksheet.Cell(row, 8).Value = Convert.ToInt32(dataRow["ReminderCount"]);
                worksheet.Cell(row, 9).Value = dataRow["ManagerName"].ToString();
                worksheet.Cell(row, 10).Value = dataRow["ManagerEmail"].ToString();
                
                // Highlight critical overdue items
                var daysOverdue = Convert.ToInt32(dataRow["DaysOverdue"]);
                if (daysOverdue > 90)
                {
                    worksheet.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.Red;
                    worksheet.Range(row, 1, row, headers.Length).Style.Font.FontColor = XLColor.White;
                }
                else if (daysOverdue > 60)
                {
                    worksheet.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.Orange;
                }
            }
            
            worksheet.Columns().AdjustToContents();
        }

        private async Task CreateUserSummarySheet(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("User Summary");
            
            const string sql = @"
                SELECT Department,
                       COUNT(*) as TotalUsers,
                       COUNT(CASE WHEN Status = 'Active' THEN 1 END) as ActiveUsers,
                       COUNT(CASE WHEN Status = 'NonCompliant' THEN 1 END) as NonCompliantUsers,
                       COUNT(CASE WHEN Role = 'Manager' THEN 1 END) as Managers,
                       COUNT(CASE WHEN Role = 'HR' THEN 1 END) as HRUsers,
                       COUNT(CASE WHEN Role = 'Safety' THEN 1 END) as SafetyUsers,
                       COUNT(CASE WHEN Role = 'Admin' THEN 1 END) as AdminUsers
                FROM Users
                GROUP BY Department
                ORDER BY Department";
            
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql);
            
            // Headers
            var headers = new[] { "Department", "Total Users", "Active", "Non-Compliant", 
                                "Managers", "HR", "Safety", "Admin" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }
            
            var headerRange = worksheet.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            
            // Data
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = i + 2;
                var dataRow = dataTable.Rows[i];
                
                for (int j = 0; j < headers.Length; j++)
                {
                    worksheet.Cell(row, j + 1).Value = dataRow[j].ToString();
                }
            }
            
            worksheet.Columns().AdjustToContents();
        }

        private async Task CreateUserDetailsSheet(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("User Details");
            
            var users = await _userRepository.GetAllUsersAsync();
            
            // Headers
            var headers = new[] { "Name", "Employee ID", "Department", "Level", "Status", 
                                "Role", "Email", "Windows Username", "Created Date", "Last Login" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }
            
            var headerRange = worksheet.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            
            // Data
            for (int i = 0; i < users.Count; i++)
            {
                var row = i + 2;
                var user = users[i];
                
                worksheet.Cell(row, 1).Value = user.Name;
                worksheet.Cell(row, 2).Value = user.EmployeeID;
                worksheet.Cell(row, 3).Value = user.Department;
                worksheet.Cell(row, 4).Value = user.Level;
                worksheet.Cell(row, 5).Value = user.Status;
                worksheet.Cell(row, 6).Value = user.Role;
                worksheet.Cell(row, 7).Value = user.Email;
                worksheet.Cell(row, 8).Value = user.WindowsUsername;
                worksheet.Cell(row, 9).Value = user.CreatedDate.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 10).Value = user.LastLoginDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
                
                // Color code by status
                if (user.Status == "NonCompliant")
                {
                    worksheet.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.LightPink;
                }
                else if (user.Status == "Inactive")
                {
                    worksheet.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.LightGray;
                }
            }
            
            worksheet.Columns().AdjustToContents();
        }

        private async Task CreateBlockedUsersSheet(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Blocked Users");
            
            const string sql = @"
                SELECT u.Name, u.EmployeeID, u.Department, la.BlockDate, la.BlockReason,
                       la.UnblockRequestedBy, ur.Name as RequestedByName, la.UnblockRequestDate,
                       la.UnblockApproved, ua.Name as ApprovedByName, la.UnblockApprovedDate
                FROM LoginAccess la
                INNER JOIN Users u ON la.UserID = u.UserID
                LEFT JOIN Users ur ON la.UnblockRequestedBy = ur.UserID
                LEFT JOIN Users ua ON la.UnblockApprovedBy = ua.UserID
                WHERE la.IsBlocked = 1
                ORDER BY la.BlockDate DESC";
            
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql);
            
            // Headers
            var headers = new[] { "Employee Name", "Employee ID", "Department", "Block Date", 
                                "Block Reason", "Unblock Requested By", "Request Date", 
                                "Approved", "Approved By", "Approved Date" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }
            
            var headerRange = worksheet.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            
            // Data
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = i + 2;
                var dataRow = dataTable.Rows[i];
                
                worksheet.Cell(row, 1).Value = dataRow["Name"].ToString();
                worksheet.Cell(row, 2).Value = dataRow["EmployeeID"].ToString();
                worksheet.Cell(row, 3).Value = dataRow["Department"].ToString();
                worksheet.Cell(row, 4).Value = Convert.ToDateTime(dataRow["BlockDate"]).ToString("dd/MM/yyyy");
                worksheet.Cell(row, 5).Value = dataRow["BlockReason"].ToString();
                worksheet.Cell(row, 6).Value = dataRow["RequestedByName"].ToString();
                worksheet.Cell(row, 7).Value = dataRow["UnblockRequestDate"] != DBNull.Value ? 
                    Convert.ToDateTime(dataRow["UnblockRequestDate"]).ToString("dd/MM/yyyy") : "";
                worksheet.Cell(row, 8).Value = Convert.ToBoolean(dataRow["UnblockApproved"]) ? "Yes" : "No";
                worksheet.Cell(row, 9).Value = dataRow["ApprovedByName"].ToString();
                worksheet.Cell(row, 10).Value = dataRow["UnblockApprovedDate"] != DBNull.Value ? 
                    Convert.ToDateTime(dataRow["UnblockApprovedDate"]).ToString("dd/MM/yyyy") : "";
            }
            
            worksheet.Columns().AdjustToContents();
        }
    }
} 