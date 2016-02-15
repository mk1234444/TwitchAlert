using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;
using TwitchAlert.classes;
using System;
using System.Diagnostics;

namespace TwitchAlert
{
    public partial class MainWindow
    {
        ContextMenu contextMenu;
        MenuItem miNIOnline;
        MenuItem miNIQuit;
        MenuItem miNIUserName;
        MenuItem miNITurnSoundOff;
        MenuItem miNISkipPopupsAtStart;
        MenuItem miNIGameStatusPopups;
        MenuItem miNIDebug;
        MenuItem miNIOpenLogFile;
        MenuItem miNITimerStatus;
        MenuItem miNIStartTimer;
        
        //MenuItem miNIRefreshFollowed;


        NotifyIcon notifyIcon;
        UserNameWindow userNameWindow;

        /// <summary>
        ///  WPF doesn't have a NotifyIcon so use the WinForms version
        /// </summary>
        public void SetupNotificationIcon()
        {
            // Create ContextMenu
            contextMenu = new ContextMenu();
            //Create MenuItems
            miNIOnline = new MenuItem { Text = "Who's Online?", Name="miNIOnline"};
            miNIUserName = new MenuItem { Text = "Change Username", Name = nameof(miNIUserName), Enabled = false };
            miNITurnSoundOff = new MenuItem { Text = "Sound Off", Name = nameof(miNITurnSoundOff)};
            miNISkipPopupsAtStart = new MenuItem { Text = "Skip Popups at Start", Name = nameof(miNISkipPopupsAtStart) };
            miNIGameStatusPopups = new MenuItem { Text = "Game and Status Changes", Name = nameof(miNIGameStatusPopups),Checked=true};
            miNIOpenLogFile = new MenuItem { Text = "Open Log File", Name = nameof(miNIOpenLogFile) };
            miNIDebug = new MenuItem { Text = "Debug", Name = nameof(miNIDebug) };
            miNITimerStatus = new MenuItem { Text = "Timer Status", Name = nameof(miNITimerStatus) };
            miNIStartTimer = new MenuItem { Text = "Start Timer", Name = nameof(miNIStartTimer) };
            miNIStartTimer.Click += (s, e) => MKTwitch.MKTwitchTimer.Start();
            miNIDebug.MenuItems.AddRange(new MenuItem[] { miNIOpenLogFile, miNIStartTimer, miNITimerStatus });
           
            miNIQuit = new MenuItem { Text = "Quit", Name = "miNIQuit" };
            // Attach events to MenuItems
            miNIQuit.Click += (s, e) => this.Close();
            miNIUserName.Click += (s, e) => GetUserName();
            miNIOnline.Click +=  (s, e) => ShowOnlineUsers();
            miNITurnSoundOff.Click += (s, e) => miNITurnSoundOff.Checked = !miNITurnSoundOff.Checked;
            miNISkipPopupsAtStart.Click += (s, e) => miNISkipPopupsAtStart.Checked = !miNISkipPopupsAtStart.Checked;
            miNIOpenLogFile.Click += (s, e) => Process.Start("log.txt");
            miNITimerStatus.Click += (s, e) => {
                var enabled = MKTwitch.IsTimerEnabled();
               // Log.WriteLog($"Timer Status = {(enabled?"Enabled":"Disabled")} * Last Pull = {lastPull}", "MKTwitchTimerLog.txt");
               // Process.Start("MKTwitchTimerLog.txt");
                MessageBox.Show($"Timer Status = {(enabled ? "Enabled" : "Disabled")}\nLast Pull = {lastPull}", "MKTwitchTimerLog.txt");

            };
 
            //miNIRefreshFollowed.Click += (s, e) => MKTwitch.UpdateFollowedUsers(USER_NAME);

            contextMenu.MenuItems.AddRange(new MenuItem[] { miNIOnline,miNIUserName, miNITurnSoundOff, miNISkipPopupsAtStart, miNIGameStatusPopups,miNIDebug, miNIQuit });
            notifyIcon = new NotifyIcon() { Icon = Properties.Resources._48_twitch, Text="Twitch Alert", Visible=true};
            notifyIcon.DoubleClick += (s, e) => ShowOnlineUsers();
            notifyIcon.Click += (s, e) => this.Focus();
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
            userNameWindow = new UserNameWindow();
            Action<string> newUserHandler = null;
            
            newUserHandler = async (newUserName) =>
            {
                if (string.IsNullOrEmpty(newUserName)) return;
               
                // If the entered username is already the current one or doesn't exist then do nothing.
                // NOTE: If username is invalid then maybe a MessageBox saying so or the UserNameWindow
                //       should be re-opened?
                if (newUserName == USER_NAME || !MKTwitch.UserExists(newUserName)) return;
                USER_NAME = newUserName;
                if (MKTwitch.IsStarted)
                {
                    MKTwitch.CancelPopupCycle = true;

                    notifyIcon.Text = $"TwitchAlert ({USER_NAME})\nRetrieving information...";


                    try
                    {
                        miNIUserName.Enabled = false;
                        await MKTwitch.ChangeUser(USER_NAME);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message,"TwitchAlert");
                    }
                    finally
                    {
                        miNIUserName.Enabled = true;
                    }


                    notifyIcon.Text = $"TwitchAlert ({USER_NAME})\nFollowing {MKTwitch.followedUsers.Count} ({MKTwitch.followedUsers.Count(i => i.IsStreaming)} Online)";
                }
                
                userNameWindow.NewUser -= newUserHandler;
                userNameWindow = null;
            };
            userNameWindow.NewUser += newUserHandler;
            userNameWindow.ShowDialog();
        }

        private async Task ShowOnlineUsers()
        {
            int count = 0;
            foreach (var user in MKTwitch.followedUsers.Where(i => i.IsStreaming))
            {
                count++;
                if (!MKTwitch.CancelPopupCycle)
                {
                    PlayOnlineSound();
                    FillInToast(user);
                    await DisplayToast();
                }
            }
            MKTwitch.CancelPopupCycle = false;
            if (count == 0)
                DisplayToast(true);
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