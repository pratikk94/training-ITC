namespace SCTMS.Models
{
    public class AuditLog
    {
        public int EventID { get; set; }
        public int? UserID { get; set; }
        public string Action { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string RecordID { get; set; } = string.Empty;
        public string OldValues { get; set; } = string.Empty;
        public string NewValues { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string SessionID { get; set; } = string.Empty;
        public string AdditionalInfo { get; set; } = string.Empty;
        
        // Navigation properties
        public User? User { get; set; }
    }

    public enum AuditAction
    {
        Create,
        Update,
        Delete,
        Login,
        Logout,
        Block,
        Unblock,
        AssignTraining,
        CompleteTraining,
        SendNotification,
        ExportReport
    }
} 