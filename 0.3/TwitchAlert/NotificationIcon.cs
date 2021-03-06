﻿using System.Windows.Forms;
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

            miNIUserName.Click += (s, e) => {
                var userNameWindow = new UserNameWindow();
                Action<string>checkHandler = null;
                checkHandler = async(newUserName) => {
                    if (!string.IsNullOrEmpty(newUserName))
                    {
                        USER_NAME = newUserName;
                        await MKTwitch.ChangeUser(USER_NAME);
                    }
                    userNameWindow.Check -= checkHandler;
                };
                userNameWindow.Check += checkHandler;
                userNameWindow.ShowDialog();

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