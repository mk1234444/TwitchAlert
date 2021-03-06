﻿using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;
using TwitchAlert.classes;
using System;
using System.Diagnostics;
using System.Windows;

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
        MenuItem miNIStopTimer;
        MenuItem miOpenAppDirectory;
        MenuItem miGoToAllGames;
        MenuItem miNICentre;
        MenuItem miNIUseFirefox;
        MenuItem miShowVB;
        
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

            miNIDebug = new MenuItem { Text = "Debug", Name = nameof(miNIDebug) };
            miNIOpenLogFile = new MenuItem { Text = "Open Log File", Name = nameof(miNIOpenLogFile) };
            miOpenAppDirectory = new MenuItem { Text = "Open App Diectory", Name = nameof(miOpenAppDirectory) };
            miNITimerStatus = new MenuItem { Text = "Timer Status", Name = nameof(miNITimerStatus) };
            miNIStartTimer = new MenuItem { Text = "Start Timer", Name = nameof(miNIStartTimer) };
            miNIStopTimer = new MenuItem { Text = "Stop Timer", Name = nameof(miNIStopTimer) };
            miNIUseFirefox = new MenuItem { Text = "Use Firefox", Name = nameof(miNIUseFirefox)};
            miNICentre = new MenuItem { Text = "Centre", Name = nameof(miNICentre) };

            miShowVB = new MenuItem { Text = "Show Current Visual", Name = nameof(miShowVB)};
            miNIStartTimer.Click += (s, e) => MKTwitch.MKTwitchTimer.Start();
            miNIStopTimer.Click += (s, e) => MKTwitch.MKTwitchTimer.Stop();
            miNIDebug.MenuItems.AddRange(new MenuItem[] { miNIOpenLogFile, miOpenAppDirectory, miNIStartTimer, miNIStopTimer, miNITimerStatus, miShowVB, miNIUseFirefox, miNICentre });
            miGoToAllGames = new MenuItem { Text = "Goto All Games", Name = nameof(miGoToAllGames) };
            miNIQuit = new MenuItem { Text = "Quit", Name = "miNIQuit" };
            // Attach events to MenuItems
            miNIQuit.Click += (s, e) => this.Close();
            miNIUserName.Click += (s, e) => GetUserName();
            miNIOnline.Click +=  (s, e) => ShowOnlineUsers();
            miNITurnSoundOff.Click += (s, e) => miNITurnSoundOff.Checked = !miNITurnSoundOff.Checked;
            miNISkipPopupsAtStart.Click += (s, e) => miNISkipPopupsAtStart.Checked = !miNISkipPopupsAtStart.Checked;
            miNIUseFirefox.Click += (s, e) => miNIUseFirefox.Checked = !miNIUseFirefox.Checked;


            miNIOpenLogFile.Click += (s, e) => Process.Start("log.txt");
            miOpenAppDirectory.Click += (s, e) => Process.Start(Environment.CurrentDirectory);
            miNITimerStatus.Click += (s, e) => {
                var enabled = MKTwitch.IsTimerEnabled();
               // Log.WriteLog($"Timer Status = {(enabled?"Enabled":"Disabled")} * Last Pull = {lastPull}", "MKTwitchTimerLog.txt");
               // Process.Start("MKTwitchTimerLog.txt");
                System.Windows.MessageBox.Show($"Timer Status = {(enabled ? "Enabled" : "Disabled")}\nLast Pull = {lastPull}", "MKTwitchTimerLog.txt");
            };
            miNICentre.Click += (s, e) => {
                CentreToast();
                //    toast.LeftPosition = this.Left = (SystemParameters.WorkArea.Width / 2) - (this.Width / 2);
            };

            miShowVB.Click += miShowVB_Click;

            miGoToAllGames.Click += (s, e) => Process.Start(@"https://www.twitch.tv/directory");


            contextMenu.MenuItems.AddRange(new MenuItem[] { miNIOnline,miNIUserName, miNITurnSoundOff, miNISkipPopupsAtStart, miNIGameStatusPopups, miGoToAllGames, miNIDebug, miNIQuit });
            notifyIcon = new NotifyIcon() { Icon = Properties.Resources._48_twitch, Text="Twitch Alert", Visible=true};
            notifyIcon.DoubleClick += (s, e) => ShowOnlineUsers();
            notifyIcon.Click += (s, e) => this.Focus();
            notifyIcon.ContextMenu = contextMenu;
        }

        private void miShowVB_Click(object sender, EventArgs e)
        {
            ToggleCurrentVisualDisplay();
        }

        private void ToggleCurrentVisualDisplay()
        {
            if (win1 == null)
            {
                miShowVB.Text = "Hide Current Visual";
                win1 = new Window1(toastBorder, miShowVB,toast,this);

                EventHandler closed = null;
                closed = (s, e) =>
                {
                    this.Closed -= closed;
                    win1 = null;
                };

                win1.Closed += closed;
                win1.Show();
                win1.BringToFront();
            }
            else
            {
                miShowVB.Text = "Show Current Visual";
                win1.Close();
                win1 = null;
            }
        }



        /// <summary>
        /// Opens UserName entry window.
        /// If the name is a valid Twitch user name then change MKTwitch
        /// over to this new userName. If the name entered is not a valid
        /// Twitch user then no changes are made in the MKTwitch class
        /// </summary>
        /// <returns>bool indicating whether the MKTwitch user name was actually changed</returns>
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
                if (newUserName == USER_NAME || !await MKTwitch.UserExists(newUserName)) return;

          

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
                       System.Windows.MessageBox.Show(ex.Message,"TwitchAlert");
                    }
                    finally
                    {
                        miNIUserName.Enabled = true;
                    }


                    notifyIcon.Text = $"TwitchAlert ({USER_NAME})\nFollowing {MKTwitch.followedStreamers.Count} ({MKTwitch.followedStreamers.Count(i => i.IsStreaming)} Online)";
                }
                
                userNameWindow.NewUser -= newUserHandler;
                userNameWindow = null;
            };
            userNameWindow.NewUser += newUserHandler;
            userNameWindow.ShowDialog();
        }

        private async Task ShowOnlineUsers()
        {
            if (win1 != null) win1.Activate();
            // If we are already in the middle of a popup cycle then do nothing
            if (MKTwitch.IsPopupCycleRunning) return;

            int count = 0;
            MKTwitch.IsPopupCycleRunning = true;
            var isStreaming = MKTwitch.followedStreamers.Where(i => i.IsStreaming).ToList();
            if (isStreaming.Count==0)
            {
                PlayOnlineSound();
               // FillInToast();
                await DisplayToast(true);
            }
            else
            {
                foreach (var user in isStreaming)
                {
                    count++;
                    if (!MKTwitch.CancelPopupCycle)
                    {
                        PlayOnlineSound();
                        FillInToast(user);
                        await DisplayToast();
                    }
                }
            }
            MKTwitch.CancelPopupCycle = false;
            MKTwitch.IsPopupCycleRunning = false;
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