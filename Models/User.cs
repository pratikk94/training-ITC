namespace SCTMS.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmployeeID { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Active, Inactive, Suspended
        public int? ReportingManagerID { get; set; }
        public string Role { get; set; } = string.Empty; // Manager, HR, Safety, Admin
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string Email { get; set; } = string.Empty;
        public string WindowsUsername { get; set; } = string.Empty;
        
        // Navigation properties
        public List<TrainingAssignment> TrainingAssignments { get; set; } = new();
        public List<NotificationLog> Notifications { get; set; } = new();
        public LoginAccess? LoginAccess { get; set; }
    }

    public enum UserRole
    {
        Manager,
        HR,
        Safety,
        Admin
    }

    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended,
        NonCompliant
    }
} 