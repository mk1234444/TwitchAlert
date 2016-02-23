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
        }
    }
}
