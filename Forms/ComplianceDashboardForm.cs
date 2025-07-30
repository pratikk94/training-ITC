using SCTMS.Models;

namespace SCTMS.Forms
{
    public partial class ComplianceDashboardForm : Form, IFormHandler
    {
        private User? _currentUser;

        public ComplianceDashboardForm()
        {
            InitializeComponent();
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
        }

        private void InitializeComponent()
        {
            this.Text = "Compliance Dashboard";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
        }
    }
} 