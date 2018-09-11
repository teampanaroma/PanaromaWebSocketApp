using System.Windows.Forms;

namespace ApplicationUpdater
{
    public partial class WaitingForm : Form
    {
        public WaitingForm(string contentText)
        {
            InitializeComponent();
            lblContent.Text = contentText;
        }
    }
}