using SCTMS.Models;

namespace SCTMS.Forms
{
    public partial class LoginForm : Form, IFormHandler
    {
        private User? _currentUser;

        public LoginForm()
        {
            InitializeComponent();
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
        }

        private void InitializeComponent()
        {
            this.Text = "Login Form";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
        }
    }
} 