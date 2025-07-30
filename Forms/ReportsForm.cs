using SCTMS.Models;

namespace SCTMS.Forms
{
    public partial class ReportsForm : Form, IFormHandler
    {
        private User? _currentUser;

        public ReportsForm()
        {
            InitializeComponent();
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
        }

        private void InitializeComponent()
        {
            this.Text = "Reports";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
        }
    }
} 