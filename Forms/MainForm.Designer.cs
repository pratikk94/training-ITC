namespace SCTMS.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            
            // Main Menu Strip
            menuStrip = new MenuStrip();
            menuFile = new ToolStripMenuItem();
            menuExit = new ToolStripMenuItem();
            menuManagement = new ToolStripMenuItem();
            menuUserManagement = new ToolStripMenuItem();
            menuTrainingManagement = new ToolStripMenuItem();
            menuView = new ToolStripMenuItem();
            menuComplianceDashboard = new ToolStripMenuItem();
            menuReports = new ToolStripMenuItem();
            menuTrainingAssignments = new ToolStripMenuItem();
            menuNotifications = new ToolStripMenuItem();
            menuMyProfile = new ToolStripMenuItem();
            menuTools = new ToolStripMenuItem();
            menuSystemSettings = new ToolStripMenuItem();
            menuHelp = new ToolStripMenuItem();
            menuAbout = new ToolStripMenuItem();

            // Status Strip
            statusStrip = new StatusStrip();
            lblComplianceStatus = new ToolStripStatusLabel();
            lblCurrentUser = new ToolStripStatusLabel();
            lblDateTime = new ToolStripStatusLabel();

            // Main Panel
            pnlMain = new Panel();
            
            // Welcome Panel
            pnlWelcome = new Panel();
            lblWelcome = new Label();
            lblDepartment = new Label();
            lblRole = new Label();
            lblLastLogin = new Label();

            // Quick Actions Panel
            pnlQuickActions = new Panel();
            lblQuickActions = new Label();
            btnMyTrainings = new Button();
            btnComplianceStatus = new Button();
            btnNotifications = new Button();
            btnReports = new Button();

            // Notifications Panel
            pnlNotifications = new Panel();
            lblRecentNotifications = new Label();
            listNotifications = new ListBox();

            // Timer for status updates
            timerStatus = new System.Windows.Forms.Timer(components);

            // Suspend layout
            SuspendLayout();

            // 
            // menuStrip
            // 
            menuStrip.ImageScalingSize = new Size(20, 20);
            menuStrip.Items.AddRange(new ToolStripItem[] {
                menuFile, menuManagement, menuView, menuTools, menuHelp });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1200, 28);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip";

            // 
            // menuFile
            // 
            menuFile.DropDownItems.AddRange(new ToolStripItem[] { menuExit });
            menuFile.Name = "menuFile";
            menuFile.Size = new Size(46, 24);
            menuFile.Text = "&File";

            // 
            // menuExit
            // 
            menuExit.Name = "menuExit";
            menuExit.Size = new Size(108, 26);
            menuExit.Text = "E&xit";
            menuExit.Click += menuExit_Click;

            // 
            // menuManagement
            // 
            menuManagement.DropDownItems.AddRange(new ToolStripItem[] {
                menuUserManagement, menuTrainingManagement });
            menuManagement.Name = "menuManagement";
            menuManagement.Size = new Size(109, 24);
            menuManagement.Text = "&Management";

            // 
            // menuUserManagement
            // 
            menuUserManagement.Name = "menuUserManagement";
            menuUserManagement.Size = new Size(223, 26);
            menuUserManagement.Text = "&User Management";
            menuUserManagement.Click += menuUserManagement_Click;

            // 
            // menuTrainingManagement
            // 
            menuTrainingManagement.Name = "menuTrainingManagement";
            menuTrainingManagement.Size = new Size(223, 26);
            menuTrainingManagement.Text = "&Training Management";
            menuTrainingManagement.Click += menuTrainingManagement_Click;

            // 
            // menuView
            // 
            menuView.DropDownItems.AddRange(new ToolStripItem[] {
                menuComplianceDashboard, menuReports, menuTrainingAssignments, 
                menuNotifications, menuMyProfile });
            menuView.Name = "menuView";
            menuView.Size = new Size(55, 24);
            menuView.Text = "&View";

            // 
            // menuComplianceDashboard
            // 
            menuComplianceDashboard.Name = "menuComplianceDashboard";
            menuComplianceDashboard.Size = new Size(224, 26);
            menuComplianceDashboard.Text = "&Compliance Dashboard";
            menuComplianceDashboard.Click += menuComplianceDashboard_Click;

            // 
            // menuReports
            // 
            menuReports.Name = "menuReports";
            menuReports.Size = new Size(224, 26);
            menuReports.Text = "&Reports";
            menuReports.Click += menuReports_Click;

            // 
            // menuTrainingAssignments
            // 
            menuTrainingAssignments.Name = "menuTrainingAssignments";
            menuTrainingAssignments.Size = new Size(224, 26);
            menuTrainingAssignments.Text = "Training &Assignments";

            // 
            // menuNotifications
            // 
            menuNotifications.Name = "menuNotifications";
            menuNotifications.Size = new Size(224, 26);
            menuNotifications.Text = "&Notifications";
            menuNotifications.Click += menuNotifications_Click;

            // 
            // menuMyProfile
            // 
            menuMyProfile.Name = "menuMyProfile";
            menuMyProfile.Size = new Size(224, 26);
            menuMyProfile.Text = "My &Profile";

            // 
            // menuTools
            // 
            menuTools.DropDownItems.AddRange(new ToolStripItem[] { menuSystemSettings });
            menuTools.Name = "menuTools";
            menuTools.Size = new Size(58, 24);
            menuTools.Text = "&Tools";

            // 
            // menuSystemSettings
            // 
            menuSystemSettings.Name = "menuSystemSettings";
            menuSystemSettings.Size = new Size(189, 26);
            menuSystemSettings.Text = "&System Settings";

            // 
            // menuHelp
            // 
            menuHelp.DropDownItems.AddRange(new ToolStripItem[] { menuAbout });
            menuHelp.Name = "menuHelp";
            menuHelp.Size = new Size(55, 24);
            menuHelp.Text = "&Help";

            // 
            // menuAbout
            // 
            menuAbout.Name = "menuAbout";
            menuAbout.Size = new Size(133, 26);
            menuAbout.Text = "&About";
            menuAbout.Click += menuAbout_Click;

            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] {
                lblComplianceStatus, lblCurrentUser, lblDateTime });
            statusStrip.Location = new Point(0, 726);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1200, 26);
            statusStrip.TabIndex = 1;
            statusStrip.Text = "statusStrip";

            // 
            // lblComplianceStatus
            // 
            lblComplianceStatus.Name = "lblComplianceStatus";
            lblComplianceStatus.Size = new Size(200, 20);
            lblComplianceStatus.Text = "Compliance Status: Loading...";

            // 
            // lblCurrentUser
            // 
            lblCurrentUser.Name = "lblCurrentUser";
            lblCurrentUser.Size = new Size(200, 20);
            lblCurrentUser.Text = "User: Loading...";

            // 
            // lblDateTime
            // 
            lblDateTime.Name = "lblDateTime";
            lblDateTime.Size = new Size(150, 20);
            lblDateTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            // 
            // pnlMain
            // 
            pnlMain.Controls.Add(pnlWelcome);
            pnlMain.Controls.Add(pnlQuickActions);
            pnlMain.Controls.Add(pnlNotifications);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 28);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(1200, 698);
            pnlMain.TabIndex = 2;

            // 
            // pnlWelcome
            // 
            pnlWelcome.BackColor = Color.LightBlue;
            pnlWelcome.Controls.Add(lblWelcome);
            pnlWelcome.Controls.Add(lblDepartment);
            pnlWelcome.Controls.Add(lblRole);
            pnlWelcome.Controls.Add(lblLastLogin);
            pnlWelcome.Dock = DockStyle.Top;
            pnlWelcome.Location = new Point(0, 0);
            pnlWelcome.Name = "pnlWelcome";
            pnlWelcome.Size = new Size(1200, 120);
            pnlWelcome.TabIndex = 0;

            // 
            // lblWelcome
            // 
            lblWelcome.AutoSize = true;
            lblWelcome.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblWelcome.Location = new Point(20, 20);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(300, 37);
            lblWelcome.TabIndex = 0;
            lblWelcome.Text = "Welcome to SCTMS";

            // 
            // lblDepartment
            // 
            lblDepartment.AutoSize = true;
            lblDepartment.Font = new Font("Segoe UI", 10F);
            lblDepartment.Location = new Point(20, 60);
            lblDepartment.Name = "lblDepartment";
            lblDepartment.Size = new Size(100, 23);
            lblDepartment.TabIndex = 1;
            lblDepartment.Text = "Department:";

            // 
            // lblRole
            // 
            lblRole.AutoSize = true;
            lblRole.Font = new Font("Segoe UI", 10F);
            lblRole.Location = new Point(300, 60);
            lblRole.Name = "lblRole";
            lblRole.Size = new Size(48, 23);
            lblRole.TabIndex = 2;
            lblRole.Text = "Role:";

            // 
            // lblLastLogin
            // 
            lblLastLogin.AutoSize = true;
            lblLastLogin.Font = new Font("Segoe UI", 10F);
            lblLastLogin.Location = new Point(20, 85);
            lblLastLogin.Name = "lblLastLogin";
            lblLastLogin.Size = new Size(93, 23);
            lblLastLogin.TabIndex = 3;
            lblLastLogin.Text = "Last Login:";

            // 
            // pnlQuickActions
            // 
            pnlQuickActions.BackColor = Color.WhiteSmoke;
            pnlQuickActions.Controls.Add(lblQuickActions);
            pnlQuickActions.Controls.Add(btnMyTrainings);
            pnlQuickActions.Controls.Add(btnComplianceStatus);
            pnlQuickActions.Controls.Add(btnNotifications);
            pnlQuickActions.Controls.Add(btnReports);
            pnlQuickActions.Dock = DockStyle.Left;
            pnlQuickActions.Location = new Point(0, 120);
            pnlQuickActions.Name = "pnlQuickActions";
            pnlQuickActions.Size = new Size(300, 578);
            pnlQuickActions.TabIndex = 1;

            // 
            // lblQuickActions
            // 
            lblQuickActions.AutoSize = true;
            lblQuickActions.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblQuickActions.Location = new Point(20, 20);
            lblQuickActions.Name = "lblQuickActions";
            lblQuickActions.Size = new Size(155, 32);
            lblQuickActions.TabIndex = 0;
            lblQuickActions.Text = "Quick Actions";

            // 
            // btnMyTrainings
            // 
            btnMyTrainings.BackColor = Color.FromArgb(0, 123, 255);
            btnMyTrainings.FlatStyle = FlatStyle.Flat;
            btnMyTrainings.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnMyTrainings.ForeColor = Color.White;
            btnMyTrainings.Location = new Point(20, 70);
            btnMyTrainings.Name = "btnMyTrainings";
            btnMyTrainings.Size = new Size(250, 50);
            btnMyTrainings.TabIndex = 1;
            btnMyTrainings.Text = "📚 My Training Assignments";
            btnMyTrainings.UseVisualStyleBackColor = false;

            // 
            // btnComplianceStatus
            // 
            btnComplianceStatus.BackColor = Color.FromArgb(40, 167, 69);
            btnComplianceStatus.FlatStyle = FlatStyle.Flat;
            btnComplianceStatus.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnComplianceStatus.ForeColor = Color.White;
            btnComplianceStatus.Location = new Point(20, 140);
            btnComplianceStatus.Name = "btnComplianceStatus";
            btnComplianceStatus.Size = new Size(250, 50);
            btnComplianceStatus.TabIndex = 2;
            btnComplianceStatus.Text = "✅ Compliance Dashboard";
            btnComplianceStatus.UseVisualStyleBackColor = false;

            // 
            // btnNotifications
            // 
            btnNotifications.BackColor = Color.FromArgb(255, 193, 7);
            btnNotifications.FlatStyle = FlatStyle.Flat;
            btnNotifications.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnNotifications.ForeColor = Color.Black;
            btnNotifications.Location = new Point(20, 210);
            btnNotifications.Name = "btnNotifications";
            btnNotifications.Size = new Size(250, 50);
            btnNotifications.TabIndex = 3;
            btnNotifications.Text = "🔔 Notifications";
            btnNotifications.UseVisualStyleBackColor = false;

            // 
            // btnReports
            // 
            btnReports.BackColor = Color.FromArgb(108, 117, 125);
            btnReports.FlatStyle = FlatStyle.Flat;
            btnReports.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnReports.ForeColor = Color.White;
            btnReports.Location = new Point(20, 280);
            btnReports.Name = "btnReports";
            btnReports.Size = new Size(250, 50);
            btnReports.TabIndex = 4;
            btnReports.Text = "📊 Reports";
            btnReports.UseVisualStyleBackColor = false;

            // 
            // pnlNotifications
            // 
            pnlNotifications.BackColor = Color.White;
            pnlNotifications.Controls.Add(lblRecentNotifications);
            pnlNotifications.Controls.Add(listNotifications);
            pnlNotifications.Dock = DockStyle.Fill;
            pnlNotifications.Location = new Point(300, 120);
            pnlNotifications.Name = "pnlNotifications";
            pnlNotifications.Size = new Size(900, 578);
            pnlNotifications.TabIndex = 2;

            // 
            // lblRecentNotifications
            // 
            lblRecentNotifications.AutoSize = true;
            lblRecentNotifications.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblRecentNotifications.Location = new Point(20, 20);
            lblRecentNotifications.Name = "lblRecentNotifications";
            lblRecentNotifications.Size = new Size(222, 32);
            lblRecentNotifications.TabIndex = 0;
            lblRecentNotifications.Text = "Recent Notifications";

            // 
            // listNotifications
            // 
            listNotifications.Font = new Font("Segoe UI", 10F);
            listNotifications.FormattingEnabled = true;
            listNotifications.ItemHeight = 23;
            listNotifications.Location = new Point(20, 70);
            listNotifications.Name = "listNotifications";
            listNotifications.Size = new Size(860, 480);
            listNotifications.TabIndex = 1;

            // 
            // timerStatus
            // 
            timerStatus.Enabled = true;
            timerStatus.Interval = 1000;
            timerStatus.Tick += (s, e) => { lblDateTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"); };

            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 752);
            Controls.Add(pnlMain);
            Controls.Add(statusStrip);
            Controls.Add(menuStrip);
            Icon = ((Icon)(resources.GetObject("$this.Icon")));
            MainMenuStrip = menuStrip;
            MinimumSize = new Size(1000, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SCTMS - Safety Compliance Training Management System";
            WindowState = FormWindowState.Maximized;

            // Resume layout
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            pnlMain.ResumeLayout(false);
            pnlWelcome.ResumeLayout(false);
            pnlWelcome.PerformLayout();
            pnlQuickActions.ResumeLayout(false);
            pnlQuickActions.PerformLayout();
            pnlNotifications.ResumeLayout(false);
            pnlNotifications.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip;
        private ToolStripMenuItem menuFile;
        private ToolStripMenuItem menuExit;
        private ToolStripMenuItem menuManagement;
        private ToolStripMenuItem menuUserManagement;
        private ToolStripMenuItem menuTrainingManagement;
        private ToolStripMenuItem menuView;
        private ToolStripMenuItem menuComplianceDashboard;
        private ToolStripMenuItem menuReports;
        private ToolStripMenuItem menuTrainingAssignments;
        private ToolStripMenuItem menuNotifications;
        private ToolStripMenuItem menuMyProfile;
        private ToolStripMenuItem menuTools;
        private ToolStripMenuItem menuSystemSettings;
        private ToolStripMenuItem menuHelp;
        private ToolStripMenuItem menuAbout;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblComplianceStatus;
        private ToolStripStatusLabel lblCurrentUser;
        private ToolStripStatusLabel lblDateTime;
        private Panel pnlMain;
        private Panel pnlWelcome;
        private Label lblWelcome;
        private Label lblDepartment;
        private Label lblRole;
        private Label lblLastLogin;
        private Panel pnlQuickActions;
        private Label lblQuickActions;
        private Button btnMyTrainings;
        private Button btnComplianceStatus;
        private Button btnNotifications;
        private Button btnReports;
        private Panel pnlNotifications;
        private Label lblRecentNotifications;
        private ListBox listNotifications;
        private System.Windows.Forms.Timer timerStatus;
    }
} 