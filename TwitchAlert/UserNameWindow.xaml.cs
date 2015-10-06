using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
    }
}
