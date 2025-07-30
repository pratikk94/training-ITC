using SCTMS.Models;

namespace SCTMS.Forms
{
    public partial class TrainingManagementForm : Form, IFormHandler
    {
        private User? _currentUser;

        public TrainingManagementForm()
        {
            InitializeComponent();
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
        }

        private void InitializeComponent()
        {
            this.Text = "Training Management";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
        }
    }
} 