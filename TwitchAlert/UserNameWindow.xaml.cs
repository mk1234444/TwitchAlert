using System;
using System.Windows;
using System.Windows.Input;

namespace TwitchAlert
{
    /// <summary>
    /// Interaction logic for UserNameWindow.xaml
    /// </summary>
    public partial class UserNameWindow : Window
    {
        public event Action<string>Check;
        public UserNameWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Check != null)
                Check(txtUserName.Text);
        }

        private void txtUserName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) this.Close();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            txtUserName.Focus();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            txtUserName.Text = Properties.Settings.Default.settingsMe;
        }
    }
}
