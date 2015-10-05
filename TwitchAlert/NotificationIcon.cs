using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;
using TwitchAlert.classes;
using System;

namespace TwitchAlert
{
    public partial class MainWindow
    {
        ContextMenu contextMenu;
        MenuItem miNIOnline;
        MenuItem miNIQuit;
        MenuItem miNIUserName;
        NotifyIcon notifyIcon;

        public void SetupNotificationIcon()
        {
            // Create ContextMenu
            contextMenu = new ContextMenu();
            //Create MenuItems
            miNIOnline = new MenuItem { Text = "Who's Online?", Name="miNIOnline"};
            miNIUserName = new MenuItem { Text = "Change Username", Name = nameof(miNIUserName) };
            miNIQuit = new MenuItem { Text = "Quit", Name = "miNIQuit" };

            // Attach events to MenuItems
            miNIQuit.Click += (s, e) => {
                this.Close();
            };

            miNIUserName.Click += (s, e) =>
            {
                GetUserName();

            };

            miNIOnline.Click +=  (s,e) =>
            {
                ShowOnlineUsers();
            };
 

            contextMenu.MenuItems.AddRange(new MenuItem[] { miNIOnline,miNIUserName, miNIQuit });
            notifyIcon = new NotifyIcon() { Icon = Properties.Resources._48_twitch, Text="Twitch Alert", Visible=true};
            notifyIcon.DoubleClick += (s, e) => {
                ShowOnlineUsers();
            };
            notifyIcon.ContextMenu = contextMenu;
        }

        /// <summary>
        /// Opens UserName entry window for user to enter name.
        /// If the name is a valid Twitch user name then change MKTwitch
        /// over to this new userName. If the name entered is not a valid
        /// Twitch user then no changes are made in MKTwitch
        /// </summary>
        /// <returns>bool indicating whether the MKTwitch use name was actually changed</returns>
        private void GetUserName()
        {
            var userNameWindow = new UserNameWindow();
            Action<string> checkHandler = null;
            
            checkHandler = async (newUserName) =>
            {
                if (!string.IsNullOrEmpty(newUserName))
                {
                    if (!MKTwitch.UserExists(newUserName)) return;
                    USER_NAME = newUserName;
                    if (MKTwitch.IsStarted)
                    {
                        await MKTwitch.ChangeUser(USER_NAME);
                        notifyIcon.Text = $"TwitchAlert ({USER_NAME})\nFollowing {MKTwitch.followedUsers.Count} ({MKTwitch.followedUsers.Count(i => i.IsStreaming)} Online)";
                    }
                }
                userNameWindow.Check -= checkHandler;
            };
            userNameWindow.Check += checkHandler;
            userNameWindow.ShowDialog();
        }

        private async Task ShowOnlineUsers()
        {
            //MKTwitch.ShowingOnlineUsers = true;
            int count = 0;
            foreach (var user in MKTwitch.followedUsers.Where(i => i.IsStreaming))
            {
                count++;
                // MKTwitch.TriggerOnline(user);
                PlayOnlineSound();
                FillInToast(user);
                await DisplayToast();
                //await Task.Delay(6000);
            }

            if (count == 0)
                DisplayToast(true);
            //MKTwitch.ShowingOnlineUsers = false;
        }

        public void DisposeNotifyIcon()
        {
            if (miNIOnline != null) miNIOnline.Dispose();
            if (miNIQuit != null) miNIQuit.Dispose();
            if (contextMenu != null) contextMenu.Dispose();
            if (notifyIcon != null) notifyIcon.Dispose();
            notifyIcon = null;
        }
    }
}