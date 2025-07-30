using SCTMS.Models;

namespace SCTMS.Forms
{
    public partial class UserManagementForm : Form, IFormHandler
    {
        private User? _currentUser;

        public UserManagementForm()
        {
            InitializeComponent();
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
        }

        private void InitializeComponent()
        {
            this.Text = "User Management";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
        }
    }
} 