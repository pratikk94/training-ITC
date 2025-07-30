namespace SCTMS.Models
{
    public class TrainingAssignment
    {
        public int AssignmentID { get; set; }
        public int UserID { get; set; }
        public string TrainingType { get; set; } = string.Empty; // TwoWheeler, FourWheeler, Mandatory
        public DateTime AssignedDate { get; set; }
        public string Status { get; set; } = string.Empty; // Assigned, InProgress, Completed, Overdue, Deferred
        public DateTime? CompletionDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public string CompletionCertificate { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int? AssignedBy { get; set; }
        public DateTime? ReminderSentDate { get; set; }
        public int ReminderCount { get; set; } = 0;
        public bool IsRefresher { get; set; } = false;
        
        // Navigation properties
        public User? User { get; set; }
        public User? AssignedByUser { get; set; }
    }

    public enum TrainingType
    {
        TwoWheeler,
        FourWheeler,
        Mandatory,
        Safety,
        Compliance
    }

    public enum TrainingStatus
    {
        Assigned,
        InProgress,
        Completed,
        Overdue,
        Deferred,
        Cancelled
    }
} 