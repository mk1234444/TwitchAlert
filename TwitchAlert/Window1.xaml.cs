using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using TwitchAlert.classes;

namespace TwitchAlert
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        System.Windows.Forms.MenuItem miShowVB;
        DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };

        public Window1(Border toastBorder,System.Windows.Forms.MenuItem miShowVB, MainWindow.Toast toast)
        {
            this.miShowVB = miShowVB;
            InitializeComponent();
            rect1.Fill = new VisualBrush() { Visual = toastBorder};
            timer.Tick += (s, e) =>
              {
                txtIsPopupCycleRunning.Text = MKTwitch.IsPopupCycleRunning ? "true" : "false";
                if(MKTwitch.MKTwitchTimer!=null)
                    txtMKTwitchTimerEnabled.Text = MKTwitch.MKTwitchTimer.IsEnabled ? "true" : "false";
                  txtTopPos.Text = toast.TopPosition.ToString();
                  txtBottomPos.Text = toast.BottomPosition.ToString();
                  txtLeftPos.Text = toast.LeftPosition.ToString();

              };
            timer.Start();
        }

        public void BringToFront()
        {
            this.Activate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
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
