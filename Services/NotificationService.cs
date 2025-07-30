using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SCTMS.Data;
using SCTMS.Models;

namespace SCTMS.Services
{
    public class NotificationService
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly bool _enableSSL;
        private readonly string _adminEmail;

        public NotificationService(DatabaseHelper dbHelper, IConfiguration configuration, ILogger<NotificationService> logger)
        {
            _dbHelper = dbHelper;
            _configuration = configuration;
            _logger = logger;
            _smtpServer = configuration["AppSettings:SMTPServer"] ?? "localhost";
            _smtpPort = int.Parse(configuration["AppSettings:SMTPPort"] ?? "587");
            _enableSSL = bool.Parse(configuration["AppSettings:EnableSSL"] ?? "true");
            _adminEmail = configuration["AppSettings:AdminEmail"] ?? "admin@company.com";
        }

        public async Task<bool> SendTrainingReminderAsync(User user, TrainingAssignment assignment)
        {
            try
            {
                var subject = $"Training Reminder: {assignment.TrainingType}";
                var message = GenerateTrainingReminderMessage(user, assignment);

                var success = await SendEmailAsync(user.Email, subject, message);
                
                if (success)
                {
                    await LogNotificationAsync(user.UserID, NotificationType.Email.ToString(), 
                        subject, message, NotificationStatus.Sent.ToString(), assignment.AssignmentID);
                }
                else
                {
                    await LogNotificationAsync(user.UserID, NotificationType.Email.ToString(), 
                        subject, message, NotificationStatus.Failed.ToString(), assignment.AssignmentID);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending training reminder to {Email}", user.Email);
                return false;
            }
        }

        public async Task<bool> SendComplianceAlertAsync(User user, List<TrainingAssignment> overdueAssignments)
        {
            try
            {
                var subject = "URGENT: Safety Training Compliance Alert";
                var message = GenerateComplianceAlertMessage(user, overdueAssignments);

                var success = await SendEmailAsync(user.Email, subject, message);
                
                if (success)
                {
                    await LogNotificationAsync(user.UserID, NotificationType.Email.ToString(), 
                        subject, message, NotificationStatus.Sent.ToString());
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending compliance alert to {Email}", user.Email);
                return false;
            }
        }

        public async Task<bool> SendManagerEscalationAsync(User manager, User employee, List<TrainingAssignment> overdueAssignments)
        {
            try
            {
                var subject = $"Manager Escalation: {employee.Name} - Training Non-Compliance";
                var message = GenerateManagerEscalationMessage(manager, employee, overdueAssignments);

                var success = await SendEmailAsync(manager.Email, subject, message);
                
                if (success)
                {
                    await LogNotificationAsync(manager.UserID, NotificationType.Email.ToString(), 
                        subject, message, NotificationStatus.Sent.ToString());
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending manager escalation to {Email}", manager.Email);
                return false;
            }
        }

        public async Task<bool> SendLoginBlockNotificationAsync(User user, string reason)
        {
            try
            {
                var subject = "Account Access Blocked - Training Non-Compliance";
                var message = GenerateLoginBlockMessage(user, reason);

                var success = await SendEmailAsync(user.Email, subject, message);
                
                if (success)
                {
                    await LogNotificationAsync(user.UserID, NotificationType.Email.ToString(), 
                        subject, message, NotificationStatus.Sent.ToString());
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending login block notification to {Email}", user.Email);
                return false;
            }
        }

        public async Task<bool> SendUnblockRequestNotificationAsync(User requestingManager, User blockedUser)
        {
            try
            {
                var subject = $"Unblock Request: {blockedUser.Name} - Manager Approval Required";
                var message = GenerateUnblockRequestMessage(requestingManager, blockedUser);

                var success = await SendEmailAsync(_adminEmail, subject, message);
                
                if (success)
                {
                    await LogNotificationAsync(requestingManager.UserID, NotificationType.Email.ToString(), 
                        subject, message, NotificationStatus.Sent.ToString());
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending unblock request notification");
                return false;
            }
        }

        public void ShowPopupNotification(string title, string message, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            try
            {
                MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing popup notification");
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_smtpServer, _smtpPort)
                {
                    EnableSsl = _enableSSL,
                    UseDefaultCredentials = true
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_adminEmail, "SCTMS - Safety Training System"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                return false;
            }
        }

        private async Task LogNotificationAsync(int userId, string type, string subject, string message, string status, int? assignmentId = null)
        {
            const string sql = @"
                INSERT INTO NotificationsLog (UserID, Type, Subject, Message, Timestamp, DeferStatus, 
                                            NotificationCategory, RelatedAssignmentID, DeliveryStatus)
                VALUES (@UserID, @Type, @Subject, @Message, @Timestamp, @DeferStatus, 
                        @NotificationCategory, @RelatedAssignmentID, @DeliveryStatus)";

            var parameters = new[]
            {
                _dbHelper.CreateParameter("@UserID", userId),
                _dbHelper.CreateParameter("@Type", type),
                _dbHelper.CreateParameter("@Subject", subject),
                _dbHelper.CreateParameter("@Message", message),
                _dbHelper.CreateParameter("@Timestamp", DateTime.Now),
                _dbHelper.CreateParameter("@DeferStatus", status),
                _dbHelper.CreateParameter("@NotificationCategory", NotificationCategory.Training.ToString()),
                _dbHelper.CreateParameter("@RelatedAssignmentID", assignmentId),
                _dbHelper.CreateParameter("@DeliveryStatus", status)
            };

            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
        }

        private string GenerateTrainingReminderMessage(User user, TrainingAssignment assignment)
        {
            var daysOverdue = (DateTime.Now - assignment.AssignedDate).Days;
            
            return $@"
                <html>
                <body>
                    <h2>Safety Training Reminder</h2>
                    <p>Dear {user.Name},</p>
                    
                    <p>This is a reminder that you have a pending safety training assignment:</p>
                    
                    <table border='1' style='border-collapse: collapse; width: 100%;'>
                        <tr><td><strong>Training Type:</strong></td><td>{assignment.TrainingType}</td></tr>
                        <tr><td><strong>Assigned Date:</strong></td><td>{assignment.AssignedDate:dd/MM/yyyy}</td></tr>
                        <tr><td><strong>Days Pending:</strong></td><td>{daysOverdue} days</td></tr>
                        <tr><td><strong>Status:</strong></td><td>{assignment.Status}</td></tr>
                    </table>
                    
                    <p><strong>Please complete this training immediately to maintain compliance.</strong></p>
                    
                    <p>For questions, contact your manager or the HR department.</p>
                    
                    <p>Best regards,<br/>
                    Safety Training Management System<br/>
                    {_configuration["AppSettings:CompanyName"]}</p>
                </body>
                </html>";
        }

        private string GenerateComplianceAlertMessage(User user, List<TrainingAssignment> overdueAssignments)
        {
            var trainingList = string.Join("", overdueAssignments.Select(a => 
                $"<tr><td>{a.TrainingType}</td><td>{a.AssignedDate:dd/MM/yyyy}</td><td>{(DateTime.Now - a.AssignedDate).Days} days</td></tr>"));

            return $@"
                <html>
                <body>
                    <h2 style='color: red;'>URGENT: Safety Training Compliance Alert</h2>
                    <p>Dear {user.Name},</p>
                    
                    <p><strong style='color: red;'>You are currently non-compliant with mandatory safety training requirements.</strong></p>
                    
                    <p>Overdue Training Assignments:</p>
                    <table border='1' style='border-collapse: collapse; width: 100%;'>
                        <tr style='background-color: #f0f0f0;'>
                            <th>Training Type</th>
                            <th>Assigned Date</th>
                            <th>Days Overdue</th>
                        </tr>
                        {trainingList}
                    </table>
                    
                    <p><strong style='color: red;'>WARNING: Failure to complete these trainings may result in system access restrictions.</strong></p>
                    
                    <p>Please complete all pending trainings immediately.</p>
                    
                    <p>Best regards,<br/>
                    Safety Training Management System<br/>
                    {_configuration["AppSettings:CompanyName"]}</p>
                </body>
                </html>";
        }

        private string GenerateManagerEscalationMessage(User manager, User employee, List<TrainingAssignment> overdueAssignments)
        {
            var trainingList = string.Join("", overdueAssignments.Select(a => 
                $"<tr><td>{a.TrainingType}</td><td>{a.AssignedDate:dd/MM/yyyy}</td><td>{(DateTime.Now - a.AssignedDate).Days} days</td></tr>"));

            return $@"
                <html>
                <body>
                    <h2>Manager Escalation: Training Non-Compliance</h2>
                    <p>Dear {manager.Name},</p>
                    
                    <p>This is to inform you that your team member <strong>{employee.Name}</strong> (Employee ID: {employee.EmployeeID}) 
                    is non-compliant with mandatory safety training requirements.</p>
                    
                    <p>Overdue Training Assignments:</p>
                    <table border='1' style='border-collapse: collapse; width: 100%;'>
                        <tr style='background-color: #f0f0f0;'>
                            <th>Training Type</th>
                            <th>Assigned Date</th>
                            <th>Days Overdue</th>
                        </tr>
                        {trainingList}
                    </table>
                    
                    <p><strong>Please take immediate action to ensure compliance.</strong></p>
                    
                    <p>You can use the SCTMS system to track progress and send additional reminders.</p>
                    
                    <p>Best regards,<br/>
                    Safety Training Management System<br/>
                    {_configuration["AppSettings:CompanyName"]}</p>
                </body>
                </html>";
        }

        private string GenerateLoginBlockMessage(User user, string reason)
        {
            return $@"
                <html>
                <body>
                    <h2 style='color: red;'>Account Access Blocked</h2>
                    <p>Dear {user.Name},</p>
                    
                    <p><strong style='color: red;'>Your system access has been blocked due to training non-compliance.</strong></p>
                    
                    <p><strong>Reason:</strong> {reason}</p>
                    <p><strong>Block Date:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                    
                    <p>To regain access, please:</p>
                    <ol>
                        <li>Complete all pending safety training assignments</li>
                        <li>Contact your manager to request account unblock</li>
                        <li>Wait for approval from the system administrator</li>
                    </ol>
                    
                    <p>For immediate assistance, contact your reporting manager or HR department.</p>
                    
                    <p>Best regards,<br/>
                    Safety Training Management System<br/>
                    {_configuration["AppSettings:CompanyName"]}</p>
                </body>
                </html>";
        }

        private string GenerateUnblockRequestMessage(User requestingManager, User blockedUser)
        {
            return $@"
                <html>
                <body>
                    <h2>Unblock Request - Manager Approval Required</h2>
                    <p>Dear Administrator,</p>
                    
                    <p>A request has been made to unblock a user account:</p>
                    
                    <table border='1' style='border-collapse: collapse; width: 100%;'>
                        <tr><td><strong>Blocked User:</strong></td><td>{blockedUser.Name}</td></tr>
                        <tr><td><strong>Employee ID:</strong></td><td>{blockedUser.EmployeeID}</td></tr>
                        <tr><td><strong>Department:</strong></td><td>{blockedUser.Department}</td></tr>
                        <tr><td><strong>Requesting Manager:</strong></td><td>{requestingManager.Name}</td></tr>
                        <tr><td><strong>Request Date:</strong></td><td>{DateTime.Now:dd/MM/yyyy HH:mm}</td></tr>
                    </table>
                    
                    <p>Please review the training compliance status and approve/reject the unblock request in the SCTMS system.</p>
                    
                    <p>Best regards,<br/>
                    Safety Training Management System<br/>
                    {_configuration["AppSettings:CompanyName"]}</p>
                </body>
                </html>";
        }
    }
} 