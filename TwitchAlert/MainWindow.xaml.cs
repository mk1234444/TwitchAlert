// 0.4.8
// FIX:  Update AssemblyInfo.cs to the correct version number
// FIX:  Twitch now requires Client-ID to be passed to any API calls
// Added:Toast now displays the number of followed Twitchers that are currently streaming in the top right 
// DONE: Convert Json property names to .Net naming convention

// TODO: If the streamers name doesnt fit then either make the font smaller or add a tooltip
// TODO: If Status/Games changes in timer tick and a cycle is already in progress then the update may be
//       displayed using the previous streamers name  DONE????? NO!!
// TODO: Use the ToastBorder in the SlideUp and SlideDown animations. Currently the rootGrid is being
//       used and when that is collapsed it leaves then empty red ToastBorder still hanging
// TODO: Sort out the toast positioning for hidden taskbar/small taskbar/large taskbar
// FIX:  The 'Updating' message obscures the streaming number in the top right.
// FIX:  The number if Twitchers streaming indicator at the top right is collapsed when
//       nobody is online
// FIX:  The 'Started' time now takes into account daylight saving in UK
// FIX:  Stopped working. Started getting 400 Bad Request errors when trying to get Streamers. (removing
//       'wRequest.ContentType = "application/json";' from the Get() and GetAsync() methods fixed it.)
// FIX:  KludgeTimer added to restart the main timer when it becomes permenantly disabled after some
//       network error.Stopgap fix. Needs fixing properly
// Add:  Added 'Goto All Games' to the NotifyIcon Menu
// Add:  Added 'Centre' MenuItem to the NotifyIcon Menu, under Debug
// FIX:  Now detects when screen resolution changes. *TEST*
// FIX:  Twitch started to sometimes return 0 followed streams. If this happens then 
//       UpdateFollowedUsers(Twitch.Root user) simply returns without changing anything
// FIX:  Fixed problem of thumbnails not being downloaded anymore by forcingTls12.
//       Article with fix https://github.com/google/google-api-dotnet-client/issues/911
// TODO: Debug/Center doesn't work on resolutions other than 1920x1080
// Add:  Now centres Toast on first run


using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Linq;
using TwitchAlert.classes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.Win32;
using System.Net;

namespace TwitchAlert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static class NativeMethods
        {
            public const Int32 MONITOR_DEFAULTTOPRIMARY = 0x00000001;
            public const Int32 MONITOR_DEFAULTTONEAREST = 0x00000002;

            [DllImport("user32.dll")]
            public static extern IntPtr MonitorFromWindow(IntPtr handle, Int32 flags);

            public static bool IsInPrimaryMonitor(MainWindow window)
            {
                var hwnd = new WindowInteropHelper(window).EnsureHandle();
                var currentMonitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);
                var primaryMonitor = NativeMethods.MonitorFromWindow(IntPtr.Zero, NativeMethods.MONITOR_DEFAULTTOPRIMARY);
                return currentMonitor == primaryMonitor;
            }
        }


        public class Toast:DependencyObject
        {
           public Toast()
            {
                Link = "";
            }

            /// <summary>
            /// The Top position the Toast will slide up to when being displayed, ex. on a 1080 display
            /// this value will be 980
            /// </summary>
            public double TopPosition
            {
                get { return (double)GetValue(TopPositionProperty); }
                set { SetValue(TopPositionProperty, value); }
            }

            /// <summary>
            /// The Top position the Toast will slide down to when being hidden, ex. on a 1080 display
            /// this value will be 1080
            /// </summary>
            public double BottomPosition
            {
                get { return (double)GetValue(BottomPositionProperty); }
                set { SetValue(BottomPositionProperty, value); }
            }
            public double LeftPosition
            {
                get { return (double)GetValue(LeftPositionProperty); }
                set { SetValue(LeftPositionProperty, value); }
            }

            public string DisplayName
            {
                get { return (string)GetValue(DisplayNameProperty); }
                set { SetValue(DisplayNameProperty, value); }
            }

            public int NumberLiveStreaming
            {
                get { return (int)GetValue(NumberLiveStreamingProperty); }
                set { SetValue(NumberLiveStreamingProperty, value); }
            }

            public string Game
            {
                get { return (string)GetValue(GameProperty); }
                set { SetValue(GameProperty, value); }
            }

            public bool IsLive
            {
                get { return (bool)GetValue(IsLiveProperty); }
                set { SetValue(IsLiveProperty, value); }
            }

            public int Viewers
            {
                get { return (int)GetValue(ViewersProperty); }
                set { SetValue(ViewersProperty, value); }
            }

            public BitmapImage Thumbnail
            {
                get { return (BitmapImage)GetValue(ThumbnailProperty); }
                set { SetValue(ThumbnailProperty, value); }
            }

            public string StreamCreatedAt
            {
                get { return (string)GetValue(StreamCreatedAtProperty); }
                set { SetValue(StreamCreatedAtProperty, $"Started: {value} ({(Viewers>1000? ((float)Viewers/1000).ToString("F1") + "k" : Viewers.ToString())} Viewers)");}
            }

            public string Link
            {
                get { return (string)GetValue(LinkProperty); }
                set { SetValue(LinkProperty, value); }
            }

            public string Status
            {
                get { return (string)GetValue(StatusProperty); }
                set { SetValue(StatusProperty, value); }
            }

            // Using a DependencyProperty as the backing store for Status.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty StatusProperty = DependencyProperty.Register("Status", typeof(string), typeof(Toast), new PropertyMetadata(null));
            // Using a DependencyProperty as the backing store for Link.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty LinkProperty = DependencyProperty.Register("Link", typeof(string), typeof(Toast), new PropertyMetadata(null));
            // Using a DependencyProperty as the backing store for TopPosition.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty TopPositionProperty = DependencyProperty.Register("TopPosition", typeof(double), typeof(Toast), new PropertyMetadata(0.0));
            // Using a DependencyProperty as the backing store for BottomPosition.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty BottomPositionProperty = DependencyProperty.Register("BottomPosition", typeof(double), typeof(Toast), new PropertyMetadata(0.0));
            // Using a DependencyProperty as the backing store for LeftPosition.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty LeftPositionProperty = DependencyProperty.Register("LeftPosition", typeof(double), typeof(Toast), new PropertyMetadata(0.0));
            // Using a DependencyProperty as the backing store for StreamCreatedAt.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty StreamCreatedAtProperty = DependencyProperty.Register("StreamCreatedAt", typeof(string), typeof(Toast), new PropertyMetadata(null));
            // Using a DependencyProperty as the backing store for Thumbnail.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty ThumbnailProperty = DependencyProperty.Register("Thumbnail", typeof(BitmapImage), typeof(Toast), new PropertyMetadata(null));
            // Using a DependencyProperty as the backing store for Viewers.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty ViewersProperty = DependencyProperty.Register("Viewers", typeof(int), typeof(Toast), new PropertyMetadata(null));
            // Using a DependencyProperty as the backing store for IsLive.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty IsLiveProperty = DependencyProperty.Register("IsLive", typeof(bool), typeof(Toast), new PropertyMetadata(false));
            // Using a DependencyProperty as the backing store for Game.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty GameProperty = DependencyProperty.Register("Game", typeof(string), typeof(Toast), new PropertyMetadata(null));
            // Using a DependencyProperty as the backing store for DisplayName.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register("DisplayName", typeof(string), typeof(Toast), new PropertyMetadata(null));
            // Using a DependencyProperty as the backing store for NumberLiveStreaming.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty NumberLiveStreamingProperty = DependencyProperty.Register("NumberLiveStreaming", typeof(int), typeof(Toast), new PropertyMetadata(null));
        }
        Toast toast = new Toast();

       // DispatcherTimer mkTwitchTimerStatusTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };

        const int NOBODY_ONLINE_HEIGHT = 100;
        string USER_NAME = Properties.Settings.Default.settingsUserName;
        DateTime lastPull;
        Storyboard ResetToastPosition, SlideUpStoryboard, SlideDownStoryboard, GameOutStoryboard, GameIn2Storyboard, StatusOutStoryboard, StatusInStoryboard;
      

        Window1 win1;

        public MainWindow()
        {
            // Needed to allow thumbnails to be downloaded
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            InitializeComponent();

            // Detect any screen resolution change
            SystemEvents.DisplaySettingsChanged += (s, e) => {
                setToastTopAndBottomPositions();
                Console.WriteLine("**** Screen Resolution Changed ****");
                Console.WriteLine($"toast.TopPosition = {toast.TopPosition}  toast.BottomPosition = {toast.BottomPosition}  this.Top = {this.Top}");
                StartAnimationAsync(ResetToastPosition);
            };

            this.DataContext = toast;
            SetupNotificationIcon();

            SlideUpStoryboard = FindResource("SlideUp") as Storyboard;
            SlideDownStoryboard = FindResource("SlideDown") as Storyboard;
            GameOutStoryboard = FindResource("GameOut") as Storyboard;
            GameIn2Storyboard = FindResource("GameIn2") as Storyboard;
            StatusInStoryboard = FindResource("StatusIn") as Storyboard;
            StatusOutStoryboard = FindResource("StatusOut") as Storyboard;
            ResetToastPosition = FindResource("ResetToastPosition") as Storyboard;

            // Subscribe to the MKTwitch Online event so we hear about any user
            // that starts streaming live
            MKTwitch.Online += async(s, e)=>{
                if (e.DisplayToast)
                {
                    FillInToast(e.User);
                    PlayOnlineSound();
                    await DisplayToast();
                }
                // Display the Online followedUser count in the NotifyIcon Tooltip
                notifyIcon.Text = $"TwitchAlert ({USER_NAME})\nFollowing {MKTwitch.followedStreamers.Count} ({MKTwitch.followedStreamers.Count(i => i.IsStreaming)} Online)";
                Log.WriteLog($"{e.User.Name} went ONLINE at {e.User.StreamCreatedAt} playing {e.User.Game}");
            };

            // Subscribe to the MKTwitch Offline event so we hear about
            // any streamer who stops streaming
            MKTwitch.OffLine += async(s, e) => {
                if (e.DisplayToast)
                {
                    FillInToast(e.User);
                    // Display the Online followedUser count in the NotifyIcon Tooltip
                    PlayOfflineSound();
                    await DisplayToast();
                }
                notifyIcon.Text = $"TwitchAlert ({USER_NAME})\nFollowing {MKTwitch.followedStreamers.Count} ({MKTwitch.followedStreamers.Count(i => i.IsStreaming)} Online)";
                Log.WriteLog($"{e.User.Name} went OFFLINE at {DateTime.Now.ToShortTimeString()} playing {e.User.Game}");
            };

            // Subscribe to the MKTwitch UpdateStarted event so we hear about
            // when we've started updating our Twitch info
            MKTwitch.UpdateStarted += (s, e) => txtUpdating.Visibility = Visibility.Visible;

            // Subscribe to the MKTwitch UpdateStarted event so we hear about
            // when we've completed updating our Twitch info
            MKTwitch.UpdateCompleted += (s, e) => {
                txtUpdating.Visibility = Visibility.Collapsed;
                lastPull = MKTwitch.LastPull = DateTime.Now;
            };


            // Subscribe to the MKTwitch FollowedUsersChanged event so we hear about
            // when its collection of FollowedUsers has changed
            MKTwitch.FollowedUsersChanged += (s, e) => {
                Console.WriteLine("\nfollowedUsers Changed");
                notifyIcon.Text = $"TwitchAlert ({USER_NAME})\nFollowing {MKTwitch.followedStreamers.Count} ({MKTwitch.followedStreamers.Count(i => i.IsStreaming)} Online)";
            };

            // Subscribe to the MKTwitch StartCompleted event so we hear when its
            // Start() method has completed
            MKTwitch.StartCompleted += (s, e) => { miNIUserName.Enabled = true; }; // mkTwitchTimerStatusTimer.Start(); };

            // Subscribe to the MKTwitch GameChanged event so we hear
            // when a Streamer changes their Game
            MKTwitch.GameChanged += async (s, e) => {
                if (!miNIGameStatusPopups.Checked) return;
                var user = e.User;
                toast.DisplayName = user.Name;
                toast.Game = e.OldGame;
                toast.Viewers = user.NumViewers;
                toast.StreamCreatedAt = user.StreamCreatedAt;
                toast.IsLive = user.IsStreaming;
                toast.Thumbnail = user.Thumbnail;
                toast.Link = user.Link;
                toast.Status = user.Status;
                Log.WriteLog($"{e.User.Name} changed GAME from {e.OldGame} to {e.NewGame}");
                Log.WriteLog($"{e.User.Name} e.OldGame: {e.OldGame} e.NewGame: {e.NewGame} e.User.Game: {e.User.Game}");
                await DisplayGameChangeToast(e.NewGame);
            };

            // Subscribe to the MKTwitch StatusChanged event so we hear
            // when a Streamer changes their Status message
            MKTwitch.StatusChanged += async(s, e) => {
                if (!miNIGameStatusPopups.Checked) return;
                var user = e.User;
                toast.DisplayName = user.Name;
                toast.Game = user.Game;
                toast.Status = e.OldStatus;
                toast.Viewers = user.NumViewers;
                toast.StreamCreatedAt = user.StreamCreatedAt;
                toast.IsLive = user.IsStreaming;
                toast.Thumbnail = user.Thumbnail;
                toast.Link = user.Link;
                Log.WriteLog($"{e.User.Name} changed STATUS from {e.OldStatus} to {e.NewStatus}");
                await DisplayStatusChangeToast(e.NewStatus);
            };

            // Subscribe to the MKTwitch Followed event so we hear
            // when the user follows a new Streamer
            MKTwitch.Followed += (s, e) =>
            {
                if (!MKTwitch.IsStarted || MKTwitch.IsChangingUser) return;
                Console.WriteLine($"\nNow following {e.User.Name}");
                PlayFollowedSound();
            };

            // Subscribe to the MKTwitch UnFollowed event so we hear
            // when the user UNfollows a Streamer
            MKTwitch.Unfollowed += (s, e) =>
            {
                if (!MKTwitch.IsStarted || MKTwitch.IsChangingUser) return;
                Console.WriteLine($"\nUnfollowing {e.User.Name}");
                PlayUnfollowedSound();
            };

            // If we dont have a username then keep asking till we get one
            while (string.IsNullOrEmpty(USER_NAME))
            {
                GetUserName();
                //notifyIcon.Text = $"TwitchAlert ({USER_NAME})\nFollowing {MKTwitch.followedUsers.Count} ({MKTwitch.followedUsers.Count(i => i.IsStreaming)} Online)";
            }
            notifyIcon.Text = $"TwitchAlert ({USER_NAME})\nRetrieving information...";
            StartEngine();
        }

        private async Task StartEngine()
        {
            try
            {
                await MKTwitch.Start(USER_NAME, Properties.Settings.Default.settingsSkipPopups);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.WriteLog(ex.Message);
                Log.WriteLog(ex.StackTrace);
            }
            finally
            {
                if (MKTwitch.IsStarted == false)
                {
                    Console.WriteLine(notifyIcon.Text = "Failed to start the MKTwitch engine. Retrying in 10 seconds");

                    try
                    {
                        await Dispatcher.BeginInvoke(new Action(() =>
                        {
                            // NotifyIcon text cannot be over 64 chars??? WTF?
                            notifyIcon.Text = $"Failed to start the MKTwitch engine.\nRetrying in 10s";
                        }));
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show($"Error setting NotifyIcon.Text {ex.Message}");
                    }

                    await Task.Delay(10000);
                    Dispatcher.BeginInvoke(new Action(() => {
                        notifyIcon.Text = $"TwitchAlert ({USER_NAME})\nRetrieving information...";
                    }));
                    StartEngine();
                }
            }
        }

        /// <summary>
        /// Calculates the Toasts Top and Bottom positions from the Screen resolution
        /// </summary>
        private void setToastTopAndBottomPositions()
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            var primary = screens.First(i => i.Primary);
            var secondary = screens.FirstOrDefault(i => !i.Primary);
            var isPrimary = NativeMethods.IsInPrimaryMonitor(this);

           // var workingAreaWidth = isPrimary ? primary.WorkingArea.Width : secondary.WorkingArea.Width;
            var workingAreaHeight = isPrimary ? primary.WorkingArea.Height : secondary.WorkingArea.Height;
            toast.BottomPosition = workingAreaHeight;
            toast.TopPosition = SystemParameters.WorkArea.Height - (toastBorder.Height + 15);

            var s = $"{nameof(setToastTopAndBottomPositions)}: {nameof(toast.TopPosition)} = {toast.TopPosition} - {nameof(toast.BottomPosition)} = {toast.BottomPosition}";
            Console.WriteLine(s);
            Log.WriteLog(s, "info.txt");
        }

        #region Windows Events
        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Seperator();

            var screens = System.Windows.Forms.Screen.AllScreens;
            var primary = screens.First(i => i.Primary);
            var secondary = screens.FirstOrDefault(i => !i.Primary);
            var isPrimary = NativeMethods.IsInPrimaryMonitor(this);

            var workingAreaWidth = isPrimary ? primary.WorkingArea.Width : secondary.WorkingArea.Width;
            var workingAreaHeight = isPrimary ? primary.WorkingArea.Height : secondary.WorkingArea.Height;
            var screenHeight = isPrimary ? primary.Bounds.Y : secondary.Bounds.Y;
            var screenWidth = isPrimary ? primary.Bounds.X : secondary.Bounds.X;

            this.Top = toast.BottomPosition = SystemParameters.WorkArea.Height;
            this.Top = toast.BottomPosition = SystemParameters.PrimaryScreenHeight; //DEBUG STUFF
            //this.Top = toast.BottomPosition = workingAreaHeight;
            setToastTopAndBottomPositions();
        

            //Console.WriteLine($"Loaded - this.Top {this.Top}");
            //------------------
            // toast.TopPosition = SystemParameters.WorkArea.Height - (toastBorder.ActualHeight);
            //------------------
            //Console.WriteLine("Window_Loaded event:");
            //Console.WriteLine($"\tsettings.Left = {Properties.Settings.Default.settingsLeft}");
            //Console.WriteLine($"\tsettings.UserName = {Properties.Settings.Default.settingsUserName}");

            // If this is the first time we've been run then centre the toast based on the current screen width
            if (Properties.Settings.Default.settingsLeft == 0.1)
            {
                //toast.LeftPosition = this.Left = SystemParameters.WorkArea.Width - this.Width;
                // Using Screens class
                //toast.LeftPosition = this.Left = workingAreaWidth - this.Width;
                CentreToast();
            }
            // If Left was saved previously off the right hand side of the screen then reset it so that the full popup will be displayed at the right
            else if (Properties.Settings.Default.settingsLeft > (SystemParameters.WorkArea.Width - this.Width))
            {
                toast.LeftPosition = this.Left = SystemParameters.WorkArea.Width - this.Width;
                // Using Screens class
                toast.LeftPosition = this.Left = workingAreaWidth - this.Width;
            }

            //-----------------------



            // If the Left was saved previously off the left side of the screen then reset is so the full popup will be displayed at the left
            else if (Properties.Settings.Default.settingsLeft < 0)
                toast.LeftPosition = this.Left;
            //toast.LeftPosition = this.Left = -15;       // The 15 takes into account the offset of the rootBorder to the main window
            else
                toast.LeftPosition = this.Left = Properties.Settings.Default.settingsLeft;
     
            miNITurnSoundOff.Checked = Properties.Settings.Default.settingsSoundOff;
            miNISkipPopupsAtStart.Checked = Properties.Settings.Default.settingsSkipPopups;
            miNIUseFirefox.Checked = Properties.Settings.Default.settingsUseFirefox;

        }

        private void window_ContentRendered(object sender, EventArgs e)
        {
            this.Top = toast.BottomPosition = SystemParameters.WorkArea.Height;
            this.Top = toast.BottomPosition = SystemParameters.PrimaryScreenHeight; // DEBUG STUFF
            //Console.WriteLine($"ContentRendered - this.Top {this.Top}");
            //Console.WriteLine($"ContentRendered - TopPosition = {toast.TopPosition}");
        }

        private void window_Closing(object sender, CancelEventArgs e)
        {
            if (win1 != null) win1.Close();
            if (userNameWindow != null) userNameWindow.Close();

            Properties.Settings.Default.settingsLeft = this.Left;
            if(!string.IsNullOrEmpty(USER_NAME))
                Properties.Settings.Default.settingsUserName = USER_NAME;
            Properties.Settings.Default.settingsSoundOff = miNITurnSoundOff.Checked;
            Properties.Settings.Default.settingsSkipPopups = miNISkipPopupsAtStart.Checked;
            Properties.Settings.Default.settingsUseFirefox = miNIUseFirefox.Checked;
            Properties.Settings.Default.Save();
            DisposeNotifyIcon();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+ Shift+ C centers the popup
            if ((Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.LeftCtrl)) && e.Key == Key.C)
                toast.LeftPosition = this.Left = (SystemParameters.WorkArea.Width / 2) - (this.Width / 2);
            else if (e.Key == Key.Escape)
                MKTwitch.CancelPopupCycle = true;

            // F1,F2 and F3 used here to test stuff
            else if (e.Key == Key.F1)
            {
                DisplayGameChangeToast("SomeNewGame");
            }
            else if (e.Key == Key.F2)
            {
                DisplayStatusChangeToast("SomeNewStatus");
            }
            else if (e.Key == Key.F3)
                PlayNewGameSound();
            else if (Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.L)
                Process.Start("log.txt");
        }
        #endregion

        #region Event Handlers
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (miNIUseFirefox.Checked)
            {
                Process.Start("Firefox.exe",e.Uri.AbsoluteUri);
            }
            else
                Process.Start(e.Uri.AbsoluteUri);
        }

        private void toastBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //var top = this.Top;
            // Console.WriteLine($"toastBorder.ActualHeight = {(sender as Border).ActualHeight}\ntoastBorder.Height = {(sender as Border).Height}");
            toast.TopPosition = SystemParameters.WorkArea.Height - ((sender as Border).Height + 15);
        }

        #region Tooltip Event Handlers
        private void txtStatus_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            bool trimmed = CalculateIsTextTrimmedMultiline(sender as TextBlock);
            if (trimmed)
            {
                var tt = Resources["StatusTooltip"] as ToolTip;
                // Find the TextBlock from within the Tooltip Template that will display our Status text
                var ttTextBlock = ((tt.Content as Border).Child as StackPanel).Children[1] as TextBlock;
                // Then give it the full Status text to display
                ttTextBlock.Text = ((sender as TextBlock).DataContext as Toast).Status;
            }

            // Set Handled if we dont need the Tooltip so no empty Tooltip appears
            e.Handled = !trimmed;
        }

        private void txtGame_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            var trimmed = CalculateIsTextTrimmed(sender as TextBlock);
            if (trimmed)
            {
                var tt = Resources["GameTooltip"] as ToolTip;
                // Find the TextBlock from within the Tooltip Template that will display our Game text
                var ttTextBlock = (tt.Content as Border).Child as TextBlock;
                // Then give it the full Game text to display
                ttTextBlock.Text = ((sender as TextBlock).DataContext as Toast).Game;
            }
            // Set Handled if we dont need the Tooltip so no empty Tooltip appears
            e.Handled = !trimmed;
        }

        private void txtDisplayName_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            var trimmed = CalculateIsTextTrimmed(txtDisplayNameDummy);
            //var trimmed = CalculateIsTextTrimmed(sender as TextBlock);
            if (trimmed)
            {
                var tt = Resources["DisplayNameTooltip"] as ToolTip;
                // Find the TextBlock from within the Tooltip Template that will display our Game text
                var ttTextBlock = (tt.Content as Border).Child as TextBlock;
                // Then give it the full Game text to display
                ttTextBlock.Text = ((sender as TextBlock).DataContext as Toast).DisplayName;
            }
            // Set Handled if we dont need the Tooltip so no empty Tooltip appears
            e.Handled = !trimmed;
        }
        #endregion
        #endregion

        private void FillInToast(User user)
        {
            toast.DisplayName = user.Name;
            toast.Game = user.Game;
            toast.Viewers = user.NumViewers;
            toast.StreamCreatedAt = user.StreamCreatedAt;
            toast.IsLive = user.IsStreaming;
            toast.Thumbnail = user.Thumbnail;
            toast.Link = user.Link;
            toast.Status = user.Status;
            toast.NumberLiveStreaming = MKTwitch.followedStreamers.Count(i => i.IsStreaming);
        }

        /// <summary>
        /// Displays the popup showing info of the Streamer. If AllOffline==true then 
        /// "Nobody Online" message is displayed in the popup instead
        /// </summary>
        /// <param name="AllOffline"></param>
        /// <returns></returns>
        public async Task DisplayToast(bool AllOffline = false)
        {
            if (win1 != null) win1.Activate();
            this.Topmost = true;
            if (AllOffline)
            {
                txtNobodyOnline.Visibility = Visibility.Visible;
                brdIsOnline.Visibility = Visibility.Collapsed;
                brdNumberOnline.Visibility = Visibility.Collapsed;
                // toast.TopPosition = SystemParameters.WorkArea.Height - (toastBorder.Height);
                toast.TopPosition = SystemParameters.WorkArea.Height - (NOBODY_ONLINE_HEIGHT);
                toast.TopPosition = SystemParameters.PrimaryScreenHeight - NOBODY_ONLINE_HEIGHT;

                var primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
                var workingAreaHeight = SystemParameters.WorkArea.Height;

                //Console.WriteLine($"TopPosition = {toast.TopPosition}");
                this.Top = SystemParameters.WorkArea.Height;
            }
            else
            {
                setToastTopAndBottomPositions();
            }
            await StartAnimationAsync(SlideUpStoryboard);
            await Task.Delay(4000);
            await StartAnimationAsync(SlideDownStoryboard);

            txtNobodyOnline.Visibility = Visibility.Collapsed;
            brdIsOnline.Visibility = Visibility.Visible;
            brdNumberOnline.Visibility = Visibility.Visible;
        }

        public async Task DisplayGameChangeToast(string newGame)
        {
            if (win1 != null) win1.Activate();
            toastBorder.BorderBrush = Brushes.Yellow;
            await StartAnimationAsync(SlideUpStoryboard);
            await StartAnimationAsync(GameOutStoryboard);
            toast.Game = newGame;
            await StartAnimationAsync(GameIn2Storyboard);
            PlayNewGameSound();
            await Task.Delay(3000);
            await StartAnimationAsync(SlideDownStoryboard);
            toastBorder.BorderBrush = Brushes.Red;
        }

        public async Task DisplayStatusChangeToast(string newStatus)
        {
            if (win1 != null) win1.Activate();
            toastBorder.BorderBrush = Brushes.Yellow;
            await StartAnimationAsync(SlideUpStoryboard);
            await StartAnimationAsync(StatusOutStoryboard);
            toast.Status = newStatus;
            PlayNewGameSound();
            await StartAnimationAsync(StatusInStoryboard);  
            await Task.Delay(3000);
            await StartAnimationAsync(SlideDownStoryboard);
            toastBorder.BorderBrush = Brushes.Red;
        }

        /// <summary>
        /// Start Storyboad animation asynchronously
        /// </summary>
        /// <param name="sb"></param>
        /// <returns></returns>
        private Task StartAnimationAsync(Storyboard sb)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            if (sb == null)
                tcs.SetException(new ArgumentNullException("Storyboard cannot be null"));
            else
            {
                EventHandler handler = null;
                handler = (s, e) =>
                {
                    sb.Completed -= handler;
                    // Turn off the 'Is Live' animation whenever the popup is no longer visible.
                    // Removes small but constant CPU hog
                    if (sb.Name == "SlideDown") toast.IsLive=false;
                    // Console.WriteLine($"{sb.Name} Animation Completed");
                    tcs.SetResult(null);
                };
                sb.Completed += handler;
                sb.Begin();
            }
            return tcs.Task;
        }

        private void PlayNewGameSound()
        {
            PlaySound(Directory.GetCurrentDirectory() + @"\sounds\Balloon_Popping_SoundBible.com_1247261379.wav");
        }

        private void PlayOnlineSound()
        {
           PlaySound(Directory.GetCurrentDirectory() + @"\sounds\Door_Bell-SoundBible.wav");
        }

        private void PlayOfflineSound()
        {
            PlaySound(Directory.GetCurrentDirectory() + @"\sounds\165316__ani-music__synthesizer-echo-plinks-2.wav");
        }

        private void PlayFollowedSound()
        {
            PlaySound(Directory.GetCurrentDirectory() + @"\sounds\149187__adriann__harp-strum.wav");
        }

        private void PlayUnfollowedSound()
        {
            PlaySound(Directory.GetCurrentDirectory() + @"\sounds\lightclunk2.wav");
        }

        private void PlaySound(string path)
        {
            if (miNITurnSoundOff.Checked) return;
            using (SoundPlayer player = new SoundPlayer(path))
            {
                player.Play();
            }
        }


        /// <summary>
        /// Returns bool indicating if the text in the multiline TextBlock has been trimmed
        /// </summary>
        /// <param name="textBlock"></param>
        /// <returns>bool</returns>
        public bool CalculateIsTextTrimmedMultiline(TextBlock textBlock)
        {
            Typeface typeface = new Typeface(
                textBlock.FontFamily,
                textBlock.FontStyle,
                textBlock.FontWeight,
                textBlock.FontStretch);

            // FormattedText is used to measure the whole width of the text held up by TextBlock container
            FormattedText formattedText = new FormattedText(
                textBlock.Text,
                System.Threading.Thread.CurrentThread.CurrentCulture,
                textBlock.FlowDirection,
                typeface,
                textBlock.FontSize,
                textBlock.Foreground);

            formattedText.MaxTextWidth = textBlock.ActualWidth;

            // When the maximum text width of the FormattedText instance is set to the actual
            // width of the textBlock, if the textBlock is being trimmed to fit then the FormattedText           
            // will report a larger height than the textBlock. Should work whether the
            // textBlock is single or multi-line.
            return (formattedText.Height > textBlock.ActualHeight);
        }

        public static bool CalculateIsTextTrimmed(TextBlock textBlock)
        {
            Typeface typeface = new Typeface(
                textBlock.FontFamily,
                textBlock.FontStyle,
                textBlock.FontWeight,
                textBlock.FontStretch);

            // FormattedText is used to measure the whole width of the text held up by TextBlock container
            FormattedText formattedText = new FormattedText(
                textBlock.Text,
                System.Threading.Thread.CurrentThread.CurrentCulture,
                textBlock.FlowDirection,
                typeface,
                textBlock.FontSize,
                textBlock.Foreground);

            // When the maximum text width of the FormattedText instance is set to the actual
            // width of the textBlock, if the textBlock is being trimmed to fit then the formatted
            // text will report a larger height than the textBlock. Should work whether the
            // textBlock is single or multi-line.
            return formattedText.Width > textBlock.ActualWidth;
        }

        /// <summary>
        /// Set the toast.LeftPosition baased on the current screen width
        /// </summary>
        private void CentreToast()
        {
            toast.LeftPosition = this.Left = (SystemParameters.WorkArea.Width / 2) - (this.Width / 2);
        }
    }
}
  