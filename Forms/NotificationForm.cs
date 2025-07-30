using SCTMS.Models;

namespace SCTMS.Forms
{
    public partial class NotificationForm : Form, IFormHandler
    {
        private User? _currentUser;

        public NotificationForm()
        {
            InitializeComponent();
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
        }

        private void InitializeComponent()
        {
            this.Text = "Notifications";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
        }
    }
} 