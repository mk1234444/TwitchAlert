using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TwitchAlert
{
    /// <summary>
    /// Interaction logic for UserNameWindow.xaml
    /// </summary>
    public partial class UserNameWindow : Window
    {
        public event Action<string>NewUser;
        public UserNameWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (NewUser != null)
                NewUser(txtUserName.Text);
        }

        private void txtUserName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Console.WriteLine($"{nameof(UserNameWindow)} KeyUp event called with a keypress of ENTER");
                this.Close();
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            txtUserName.Focus();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
#if debug
            txtUserName.Text = Properties.Settings.Default.settingsMe;
            await Task.Delay(500);
#endif
        
            Console.WriteLine($"{nameof(UserNameWindow)} MouseDoubleClick event called");
            this.Close();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
        }
    }
}
