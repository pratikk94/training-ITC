using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SCTMS.Models;
using SCTMS.Services;

namespace SCTMS.Forms
{
    public partial class MainForm : Form, IFormHandler
    {
        private readonly WindowsAuthService _authService;
        private readonly NotificationService _notificationService;
        private readonly ComplianceService _complianceService;
        private readonly ILogger<MainForm> _logger;
        private readonly IServiceProvider _serviceProvider;
        private User? _currentUser;
        private System.Windows.Forms.Timer? _complianceTimer;

        public MainForm(
            WindowsAuthService authService, 
            NotificationService notificationService,
            ComplianceService complianceService,
            ILogger<MainForm> logger,
            IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _authService = authService;
            _notificationService = notificationService;
            _complianceService = complianceService;
            _logger = logger;
            _serviceProvider = serviceProvider;
            
            InitializeForm();
        }

        private async void InitializeForm()
        {
            try
            {
                // Authenticate current Windows user
                _currentUser = await _authService.AuthenticateCurrentUserAsync();
                
                if (_currentUser == null)
                {
                    MessageBox.Show("Unable to authenticate user. Please ensure you are logged into the domain.",
                        "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return;
                }

                // Check if user is blocked
                var loginAccess = await _complianceService.CheckLoginAccessAsync(_currentUser.UserID);
                if (loginAccess != null && loginAccess.IsBlocked)
                {
                    MessageBox.Show($"Your account is blocked due to: {loginAccess.BlockReason}\n\nPlease contact your manager or HR for assistance.",
                        "Account Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Application.Exit();
                    return;
                }

                // Update UI with user information
                UpdateUserInterface();
                
                // Start compliance monitoring timer
                StartComplianceTimer();
                
                // Show welcome message
                _notificationService.ShowPopupNotification("Welcome to SCTMS", 
                    $"Welcome back, {_currentUser.Name}!\n\nSafety Compliance Training Management System is ready.");

                _logger.LogInformation("Main form initialized for user {UserId} - {Name}", _currentUser.UserID, _currentUser.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing main form");
                MessageBox.Show($"Error initializing application: {ex.Message}",
                    "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateUserInterface()
        {
            if (_currentUser == null) return;

            // Update title bar and status
            this.Text = $"SCTMS - Safety Compliance Training Management System - {_currentUser.Name}";
            lblWelcome.Text = $"Welcome, {_currentUser.Name}";
            lblDepartment.Text = $"Department: {_currentUser.Department}";
            lblRole.Text = $"Role: {_currentUser.Role}";
            lblLastLogin.Text = $"Last Login: {_currentUser.LastLoginDate?.ToString("dd/MM/yyyy HH:mm") ?? "First time"}";

            // Enable/disable menu items based on user role
            ConfigureMenusBasedOnRole();
        }

        private void ConfigureMenusBasedOnRole()
        {
            if (_currentUser == null) return;

            var role = Enum.Parse<UserRole>(_currentUser.Role);

            // All users can access these
            menuTrainingAssignments.Enabled = true;
            menuMyProfile.Enabled = true;
            menuNotifications.Enabled = true;

            // Role-based access control
            switch (role)
            {
                case UserRole.Admin:
                    menuUserManagement.Enabled = true;
                    menuTrainingManagement.Enabled = true;
                    menuComplianceDashboard.Enabled = true;
                    menuReports.Enabled = true;
                    menuSystemSettings.Enabled = true;
                    break;

                case UserRole.HR:
                    menuUserManagement.Enabled = true;
                    menuTrainingManagement.Enabled = true;
                    menuComplianceDashboard.Enabled = true;
                    menuReports.Enabled = true;
                    menuSystemSettings.Enabled = false;
                    break;

                case UserRole.Safety:
                    menuUserManagement.Enabled = false;
                    menuTrainingManagement.Enabled = true;
                    menuComplianceDashboard.Enabled = true;
                    menuReports.Enabled = true;
                    menuSystemSettings.Enabled = false;
                    break;

                case UserRole.Manager:
                    menuUserManagement.Enabled = false;
                    menuTrainingManagement.Enabled = true;
                    menuComplianceDashboard.Enabled = true;
                    menuReports.Enabled = false;
                    menuSystemSettings.Enabled = false;
                    break;
            }
        }

        private void StartComplianceTimer()
        {
            _complianceTimer = new System.Windows.Forms.Timer();
            _complianceTimer.Interval = 300000; // 5 minutes
            _complianceTimer.Tick += async (s, e) => await CheckComplianceStatus();
            _complianceTimer.Start();
        }

        private async Task CheckComplianceStatus()
        {
            try
            {
                if (_currentUser == null) return;

                // Check for overdue assignments
                var overdueAssignments = await _complianceService.GetOverdueAssignmentsAsync(_currentUser.UserID);
                
                if (overdueAssignments.Any())
                {
                    var overdueDays = overdueAssignments.Max(a => (DateTime.Now - a.AssignedDate).Days);
                    
                    // Show warning if overdue
                    lblComplianceStatus.Text = $"⚠️ {overdueAssignments.Count} overdue training(s) - {overdueDays} days";
                    lblComplianceStatus.ForeColor = Color.Red;
                    
                    // Show popup reminder every hour for overdue trainings
                    var lastPopup = Properties.Settings.Default.LastPopupTime;
                    if (DateTime.Now.Subtract(lastPopup).TotalHours >= 1)
                    {
                        _notificationService.ShowPopupNotification("Training Overdue", 
                            $"You have {overdueAssignments.Count} overdue training assignments.\n\nPlease complete them immediately to maintain compliance.", 
                            MessageBoxIcon.Warning);
                        
                        Properties.Settings.Default.LastPopupTime = DateTime.Now;
                        Properties.Settings.Default.Save();
                    }
                }
                else
                {
                    lblComplianceStatus.Text = "✅ Compliance Status: Up to date";
                    lblComplianceStatus.ForeColor = Color.Green;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking compliance status");
            }
        }

        // Menu event handlers
        private async void menuUserManagement_Click(object sender, EventArgs e)
        {
            try
            {
                var form = _serviceProvider.GetRequiredService<UserManagementForm>();
                form.SetCurrentUser(_currentUser!);
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening User Management form");
                MessageBox.Show($"Error opening User Management: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void menuTrainingManagement_Click(object sender, EventArgs e)
        {
            try
            {
                var form = _serviceProvider.GetRequiredService<TrainingManagementForm>();
                form.SetCurrentUser(_currentUser!);
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening Training Management form");
                MessageBox.Show($"Error opening Training Management: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void menuComplianceDashboard_Click(object sender, EventArgs e)
        {
            try
            {
                var form = _serviceProvider.GetRequiredService<ComplianceDashboardForm>();
                form.SetCurrentUser(_currentUser!);
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening Compliance Dashboard");
                MessageBox.Show($"Error opening Compliance Dashboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void menuReports_Click(object sender, EventArgs e)
        {
            try
            {
                var form = _serviceProvider.GetRequiredService<ReportsForm>();
                form.SetCurrentUser(_currentUser!);
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening Reports form");
                MessageBox.Show($"Error opening Reports: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void menuNotifications_Click(object sender, EventArgs e)
        {
            try
            {
                var form = _serviceProvider.GetRequiredService<NotificationForm>();
                form.SetCurrentUser(_currentUser!);
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening Notifications form");
                MessageBox.Show($"Error opening Notifications: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit SCTMS?", "Confirm Exit", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void menuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Safety Compliance Training Management System (SCTMS)\n\n" +
                "Version 1.0.0\n" +
                "Developed for ITC Training Division\n\n" +
                "This system manages mandatory safety training compliance\n" +
                "for two-wheeler and four-wheeler operators.\n\n" +
                "© 2024 ITC Training Division",
                "About SCTMS",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            UpdateUserInterface();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _complianceTimer?.Stop();
            _complianceTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }

    public interface IFormHandler
    {
        void SetCurrentUser(User user);
    }
} 