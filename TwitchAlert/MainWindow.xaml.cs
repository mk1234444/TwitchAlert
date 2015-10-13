// 0.4.4
// TEST:  If there were 2 or more streamers who started/stopped streaming within any given timer tick then there would be no delay between their popups.
// TODO:  Also there's no delay between Online and Offline popups if thay happen on the same tick
// TODO:  Add code to detect if user has followed a new streamer (at the moment closing the reopening fixes this)
// TODO:  Add options in the ContextMenu to recieve popups whenever a streamer changes their Game and/or Status message
// DONE:  Add option in the ContextMenu to skip popups when the app firsts starts. If a user follows 100s of
//        streamers then there is the potential for dozens of those streamers to already be streaming at startup triggering
//        popups for them all.
// FIXED: Removed the 25 limit on the get followed users call
// DONE:  Added 'Retrieving information...' to the NotifyIcon Tooltip during first pull
// DONE:  Pressing Esc during a popup cycle will stop that cycle (This does not turn off future popups)
// TODO:  Disables the Change Username MenuItem at startup until MKTwitch.Start() has completed

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

namespace TwitchAlert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class Toast:DependencyObject
        {
           public Toast()
            {
                Link = "";
            }
            public double TopPosition
            {
                get { return (double)GetValue(TopPositionProperty); }
                set { SetValue(TopPositionProperty, value); }
            }
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
                set { SetValue(StreamCreatedAtProperty, $"Started: {value}   ({Viewers} Viewers)"); }
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
            // Using a DependencyProperty as the backing store for TopPosition.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty BottomPositionProperty = DependencyProperty.Register("BottomPosition", typeof(double), typeof(Toast), new PropertyMetadata(0.0));
            // Using a DependencyProperty as the backing store for LeftPosition.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty LeftPositionProperty = DependencyProperty.Register("LeftPosition", typeof(double), typeof(Toast), new PropertyMetadata(0.0));
            // Using a DependencyProperty as the backing store for StreamCreatedAt.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty StreamCreatedAtProperty = DependencyProperty.Register("StreamCreatedAt", typeof(string), typeof(Toast), new PropertyMetadata(null));
            // Using a DependencyProperty as the backing store for ThumbNail.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty ThumbnailProperty = DependencyProperty.Register("Thumbnail", typeof(BitmapImage), typeof(Toast), new PropertyMetadata(null));
            // Using a DependencyProperty as the backing store for Viewers.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty ViewersProperty = DependencyProperty.Register("Viewers", typeof(int), typeof(Toast), new PropertyMetadata(null));
            // Using a DependencyProperty as the backing store for IsLive.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty IsLiveProperty = DependencyProperty.Register("IsLive", typeof(bool), typeof(Toast), new PropertyMetadata(false));
            // Using a DependencyProperty as the backing store for Game.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty GameProperty = DependencyProperty.Register("Game", typeof(string), typeof(Toast), new PropertyMetadata(null));
            // Using a DependencyProperty as the backing store for DisplayName.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register("DisplayName", typeof(string), typeof(Toast), new PropertyMetadata(null));  
        }

        Toast toast = new Toast();
        string USER_NAME = Properties.Settings.Default.settingsUserName;
        
        Storyboard SlideUpStoryboard;
        Storyboard SlideDownStoryboard;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = toast;
            SetupNotificationIcon();

            SlideUpStoryboard = FindResource("SlideUp") as Storyboard;
            SlideDownStoryboard = FindResource("SlideDown") as Storyboard;


            //DispatcherTimer debugTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1)};
            //debugTimer.Tick += (s, e) => {
            //    txtTopPostion.Text = $"TopPosition {toast.TopPosition.ToString()} this.Top {this.Top}";
            //   // Console.WriteLine(toast.TopPosition);
            //};
            //debugTimer.Start();


            // Subscribe to the MKTwitch Online event so we hear about any user
            // that starts streaming live
            MKTwitch.Online += (s, e)=>{
                FillInToast(e.User);       
                PlayOnlineSound();
                DisplayToast();
                // Display the Online followedUser count in the NotifyIcon Tooltip
                notifyIcon.Text = $"TwitchAlert ({USER_NAME})\nFollowing {MKTwitch.followedUsers.Count} ({MKTwitch.followedUsers.Count(i => i.IsStreaming)} Online)";
            };

            // Subscribe to the MKTwitch Offline event so we hear about
            // any streamer who stops streaming
            MKTwitch.OffLine += (s, e) => {
                FillInToast(e.User);
                // Display the Online followedUser count in the NotifyIcon Tooltip
                notifyIcon.Text = $"TwitchAlert ({USER_NAME})\nFollowing {MKTwitch.followedUsers.Count} ({MKTwitch.followedUsers.Count(i => i.IsStreaming)} Online)";
                PlayOfflineSound();
                DisplayToast();
            };

            // Subscribe to the MKTwitch UpdateStarted event so we hear about
            // when we've started updating our Twitch info
            MKTwitch.UpdateStarted += (s, e) => txtUpdating.Visibility = Visibility.Visible;

            // Subscribe to the MKTwitch UpdateStarted event so we hear about
            // when we've completed updating our Twitch info
            MKTwitch.UpdateCompleted += (s, e) => txtUpdating.Visibility = Visibility.Collapsed;


            // Subscribe to the MKTwitch FollowedUsersChanged event so we hear about
            // when its collection of FollowedUsers has changed
            MKTwitch.FollowedUsersChanged += (s, e) => {
                Console.WriteLine("followedUsers Changed");
                notifyIcon.Text = $"TwitchAlert ({USER_NAME})\nFollowing {MKTwitch.followedUsers.Count} ({MKTwitch.followedUsers.Count(i => i.IsStreaming)} Online)";
            };

            MKTwitch.StartCompleted += (s, e) => { miNIUserName.Enabled = true; };


            // If we dont have a username then keep asking till we get one
            while (string.IsNullOrEmpty(USER_NAME))
            {
                GetUserName();
                //notifyIcon.Text = $"TwitchAlert ({USER_NAME})\nFollowing {MKTwitch.followedUsers.Count} ({MKTwitch.followedUsers.Count(i => i.IsStreaming)} Online)";
            }
            notifyIcon.Text = $"TwitchAlert ({USER_NAME})\nRetrieving information...";
            MKTwitch.Start(USER_NAME, Properties.Settings.Default.settingsSkipPopups);
        }

        #region Windows Events
        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Top = toast.BottomPosition = SystemParameters.WorkArea.Height;
            //Console.WriteLine($"Loaded - this.Top {this.Top}");
            //------------------
            // toast.TopPosition = SystemParameters.WorkArea.Height - (toastBorder.ActualHeight);
            //------------------
            //Console.WriteLine("Window_Loaded event:");
            //Console.WriteLine($"\tsettings.Left = {Properties.Settings.Default.settingsLeft}");
            //Console.WriteLine($"\tsettings.UserName = {Properties.Settings.Default.settingsUserName}");

            // If this is the first time we've been run then calculate the Left position, else
            // use the Left position that we saved the last time we ran
            if (Properties.Settings.Default.settingsLeft == 0.1)
                toast.LeftPosition = this.Left = SystemParameters.WorkArea.Width - this.Width;
            // If Left was saved previously off the right hand side of the screen then reset it so that the full popup will be displayed at the right
            else if (Properties.Settings.Default.settingsLeft > (SystemParameters.WorkArea.Width - this.Width))
                toast.LeftPosition = this.Left = SystemParameters.WorkArea.Width - this.Width;
            // If the Left was saved previously off the left side of the screen then reset is so the full popup will be displayed at the left
            else if (Properties.Settings.Default.settingsLeft < 0)
                toast.LeftPosition = this.Left = -15;       // The 15 takes into account the offset of the rootBorder to the main window
            else
                toast.LeftPosition = this.Left = Properties.Settings.Default.settingsLeft;

            miNITurnSoundOff.Checked = Properties.Settings.Default.settingsSoundOff;
            miNISkipPopupsAtStart.Checked = Properties.Settings.Default.settingsSkipPopups;
        }

        private void window_ContentRendered(object sender, EventArgs e)
        {
            this.Top = toast.BottomPosition = SystemParameters.WorkArea.Height;
            //Console.WriteLine($"ContentRendered - this.Top {this.Top}");
            //Console.WriteLine($"ContentRendered - TopPosition = {toast.TopPosition}");
        }

        private void window_Closing(object sender, CancelEventArgs e)
        {
            //Console.WriteLine($"Window_Closing");
            //Console.WriteLine($"\tthis.Left {this.Left} this.Top {this.Top} toast.TopPosition {toast.TopPosition} toastLeftPosition {toast.LeftPosition}");
            //Console.WriteLine($"\tUSER_NAME {USER_NAME}");

            if (userNameWindow != null) userNameWindow.Close();
            Properties.Settings.Default.settingsLeft = this.Left;
            if(!string.IsNullOrEmpty(USER_NAME))
                Properties.Settings.Default.settingsUserName = USER_NAME;
            Properties.Settings.Default.settingsSoundOff = miNITurnSoundOff.Checked;
            Properties.Settings.Default.settingsSkipPopups = miNISkipPopupsAtStart.Checked;

            Properties.Settings.Default.Save();

            //Console.WriteLine("Property settings after save");
            //Console.WriteLine($"\tProperties.Settings.Default.settingsLeft {Properties.Settings.Default.settingsLeft}");
            //Console.WriteLine($"\tProperties.Settings.Default.settingsUserName {Properties.Settings.Default.settingsUserName}");
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
        }
        #endregion

        #region Event Handlers
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
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
        }

        /// <summary>
        /// Displays the popup showing info of the Streamer. If AllOffline==true then 
        /// "Nobody Online" message is displayed in the popup instead
        /// </summary>
        /// <param name="AllOffline"></param>
        /// <returns></returns>
        public async Task DisplayToast(bool AllOffline = false)
        {
            this.Topmost = true;
            if (AllOffline)
            {
                txtNobodyOnline.Visibility = Visibility.Visible;
                brdIsOnline.Visibility = Visibility.Collapsed;
                // toast.TopPosition = SystemParameters.WorkArea.Height - (toastBorder.Height);
                toast.TopPosition = SystemParameters.WorkArea.Height - (this.Height);

                //Console.WriteLine($"TopPosition = {toast.TopPosition}");
                this.Top = SystemParameters.WorkArea.Height;
            }
            await StartAnimationAsync(SlideUpStoryboard);
            await Task.Delay(4000);
            await StartAnimationAsync(SlideDownStoryboard);

            txtNobodyOnline.Visibility = Visibility.Collapsed;
            brdIsOnline.Visibility = Visibility.Visible;
        }

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
                  //  Console.WriteLine($"{sb.Name} Animation Completed");
                    tcs.SetResult(null);
                };
                sb.Completed += handler;
                sb.Begin();
            }
            return tcs.Task;
        }

        private void PlayOnlineSound()
        {
            if (miNITurnSoundOff.Checked) return;
            using (SoundPlayer player = new SoundPlayer(Directory.GetCurrentDirectory() + @"\sounds\Door_Bell-SoundBible.wav"))
            {
                player.Play();
            }
        }

        private void PlayOfflineSound()
        {
            if (miNITurnSoundOff.Checked) return;
            using (SoundPlayer player = new SoundPlayer(Directory.GetCurrentDirectory() + @"\sounds\165316__ani-music__synthesizer-echo-plinks-2.wav"))
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

      
    }
}
