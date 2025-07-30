namespace SCTMS.Models
{
    public class NotificationLog
    {
        public int NotificationID { get; set; }
        public int UserID { get; set; }
        public string Type { get; set; } = string.Empty; // Email, Popup, SMS
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string DeferStatus { get; set; } = string.Empty; // Sent, Deferred, Failed, Read
        public DateTime? ReadTimestamp { get; set; }
        public string NotificationCategory { get; set; } = string.Empty; // Training, Compliance, System
        public int? RelatedAssignmentID { get; set; }
        public string DeliveryStatus { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        
        // Navigation properties
        public User? User { get; set; }
        public TrainingAssignment? RelatedAssignment { get; set; }
    }

    public enum NotificationType
    {
        Email,
        Popup,
        SMS,
        InApp
    }

    public enum NotificationStatus
    {
        Sent,
        Deferred,
        Failed,
        Read,
        Pending
    }

    public enum NotificationCategory
    {
        Training,
        Compliance,
        System,
        Reminder,
        Alert,
        Information
    }
} 