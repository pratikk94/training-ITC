-- Safety Compliance Training Management System (SCTMS) Database Schema
-- Created for ITC Training Division
-- Compatible with SQL Server 2019+

-- Create database (run separately)
-- CREATE DATABASE SCTMS;
-- GO
-- USE SCTMS;
-- GO

-- =============================================
-- Table: Users
-- =============================================
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    EmployeeID NVARCHAR(50) UNIQUE NOT NULL,
    Level NVARCHAR(50) NOT NULL,
    Department NVARCHAR(100) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active', -- Active, Inactive, Suspended, NonCompliant
    ReportingManagerID INT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'Manager', -- Manager, HR, Safety, Admin
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastLoginDate DATETIME2 NULL,
    Email NVARCHAR(100) NOT NULL,
    WindowsUsername NVARCHAR(50) NOT NULL,
    
    CONSTRAINT FK_Users_ReportingManager FOREIGN KEY (ReportingManagerID) REFERENCES Users(UserID),
    CONSTRAINT CK_Users_Status CHECK (Status IN ('Active', 'Inactive', 'Suspended', 'NonCompliant')),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Manager', 'HR', 'Safety', 'Admin'))
);

-- =============================================
-- Table: TrainingAssignments
-- =============================================
CREATE TABLE TrainingAssignments (
    AssignmentID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    TrainingType NVARCHAR(50) NOT NULL, -- TwoWheeler, FourWheeler, Mandatory
    AssignedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Assigned', -- Assigned, InProgress, Completed, Overdue, Deferred
    CompletionDate DATETIME2 NULL,
    NextDueDate DATETIME2 NULL,
    CompletionCertificate NVARCHAR(255) NULL,
    Notes NVARCHAR(MAX) NULL,
    AssignedBy INT NULL,
    ReminderSentDate DATETIME2 NULL,
    ReminderCount INT NOT NULL DEFAULT 0,
    IsRefresher BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_TrainingAssignments_User FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    CONSTRAINT FK_TrainingAssignments_AssignedBy FOREIGN KEY (AssignedBy) REFERENCES Users(UserID),
    CONSTRAINT CK_TrainingAssignments_Type CHECK (TrainingType IN ('TwoWheeler', 'FourWheeler', 'Mandatory', 'Safety', 'Compliance')),
    CONSTRAINT CK_TrainingAssignments_Status CHECK (Status IN ('Assigned', 'InProgress', 'Completed', 'Overdue', 'Deferred', 'Cancelled'))
);

-- =============================================
-- Table: NotificationsLog
-- =============================================
CREATE TABLE NotificationsLog (
    NotificationID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    Type NVARCHAR(20) NOT NULL, -- Email, Popup, SMS, InApp
    Subject NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETDATE(),
    DeferStatus NVARCHAR(20) NOT NULL DEFAULT 'Sent', -- Sent, Deferred, Failed, Read, Pending
    ReadTimestamp DATETIME2 NULL,
    NotificationCategory NVARCHAR(30) NOT NULL DEFAULT 'Training', -- Training, Compliance, System, Reminder, Alert, Information
    RelatedAssignmentID INT NULL,
    DeliveryStatus NVARCHAR(20) NOT NULL DEFAULT 'Sent',
    ErrorMessage NVARCHAR(500) NULL,
    
    CONSTRAINT FK_NotificationsLog_User FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    CONSTRAINT FK_NotificationsLog_Assignment FOREIGN KEY (RelatedAssignmentID) REFERENCES TrainingAssignments(AssignmentID),
    CONSTRAINT CK_NotificationsLog_Type CHECK (Type IN ('Email', 'Popup', 'SMS', 'InApp')),
    CONSTRAINT CK_NotificationsLog_Status CHECK (DeferStatus IN ('Sent', 'Deferred', 'Failed', 'Read', 'Pending')),
    CONSTRAINT CK_NotificationsLog_Category CHECK (NotificationCategory IN ('Training', 'Compliance', 'System', 'Reminder', 'Alert', 'Information'))
);

-- =============================================
-- Table: LoginAccess
-- =============================================
CREATE TABLE LoginAccess (
    LoginAccessID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    IsBlocked BIT NOT NULL DEFAULT 0,
    BlockDate DATETIME2 NULL,
    BlockReason NVARCHAR(100) NULL, -- NonCompliance, ManualBlock, SystemBlock, SecurityViolation, InactiveUser
    UnblockRequestedBy INT NULL,
    UnblockRequestDate DATETIME2 NULL,
    UnblockApproved BIT NOT NULL DEFAULT 0,
    UnblockApprovedBy INT NULL,
    UnblockApprovedDate DATETIME2 NULL,
    UnblockNotes NVARCHAR(500) NULL,
    LastLoginAttempt DATETIME2 NULL,
    FailedLoginAttempts INT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_LoginAccess_User FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    CONSTRAINT FK_LoginAccess_UnblockRequested FOREIGN KEY (UnblockRequestedBy) REFERENCES Users(UserID),
    CONSTRAINT FK_LoginAccess_UnblockApproved FOREIGN KEY (UnblockApprovedBy) REFERENCES Users(UserID),
    CONSTRAINT CK_LoginAccess_BlockReason CHECK (BlockReason IN ('NonCompliance', 'ManualBlock', 'SystemBlock', 'SecurityViolation', 'InactiveUser'))
);

-- =============================================
-- Table: AuditLog
-- =============================================
CREATE TABLE AuditLog (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NULL,
    Action NVARCHAR(50) NOT NULL, -- Create, Update, Delete, Login, Logout, Block, Unblock, AssignTraining, CompleteTraining, SendNotification, ExportReport
    TableName NVARCHAR(50) NULL,
    RecordID NVARCHAR(20) NULL,
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETDATE(),
    IPAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    SessionID NVARCHAR(100) NULL,
    AdditionalInfo NVARCHAR(MAX) NULL,
    
    CONSTRAINT FK_AuditLog_User FOREIGN KEY (UserID) REFERENCES Users(UserID),
    CONSTRAINT CK_AuditLog_Action CHECK (Action IN ('Create', 'Update', 'Delete', 'Login', 'Logout', 'Block', 'Unblock', 'AssignTraining', 'CompleteTraining', 'SendNotification', 'ExportReport'))
);

-- =============================================
-- Table: SystemSettings (Optional - for configuration)
-- =============================================
CREATE TABLE SystemSettings (
    SettingID INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey NVARCHAR(100) UNIQUE NOT NULL,
    SettingValue NVARCHAR(500) NOT NULL,
    Description NVARCHAR(200) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedBy INT NULL,
    
    CONSTRAINT FK_SystemSettings_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(UserID)
);

-- =============================================
-- Create Indexes for Performance
-- =============================================

-- Users table indexes
CREATE NONCLUSTERED INDEX IX_Users_EmployeeID ON Users(EmployeeID);
CREATE NONCLUSTERED INDEX IX_Users_WindowsUsername ON Users(WindowsUsername);
CREATE NONCLUSTERED INDEX IX_Users_Department ON Users(Department);
CREATE NONCLUSTERED INDEX IX_Users_Status ON Users(Status);
CREATE NONCLUSTERED INDEX IX_Users_ReportingManager ON Users(ReportingManagerID);

-- TrainingAssignments table indexes
CREATE NONCLUSTERED INDEX IX_TrainingAssignments_UserID ON TrainingAssignments(UserID);
CREATE NONCLUSTERED INDEX IX_TrainingAssignments_Status ON TrainingAssignments(Status);
CREATE NONCLUSTERED INDEX IX_TrainingAssignments_AssignedDate ON TrainingAssignments(AssignedDate);
CREATE NONCLUSTERED INDEX IX_TrainingAssignments_NextDueDate ON TrainingAssignments(NextDueDate);
CREATE NONCLUSTERED INDEX IX_TrainingAssignments_TrainingType ON TrainingAssignments(TrainingType);
CREATE NONCLUSTERED INDEX IX_TrainingAssignments_CompletionDate ON TrainingAssignments(CompletionDate);

-- NotificationsLog table indexes
CREATE NONCLUSTERED INDEX IX_NotificationsLog_UserID ON NotificationsLog(UserID);
CREATE NONCLUSTERED INDEX IX_NotificationsLog_Timestamp ON NotificationsLog(Timestamp);
CREATE NONCLUSTERED INDEX IX_NotificationsLog_Type ON NotificationsLog(Type);
CREATE NONCLUSTERED INDEX IX_NotificationsLog_Status ON NotificationsLog(DeferStatus);

-- LoginAccess table indexes
CREATE NONCLUSTERED INDEX IX_LoginAccess_UserID ON LoginAccess(UserID);
CREATE NONCLUSTERED INDEX IX_LoginAccess_IsBlocked ON LoginAccess(IsBlocked);

-- AuditLog table indexes
CREATE NONCLUSTERED INDEX IX_AuditLog_UserID ON AuditLog(UserID);
CREATE NONCLUSTERED INDEX IX_AuditLog_Timestamp ON AuditLog(Timestamp);
CREATE NONCLUSTERED INDEX IX_AuditLog_Action ON AuditLog(Action);
CREATE NONCLUSTERED INDEX IX_AuditLog_TableName ON AuditLog(TableName);

-- =============================================
-- Insert Default System Settings
-- =============================================
INSERT INTO SystemSettings (SettingKey, SettingValue, Description) VALUES
('NonComplianceDays', '60', 'Number of days after which non-completion is considered non-compliant'),
('NewJoinerGraceDays', '30', 'Grace period for new joiners to complete training'),
('ReminderIntervalDays', '10', 'Interval in days between reminder notifications'),
('RefresherCycleYears', '3', 'Number of years after which refresher training is required'),
('MaxReminderCount', '5', 'Maximum number of reminders to send before escalation'),
('AutoBlockEnabled', '1', 'Enable automatic blocking of non-compliant users'),
('EmailNotificationsEnabled', '1', 'Enable email notifications'),
('PopupNotificationsEnabled', '1', 'Enable popup notifications'),
('ManagerEscalationEnabled', '1', 'Enable manager escalation for non-compliance'),
('CompanyName', 'ITC Training Division', 'Company name for notifications and reports'),
('AdminEmail', 'admin@company.com', 'Administrator email address'),
('SMTPServer', 'mail.company.com', 'SMTP server for email notifications'),
('SMTPPort', '587', 'SMTP server port'),
('EnableSSL', '1', 'Enable SSL for SMTP connection');

-- =============================================
-- Create Views for Reporting
-- =============================================

-- View: User Compliance Summary
CREATE VIEW vw_UserComplianceSummary AS
SELECT 
    u.UserID,
    u.Name,
    u.EmployeeID,
    u.Department,
    u.Status as UserStatus,
    u.Role,
    COUNT(ta.AssignmentID) as TotalAssignments,
    COUNT(CASE WHEN ta.Status = 'Completed' THEN 1 END) as CompletedAssignments,
    COUNT(CASE WHEN ta.Status IN ('Assigned', 'InProgress') THEN 1 END) as PendingAssignments,
    COUNT(CASE WHEN ta.Status = 'Overdue' THEN 1 END) as OverdueAssignments,
    CASE 
        WHEN COUNT(CASE WHEN ta.Status = 'Overdue' THEN 1 END) > 0 THEN 'Non-Compliant'
        WHEN COUNT(CASE WHEN ta.Status IN ('Assigned', 'InProgress') THEN 1 END) > 0 THEN 'Pending'
        ELSE 'Compliant'
    END as ComplianceStatus,
    MAX(ta.CompletionDate) as LastCompletionDate,
    la.IsBlocked as LoginBlocked
FROM Users u
LEFT JOIN TrainingAssignments ta ON u.UserID = ta.UserID
LEFT JOIN LoginAccess la ON u.UserID = la.UserID
WHERE u.Status = 'Active'
GROUP BY u.UserID, u.Name, u.EmployeeID, u.Department, u.Status, u.Role, la.IsBlocked;

-- View: Training Assignment Details
CREATE VIEW vw_TrainingAssignmentDetails AS
SELECT 
    ta.AssignmentID,
    ta.UserID,
    u.Name as UserName,
    u.EmployeeID,
    u.Department,
    ta.TrainingType,
    ta.AssignedDate,
    ta.Status,
    ta.CompletionDate,
    ta.NextDueDate,
    ta.IsRefresher,
    DATEDIFF(day, ta.AssignedDate, GETDATE()) as DaysSinceAssigned,
    CASE 
        WHEN ta.Status = 'Completed' THEN 0
        WHEN ta.NextDueDate IS NOT NULL AND ta.NextDueDate < GETDATE() THEN DATEDIFF(day, ta.NextDueDate, GETDATE())
        ELSE DATEDIFF(day, ta.AssignedDate, GETDATE())
    END as DaysOverdue,
    ab.Name as AssignedByName,
    ta.ReminderCount,
    ta.Notes
FROM TrainingAssignments ta
INNER JOIN Users u ON ta.UserID = u.UserID
LEFT JOIN Users ab ON ta.AssignedBy = ab.UserID;

-- View: Notification Summary
CREATE VIEW vw_NotificationSummary AS
SELECT 
    nl.UserID,
    u.Name as UserName,
    u.Department,
    COUNT(*) as TotalNotifications,
    COUNT(CASE WHEN nl.Type = 'Email' THEN 1 END) as EmailNotifications,
    COUNT(CASE WHEN nl.Type = 'Popup' THEN 1 END) as PopupNotifications,
    COUNT(CASE WHEN nl.DeferStatus = 'Sent' THEN 1 END) as SentNotifications,
    COUNT(CASE WHEN nl.DeferStatus = 'Failed' THEN 1 END) as FailedNotifications,
    MAX(nl.Timestamp) as LastNotificationDate
FROM NotificationsLog nl
INNER JOIN Users u ON nl.UserID = u.UserID
WHERE nl.Timestamp >= DATEADD(month, -6, GETDATE()) -- Last 6 months
GROUP BY nl.UserID, u.Name, u.Department;

-- =============================================
-- Stored Procedures
-- =============================================

-- Procedure: Get Overdue Assignments
CREATE PROCEDURE sp_GetOverdueAssignments
    @NonComplianceDays INT = 60
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ta.AssignmentID,
        ta.UserID,
        u.Name,
        u.EmployeeID,
        u.Department,
        ta.TrainingType,
        ta.AssignedDate,
        ta.Status,
        DATEDIFF(day, ta.AssignedDate, GETDATE()) as DaysOverdue,
        u.Email,
        m.Name as ManagerName,
        m.Email as ManagerEmail
    FROM TrainingAssignments ta
    INNER JOIN Users u ON ta.UserID = u.UserID
    LEFT JOIN Users m ON u.ReportingManagerID = m.UserID
    WHERE ta.Status IN ('Assigned', 'InProgress')
      AND DATEDIFF(day, ta.AssignedDate, GETDATE()) > @NonComplianceDays
      AND u.Status = 'Active'
    ORDER BY DATEDIFF(day, ta.AssignedDate, GETDATE()) DESC;
END;
GO

-- Procedure: Block Non-Compliant Users
CREATE PROCEDURE sp_BlockNonCompliantUsers
    @NonComplianceDays INT = 60,
    @BlockedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UsersToBlock TABLE (UserID INT);
    
    -- Find users to block
    INSERT INTO @UsersToBlock (UserID)
    SELECT DISTINCT u.UserID
    FROM Users u
    INNER JOIN TrainingAssignments ta ON u.UserID = ta.UserID
    LEFT JOIN LoginAccess la ON u.UserID = la.UserID
    WHERE ta.Status IN ('Assigned', 'InProgress', 'Overdue')
      AND DATEDIFF(day, ta.AssignedDate, GETDATE()) > @NonComplianceDays
      AND u.Status = 'Active'
      AND (la.IsBlocked IS NULL OR la.IsBlocked = 0);
    
    -- Block users
    MERGE LoginAccess AS target
    USING @UsersToBlock AS source ON target.UserID = source.UserID
    WHEN MATCHED THEN
        UPDATE SET 
            IsBlocked = 1,
            BlockDate = GETDATE(),
            BlockReason = 'NonCompliance'
    WHEN NOT MATCHED THEN
        INSERT (UserID, IsBlocked, BlockDate, BlockReason)
        VALUES (source.UserID, 1, GETDATE(), 'NonCompliance');
    
    -- Update user status
    UPDATE Users 
    SET Status = 'NonCompliant'
    WHERE UserID IN (SELECT UserID FROM @UsersToBlock);
    
    -- Log audit entries
    INSERT INTO AuditLog (UserID, Action, TableName, RecordID, AdditionalInfo)
    SELECT UserID, 'Block', 'LoginAccess', CAST(UserID AS NVARCHAR), 'Auto-blocked for non-compliance'
    FROM @UsersToBlock;
    
    SELECT @@ROWCOUNT as UsersBlocked;
END;
GO

-- =============================================
-- Create Triggers for Audit Logging
-- =============================================

-- Trigger for Users table
CREATE TRIGGER tr_Users_Audit
ON Users
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Handle INSERT
    IF EXISTS(SELECT * FROM inserted) AND NOT EXISTS(SELECT * FROM deleted)
    BEGIN
        INSERT INTO AuditLog (UserID, Action, TableName, RecordID, NewValues)
        SELECT 
            i.UserID,
            'Create',
            'Users',
            CAST(i.UserID AS NVARCHAR),
            CONCAT('Name:', i.Name, '; EmployeeID:', i.EmployeeID, '; Department:', i.Department)
        FROM inserted i;
    END
    
    -- Handle UPDATE
    IF EXISTS(SELECT * FROM inserted) AND EXISTS(SELECT * FROM deleted)
    BEGIN
        INSERT INTO AuditLog (UserID, Action, TableName, RecordID, OldValues, NewValues)
        SELECT 
            i.UserID,
            'Update',
            'Users',
            CAST(i.UserID AS NVARCHAR),
            CONCAT('Status:', d.Status, '; Department:', d.Department),
            CONCAT('Status:', i.Status, '; Department:', i.Department)
        FROM inserted i
        INNER JOIN deleted d ON i.UserID = d.UserID;
    END
    
    -- Handle DELETE
    IF NOT EXISTS(SELECT * FROM inserted) AND EXISTS(SELECT * FROM deleted)
    BEGIN
        INSERT INTO AuditLog (UserID, Action, TableName, RecordID, OldValues)
        SELECT 
            d.UserID,
            'Delete',
            'Users',
            CAST(d.UserID AS NVARCHAR),
            CONCAT('Name:', d.Name, '; EmployeeID:', d.EmployeeID)
        FROM deleted d;
    END
END;
GO

-- Trigger for TrainingAssignments table
CREATE TRIGGER tr_TrainingAssignments_Audit
ON TrainingAssignments
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Handle INSERT
    IF EXISTS(SELECT * FROM inserted) AND NOT EXISTS(SELECT * FROM deleted)
    BEGIN
        INSERT INTO AuditLog (UserID, Action, TableName, RecordID, NewValues)
        SELECT 
            i.UserID,
            'AssignTraining',
            'TrainingAssignments',
            CAST(i.AssignmentID AS NVARCHAR),
            CONCAT('TrainingType:', i.TrainingType, '; Status:', i.Status)
        FROM inserted i;
    END
    
    -- Handle UPDATE
    IF EXISTS(SELECT * FROM inserted) AND EXISTS(SELECT * FROM deleted)
    BEGIN
        INSERT INTO AuditLog (UserID, Action, TableName, RecordID, OldValues, NewValues)
        SELECT 
            i.UserID,
            CASE WHEN i.Status = 'Completed' AND d.Status != 'Completed' THEN 'CompleteTraining' ELSE 'Update' END,
            'TrainingAssignments',
            CAST(i.AssignmentID AS NVARCHAR),
            CONCAT('Status:', d.Status, '; CompletionDate:', ISNULL(CAST(d.CompletionDate AS NVARCHAR), 'NULL')),
            CONCAT('Status:', i.Status, '; CompletionDate:', ISNULL(CAST(i.CompletionDate AS NVARCHAR), 'NULL'))
        FROM inserted i
        INNER JOIN deleted d ON i.AssignmentID = d.AssignmentID;
    END
END;
GO

PRINT 'SCTMS Database Schema Created Successfully!';
PRINT 'Please update the connection string in appsettings.json to point to this database.';
PRINT 'Default admin user should be created through the application or manually inserted.'; 