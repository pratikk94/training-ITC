# SCTMS Windows Setup & Deployment Guide

## 🪟 Complete Guide to Running SCTMS on Windows

This guide provides step-by-step instructions for setting up and running the Safety Compliance Training Management System (SCTMS) on Windows environments.

---

## 📋 Prerequisites Checklist

### System Requirements
- ✅ **Windows 10** (version 1903 or later) OR **Windows 11**
- ✅ **Windows Server 2019** or later (for server deployment)
- ✅ **4GB RAM** minimum (8GB recommended)
- ✅ **2GB free disk space**
- ✅ **Network connectivity** for database and email

### Required Software
- ✅ **.NET 8.0 Runtime** (Desktop & ASP.NET Core)
- ✅ **SQL Server** (2019 or later, Express edition acceptable)
- ✅ **SQL Server Management Studio** (SSMS)
- ✅ **Active Directory** domain membership
- ✅ **SMTP Server** access (Exchange, Office 365, or Gmail)

---

## 🔧 Step 1: Install .NET 8.0 Runtime

### Option A: Download from Microsoft
1. Visit: https://dotnet.microsoft.com/download/dotnet/8.0
2. Download **".NET Desktop Runtime 8.0.x"** for Windows x64
3. Run the installer as Administrator
4. Restart your computer

### Option B: Using PowerShell (Recommended for IT Admins)
```powershell
# Run PowerShell as Administrator
winget install Microsoft.DotNet.DesktopRuntime.8
```

### Verify Installation
```cmd
dotnet --version
```
Should show: `8.0.x`

---

## 🗄️ Step 2: SQL Server Setup

### Option A: SQL Server Express (Free)
1. Download **SQL Server 2022 Express** from Microsoft
2. Run installer, choose **Basic** installation
3. Note the **Server Name** (usually `COMPUTERNAME\SQLEXPRESS`)
4. Enable **Windows Authentication**

### Option B: Existing SQL Server Instance
1. Ensure you have `db_creator` permissions
2. Note your **Server Name** and **Instance**
3. Ensure **Windows Authentication** is enabled

### Install SQL Server Management Studio (SSMS)
1. Download SSMS from Microsoft website
2. Install and connect to your SQL Server instance
3. Test connection successfully

---

## 🏗️ Step 3: Database Setup

### 1. Create SCTMS Database
Open **SQL Server Management Studio** and run:

```sql
-- Create the database
CREATE DATABASE SCTMS;
GO

-- Verify database creation
USE SCTMS;
SELECT DB_NAME() AS DatabaseName;
```

### 2. Execute Database Schema
1. Open the file: `Database/CreateTables.sql`
2. Copy all contents
3. In SSMS, ensure you're connected to **SCTMS** database
4. Paste and execute the entire script
5. Verify tables were created:

```sql
-- Check tables were created
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE';
```

You should see: `Users`, `TrainingAssignments`, `NotificationsLog`, `LoginAccess`, `AuditLog`, `SystemSettings`

### 3. Create Test Admin User (Optional)
```sql
USE SCTMS;

-- Insert a test admin user (replace with your Windows username)
INSERT INTO Users (Name, EmployeeID, Level, Department, Status, Role, CreatedDate, Email, WindowsUsername)
VALUES ('Test Admin', 'ADMIN001', 'Administrator', 'IT', 'Active', 'Admin', GETDATE(), 
        'admin@yourcompany.com', 'YourWindowsUsername');

-- Verify user was created
SELECT * FROM Users;
```

---

## ⚙️ Step 4: Application Configuration

### 1. Update Database Connection
Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR_SERVER_NAME\\SQLEXPRESS;Initial Catalog=SCTMS;Integrated Security=True;TrustServerCertificate=True"
  }
}
```

**Replace `YOUR_SERVER_NAME`** with your actual computer name.

### 2. Configure Email Settings
Update the `AppSettings` section in `appsettings.json`:

#### For Office 365/Exchange Online:
```json
{
  "AppSettings": {
    "CompanyName": "Your Company Name",
    "AdminEmail": "admin@yourcompany.com",
    "SMTPServer": "smtp.office365.com",
    "SMTPPort": 587,
    "EnableSSL": true
  }
}
```

#### For Gmail (if allowed by IT):
```json
{
  "AppSettings": {
    "SMTPServer": "smtp.gmail.com",
    "SMTPPort": 587,
    "EnableSSL": true
  }
}
```

#### For On-Premises Exchange:
```json
{
  "AppSettings": {
    "SMTPServer": "mail.yourcompany.local",
    "SMTPPort": 25,
    "EnableSSL": false
  }
}
```

### 3. Customize Business Rules
Adjust compliance settings as needed:

```json
{
  "AppSettings": {
    "NonComplianceDays": 60,        // Days before user is non-compliant
    "NewJoinerGraceDays": 30,       // Grace period for new employees
    "ReminderIntervalDays": 10,     // Days between reminder emails
    "RefresherCycleYears": 3        // Years before refresher training required
  }
}
```

---

## 🚀 Step 5: Build and Run the Application

### Option A: Using Visual Studio (Development)
1. Open `SCTMS.sln` in Visual Studio 2022
2. Right-click solution → **Restore NuGet Packages**
3. Press **F5** to build and run
4. Application should start and show the main window

### Option B: Using Command Line
```cmd
# Navigate to SCTMS folder
cd C:\Path\To\SCTMS

# Restore NuGet packages
dotnet restore

# Build the application
dotnet build --configuration Release

# Run the application
dotnet run
```

### Option C: Create Executable
```cmd
# Create self-contained executable
dotnet publish --configuration Release --self-contained true --runtime win-x64 --output "C:\SCTMS-Deploy"

# Navigate to deployment folder
cd C:\SCTMS-Deploy

# Run the executable
SCTMS.exe
```

---

## 🧪 Step 6: Test the Application

### 1. Initial Login Test
1. **Launch SCTMS.exe**
2. Application should authenticate you automatically using Windows credentials
3. If successful, you'll see the main dashboard
4. Check the status bar shows your username

### 2. Database Connection Test
1. Look for any database connection errors in the console
2. Main form should load user information
3. Try navigating to different menu items

### 3. Email Test (Optional)
1. Go to **Tools** → **System Settings** (if available)
2. Send a test notification to verify SMTP settings
3. Check your email for test message

---

## 🔧 Step 7: Production Deployment

### For Single User Installation
1. Copy the `C:\SCTMS-Deploy` folder to the target machine
2. Ensure .NET 8.0 Runtime is installed
3. Update `appsettings.json` with correct database server
4. Create desktop shortcut to `SCTMS.exe`

### For Multiple Users (Network Deployment)
1. **Option A: Shared Network Drive**
   ```
   \\server\SCTMS\SCTMS.exe
   ```
   - Deploy to network share
   - Users run from network location
   - Single configuration file

2. **Option B: Local Installation with Central Database**
   - Install application locally on each machine
   - All point to same SQL Server database
   - Easier for offline usage

### Create Windows Installer (Advanced)
```cmd
# Install Wix Toolset
dotnet tool install --global wix

# Create MSI installer
dotnet build --configuration Release
wix create installer --source bin/Release/net8.0-windows/win-x64/publish/
```

---

## 🔐 Step 8: Security Configuration

### Windows Firewall Rules
If deploying across network, add firewall exceptions:

```cmd
# Allow SCTMS through Windows Firewall
netsh advfirewall firewall add rule name="SCTMS Application" dir=in action=allow program="C:\SCTMS-Deploy\SCTMS.exe"
```

### SQL Server Firewall
```cmd
# Allow SQL Server through firewall (default port 1433)
netsh advfirewall firewall add rule name="SQL Server" dir=in action=allow protocol=TCP localport=1433
```

### Active Directory Permissions
Ensure the application service account has:
- **Read** permissions on user accounts
- **Read** permissions on group memberships
- Access to query user properties (email, department, manager)

---

## 📊 Step 9: Monitoring & Maintenance

### Application Logs
Logs are written to:
- **Console Output** (during development)
- **Windows Event Log** → **Application** (in production)
- **Debug Output** (when debugging)

### Database Maintenance
Set up automated tasks:

```sql
-- Weekly backup
BACKUP DATABASE SCTMS TO DISK = 'C:\Backups\SCTMS_Weekly.bak'

-- Monthly audit log cleanup (keep 6 months)
DELETE FROM AuditLog WHERE Timestamp < DATEADD(month, -6, GETDATE())

-- Update database statistics
UPDATE STATISTICS Users
UPDATE STATISTICS TrainingAssignments
```

### Performance Monitoring
Monitor these metrics:
- **Application Response Time**
- **Database Query Performance**
- **Email Send Success Rate**
- **Active Directory Sync Time**

---

## 🆘 Troubleshooting Guide

### Common Issues & Solutions

#### ❌ "Database Connection Failed"
**Cause**: SQL Server not accessible
**Solution**:
1. Check SQL Server is running: `services.msc` → Find SQL Server services
2. Verify connection string in `appsettings.json`
3. Test connection in SSMS
4. Check Windows Firewall settings

#### ❌ "Windows Authentication Failed"
**Cause**: Not joined to domain or AD issues
**Solution**:
1. Verify computer is domain-joined: `systeminfo | findstr Domain`
2. Check network connectivity to domain controller
3. Try logging out and back in to Windows
4. Contact IT for AD connectivity issues

#### ❌ "Email Notifications Not Sending"
**Cause**: SMTP configuration issues
**Solution**:
1. Verify SMTP settings in `appsettings.json`
2. Test SMTP connectivity: `telnet smtp.office365.com 587`
3. Check firewall allows outbound port 587/25
4. Verify email account has send permissions

#### ❌ "Application Won't Start"
**Cause**: Missing .NET Runtime or dependencies
**Solution**:
1. Reinstall .NET 8.0 Runtime
2. Check Windows Event Log for detailed error
3. Run from command line to see console errors
4. Verify all NuGet packages restored

#### ❌ "Slow Performance"
**Cause**: Database performance issues
**Solution**:
```sql
-- Check database performance
SELECT * FROM sys.dm_exec_query_stats
ORDER BY total_elapsed_time DESC

-- Rebuild indexes
ALTER INDEX ALL ON Users REBUILD
ALTER INDEX ALL ON TrainingAssignments REBUILD
```

### Getting Help
1. **Check Windows Event Log**: `eventvwr.msc` → Application Logs
2. **Enable Debug Logging**: Set logging level to `Debug` in `appsettings.json`
3. **Run from Command Line**: See console output for detailed errors
4. **Contact IT Support**: Provide log files and error screenshots

---

## 📞 Support Information

### Technical Requirements
- **Minimum .NET Version**: 8.0
- **Supported SQL Server**: 2019, 2022 (Express, Standard, Enterprise)
- **Supported Windows**: 10 (1903+), 11, Server 2019/2022
- **Active Directory**: Windows Server 2016+ domain functional level

### Performance Recommendations
- **Development**: 4GB RAM, any modern CPU
- **Production (1-50 users)**: 8GB RAM, SQL Server Express
- **Production (50+ users)**: 16GB RAM, SQL Server Standard
- **High Availability**: SQL Server cluster, load balancing

### Backup Strategy
1. **Daily**: Database transaction log backup
2. **Weekly**: Full database backup
3. **Monthly**: Application files backup
4. **Quarterly**: Complete system backup

---

## ✅ Quick Start Checklist

- [ ] Install .NET 8.0 Runtime
- [ ] Install SQL Server (Express or full)
- [ ] Install SQL Server Management Studio
- [ ] Create SCTMS database
- [ ] Run database schema script
- [ ] Update `appsettings.json` with your settings
- [ ] Build application (`dotnet build`)
- [ ] Test database connection
- [ ] Configure email settings
- [ ] Test application launch
- [ ] Verify Windows Authentication works
- [ ] Create desktop shortcut
- [ ] Document settings for your organization

---

**🎉 Congratulations! Your SCTMS application should now be running successfully on Windows.**

For additional support or customization, refer to the main `README.md` file or contact your IT administrator. 