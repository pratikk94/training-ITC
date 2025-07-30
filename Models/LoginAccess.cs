namespace SCTMS.Models
{
    public class LoginAccess
    {
        public int LoginAccessID { get; set; }
        public int UserID { get; set; }
        public bool IsBlocked { get; set; } = false;
        public DateTime? BlockDate { get; set; }
        public string BlockReason { get; set; } = string.Empty; // NonCompliance, Manual, System
        public int? UnblockRequestedBy { get; set; }
        public DateTime? UnblockRequestDate { get; set; }
        public bool UnblockApproved { get; set; } = false;
        public int? UnblockApprovedBy { get; set; }
        public DateTime? UnblockApprovedDate { get; set; }
        public string UnblockNotes { get; set; } = string.Empty;
        public DateTime? LastLoginAttempt { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        
        // Navigation properties
        public User? User { get; set; }
        public User? UnblockRequestedByUser { get; set; }
        public User? UnblockApprovedByUser { get; set; }
    }

    public enum BlockReason
    {
        NonCompliance,
        ManualBlock,
        SystemBlock,
        SecurityViolation,
        InactiveUser
    }
} 