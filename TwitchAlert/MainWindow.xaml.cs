﻿// 0.4
// Change the layout to include Username and Status information

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

            MKTwitch.Updating += (s, e) => {
                txtUpdating.Visibility = e.IsUpdating ? Visibility.Visible : Visibility.Collapsed;
            };

            while (string.IsNullOrEmpty(USER_NAME))
                GetUserName();

            MKTwitch.Start(USER_NAME);
        }

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

        #region Windows Events
        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Top = toast.BottomPosition = SystemParameters.WorkArea.Height;
            //Console.WriteLine($"Loaded - this.Top {this.Top}");
            //------------------
           // toast.TopPosition = SystemParameters.WorkArea.Height - (toastBorder.ActualHeight);
            //------------------

            // If this is the first time we've been run then calculate the Left position, else
            // use the Left position that we saved the last time we ran
            if (Properties.Settings.Default.settingsLeft == 0.1)
                toast.LeftPosition = this.Left = SystemParameters.WorkArea.Width - txtNobodyOnline.Width;
            else
                toast.LeftPosition = this.Left = Properties.Settings.Default.settingsLeft;
        }

        private void window_ContentRendered(object sender, EventArgs e)
        {
            this.Top = toast.BottomPosition = SystemParameters.WorkArea.Height;
            //Console.WriteLine($"ContentRendered - this.Top {this.Top}");
            //Console.WriteLine($"ContentRendered - TopPosition = {toast.TopPosition}");
        }

        private void window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.settingsLeft = this.Left;
            if(!string.IsNullOrEmpty(USER_NAME))
                Properties.Settings.Default.settingsUserName = USER_NAME;
            Properties.Settings.Default.Save();
            DisposeNotifyIcon();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        #endregion

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
                toast.TopPosition = SystemParameters.WorkArea.Height - (toastBorder.Height);
                Console.WriteLine($"TopPosition = {toast.TopPosition}");
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
                    Console.WriteLine("Animation Completed");
                    tcs.SetResult(null);
                };
                sb.Completed += handler;
                sb.Begin();
            }
            return tcs.Task;
        }

        private void PlayOnlineSound()
        {
            using (SoundPlayer player = new SoundPlayer(Directory.GetCurrentDirectory() + @"\sounds\Door_Bell-SoundBible.wav"))
            {
                player.Play();
            }
        }

        private void PlayOfflineSound()
        {
            using (SoundPlayer player = new SoundPlayer(Directory.GetCurrentDirectory() + @"\sounds\165316__ani-music__synthesizer-echo-plinks-2.wav"))
            {
                player.Play();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

        private void toastBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //var top = this.Top;
            Console.WriteLine($"toastBorder.ActualHeight = {(sender as Border).ActualHeight}\ntoastBorder.Height = {(sender as Border).Height}");
            toast.TopPosition = SystemParameters.WorkArea.Height - ((sender as Border).Height +15);
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
            // width of the textBlock, if the textBlock is being trimmed to fit then the formatted
            // text will report a larger height than the textBlock. Should work whether the
            // textBlock is single or multi-line.
            return (formattedText.Height > textBlock.ActualHeight);
        }

        private void txtStatus_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            var trimmed = CalculateIsTextTrimmedMultiline(sender as TextBlock);
            if(trimmed)
            {
                var tt = Resources["StatusTooltip"] as ToolTip;
                // Find the TextBlock from within the Tooltip Template that will display our Status text
                var ttTextBlock = ((tt.Content as Border).Child as StackPanel).Children[1] as TextBlock;
                // Then give it the full Status text to display
                ttTextBlock.Text = ((sender as TextBlock).DataContext as Toast).Status;
            }
            // Set Handled if the text wasnt trimmed so no empty Tooltip appears
            e.Handled = !trimmed;
        }
    }
}
