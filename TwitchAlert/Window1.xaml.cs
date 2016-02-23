using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TwitchAlert
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        System.Windows.Forms.MenuItem miShowVB;

        public Window1(Border toastBorder,System.Windows.Forms.MenuItem miShowVB)
        {
            this.miShowVB = miShowVB;
            InitializeComponent();
            rect1.Fill = new VisualBrush() { Visual = toastBorder};
        }

        public void BringToFront()
        {
            this.Activate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            miShowVB.Text = "Show Current Visual";
            Properties.Settings.Default.settingsVBWindowLeft = this.Left;
            Properties.Settings.Default.settingsVBWindowTop = this.Top;
            Properties.Settings.Default.Save();
        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            this.Left = Properties.Settings.Default.settingsVBWindowLeft;
            this.Top = Properties.Settings.Default.settingsVBWindowTop;
        }
    }
}
