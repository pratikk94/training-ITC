# Safety Compliance Training Management System (SCTMS)

## Overview

The Safety Compliance Training Management System (SCTMS) is a Windows Forms .NET application designed specifically for ITC Training Division to automate and enforce mandatory safety training compliance for two-wheeler and four-wheeler operators.

## Features

### 🔐 Windows Authentication
- **Active Directory Integration**: Seamless login using Windows credentials
- **Role-based Access Control**: Admin, HR, Safety, and Manager roles
- **Automatic User Provisioning**: Users are automatically created from AD

### 👥 User Management
- **Employee Import/Sync**: Import manager and employee data from AD
- **Role-based Permissions**: Different access levels based on user roles
- **Reporting Structure**: Maintains manager-employee relationships

### 📚 Training Management
- **Multiple Training Types**: Two-wheeler, Four-wheeler, and Mandatory safety training
- **Automated Assignment**: Assign trainings to eligible users
- **Progress Tracking**: Monitor completion status and due dates
- **Refresher Cycle**: Automatic refresher training every 3 years

### 🔔 Notification & Escalation Engine
- **Email Notifications**: Automated reminders and alerts
- **Popup Notifications**: In-app desktop notifications
- **Manager Escalation**: Automatic escalation to reporting managers
- **Customizable Templates**: Professional email templates

### 🔒 Login Access Control
- **Automated Blocking**: Block users after 60 days of non-compliance
- **Grace Period**: 30-day grace period for new joiners
- **Unblock Workflow**: Manager request and admin approval process

### 📊 Compliance Dashboard
- **Real-time Status**: Live compliance monitoring
- **Department-wise Reports**: Compliance by department
- **Visual Indicators**: Color-coded status indicators
- **Export Functionality**: Excel export with ClosedXML

## Technical Architecture

### Technology Stack
- **Frontend**: Windows Forms (.NET 8.0)
- **Backend**: C# with ADO.NET
- **Database**: Microsoft SQL Server
- **Authentication**: Windows Authentication (Active Directory)
- **Email**: SMTP with Exchange/Office 365 support
- **Reporting**: ClosedXML for Excel export
- **Logging**: Microsoft.Extensions.Logging

### Database Schema
- **Users**: Employee information and roles
- **TrainingAssignments**: Training assignments and tracking
- **NotificationsLog**: All notification history
- **LoginAccess**: Access control and blocking
- **AuditLog**: Complete audit trail
- **SystemSettings**: Configurable parameters

## Prerequisites

- Windows 10/11 or Windows Server 2019+
- .NET 8.0 Runtime
- SQL Server 2019+ (Express edition supported)
- Active Directory domain membership
- SMTP server access (Exchange/Office 365)

## Installation & Setup

### 1. Database Setup

1. **Create Database**:
   ```sql
   CREATE DATABASE SCTMS;
   ```

2. **Run Schema Script**:
   Execute `Database/CreateTables.sql` in SQL Server Management Studio

3. **Update Connection String**:
   Modify `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=YOUR_SERVER;Initial Catalog=SCTMS;Integrated Security=True;TrustServerCertificate=True"
     }
   }
   ```

### 2. Application Configuration

Update `appsettings.json` with your organization's settings:

```json
{
  "AppSettings": {
    "CompanyName": "Your Company Name",
    "NonComplianceDays": 60,
    "NewJoinerGraceDays": 30,
    "ReminderIntervalDays": 10,
    "RefresherCycleYears": 3,
    "AdminEmail": "admin@yourcompany.com",
    "SMTPServer": "mail.yourcompany.com",
    "SMTPPort": 587,
    "EnableSSL": true
  }
}
```

### 3. Build & Deploy

1. **Build Application**:
   ```bash
   dotnet build --configuration Release
   ```

2. **Publish for Deployment**:
   ```bash
   dotnet publish --configuration Release --self-contained true --runtime win-x64
   ```

3. **Deploy to User Machines**:
   - Copy published files to target machines
   - Ensure users have necessary permissions
   - Create desktop shortcuts if needed

### 4. First Run Setup

1. **Admin User Creation**:
   First user with Admin role should be created manually or imported from AD

2. **Active Directory Sync**:
   Run initial AD synchronization to import users

3. **System Settings**:
   Configure system parameters via admin interface

## Usage Guide

### For Administrators

1. **User Management**:
   - Import users from Active Directory
   - Assign roles and permissions
   - Manage reporting structures

2. **Training Management**:
   - Create training assignments
   - Monitor completion status
   - Generate compliance reports

3. **System Configuration**:
   - Configure notification settings
   - Set compliance parameters
   - Manage system settings

### For Managers

1. **Team Monitoring**:
   - View team compliance status
   - Track training progress
   - Request user unblocking

2. **Training Assignment**:
   - Assign trainings to team members
   - Monitor completion deadlines
   - Send manual reminders

### For Employees

1. **Training Tracking**:
   - View assigned trainings
   - Update completion status
   - Upload certificates

2. **Notifications**:
   - Receive training reminders
   - Get compliance alerts
   - View notification history

## Security Features

- **Windows Authentication**: Leverages existing AD credentials
- **Role-based Access**: Granular permission system
- **Audit Logging**: Complete activity tracking
- **Data Encryption**: SQL Server encryption support
- **Secure Communication**: HTTPS/TLS for external communications

## Monitoring & Maintenance

### Automated Processes

- **Daily Compliance Check**: Runs automatically
- **Reminder Notifications**: Sent every 10 days
- **Automatic Blocking**: After 60 days non-compliance
- **Refresher Assignments**: Created automatically

### Database Maintenance

- **Regular Backups**: Implement automated backups
- **Index Maintenance**: Optimize performance
- **Audit Log Cleanup**: Archive old audit entries
- **Performance Monitoring**: Track query performance

## Troubleshooting

### Common Issues

1. **Database Connection Failed**:
   - Verify SQL Server is running
   - Check connection string
   - Ensure Windows Authentication is enabled

2. **Active Directory Authentication Failed**:
   - Verify domain membership
   - Check AD service connectivity
   - Ensure user has proper permissions

3. **Email Notifications Not Sending**:
   - Verify SMTP settings
   - Check firewall rules
   - Test SMTP connectivity

4. **Performance Issues**:
   - Check database indexes
   - Monitor memory usage
   - Review SQL query performance

### Log Files

Application logs are stored in:
- Console output (debug mode)
- Windows Event Log (production)
- Custom log files (if configured)

### Support Contacts

- **Technical Support**: IT Department
- **Application Issues**: HR Department
- **Training Content**: Safety Department

## Development

### Development Environment

1. **Prerequisites**:
   - Visual Studio 2022 or VS Code
   - .NET 8.0 SDK
   - SQL Server Developer Edition
   - Git for version control

2. **Development Setup**:
   ```bash
   git clone <repository-url>
   cd SCTMS
   dotnet restore
   dotnet build
   ```

3. **Database Development**:
   - Use local SQL Server instance
   - Apply migrations for schema changes
   - Test with sample data

### Contributing

1. Create feature branch
2. Implement changes with tests
3. Update documentation
4. Submit pull request

## Compliance & Regulations

This system helps organizations comply with:
- **Occupational Safety Requirements**
- **Training Documentation Standards**
- **Audit Trail Regulations**
- **Data Protection Guidelines**

## Version History

- **v1.0.0** - Initial release with core functionality
- **v1.1.0** - Enhanced reporting and notifications
- **v1.2.0** - Active Directory integration improvements

## License

© 2024 ITC Training Division. All rights reserved.

This software is proprietary and confidential. Unauthorized copying, distribution, or use is strictly prohibited.

---

For technical support or questions, please contact the IT Department or refer to the system documentation. 