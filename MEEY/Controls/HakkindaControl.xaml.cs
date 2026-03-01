using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MEEY.Controls
{
    public partial class HakkindaControl : UserControl
    {
        public HakkindaControl()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
