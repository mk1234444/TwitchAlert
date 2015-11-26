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
using System.Windows.Threading;

namespace TwitchAlert
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public class Toast:DependencyObject
        {
            public double TopPosition
            {
                get { return (double)GetValue(TopPositionProperty); }
                set { SetValue(TopPositionProperty, value); Console.WriteLine(value); }
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
        const string MK1234444 = "mk1234444";
        string USER_NAME = MK1234444;
        Storyboard FadeInStoryboard;
        Storyboard FadeOutStoryboard;
        Storyboard SlideUpStoryboard;
        Storyboard SlideDownStoryboard;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = toast;
            SetupNotificationIcon();

            SlideUpStoryboard =  FindResource("SlideUp") as Storyboard;
            SlideDownStoryboard = FindResource("SlideDown") as Storyboard;


            DispatcherTimer debugTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1)};
            debugTimer.Tick += (s, e) => {
                txtTopPostion.Text = $"TopPosition {toast.TopPosition.ToString()} this.Top {this.Top}";
               // Console.WriteLine(toast.TopPosition);
            };
            debugTimer.Start();








      
  

            // Subscribe to the MKTwitch Online event so we hear about any user
            // that starts streaming live
            MKTwitch.Online += (s, e)=>{
                toast.DisplayName = e.User.Name;
                toast.Game = e.User.Game;
                toast.Viewers = e.User.NumViewers;
                toast.StreamCreatedAt = e.User.StreamCreatedAt;
                toast.IsLive = true;
                toast.Thumbnail = e.User.Thumbnail;
                toast.Link = e.User.Link;
                this.Top = SystemParameters.WorkArea.Height;
                //----------------
                //if (toast.TopPosition == 0)
                //    toast.TopPosition = SystemParameters.WorkArea.Height - (txtNobodyOnline.Height);
                //-----------------
                switch(e.User.Name)
                {
                    #region Hardcoded Thumbnails
                    //case "OpheliaNoir":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/ophelianoir-profile_image-3e6a95b7f0ff98da-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "EpicGamingParty":
                    //    toast.Thumbnail = new BitmapImage( new Uri("pack://application:,,,/Images/epicgamingparty-profile_image-d438e8a824c62a1d-300x300.png", UriKind.Absolute));
                    //    break;
                    //case "Trisarahtops_":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/trisarahtops_-profile_image-39038cf775e9afe5-300x300.png", UriKind.Absolute));
                    //    break;
                    //case "Trystan34":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/trystan34-profile_image-f4af5b396d44fa7f-300x300.png", UriKind.Absolute));
                    //    break;
                    //case "Thuhmp":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/thuhmp-profile_image-a083cfbe25d22c28-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "zhivcheg":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/zhivcheg-profile_image-7c2966f36cc208e9-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "WhatMinjaPlays":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/whatminjaplays-profile_image-73edf252ce515cd5-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "Yaybutts":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/yaybutts-profile_image-cfb96080e30765c7-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "MersiCR":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/mersicr-profile_image-d3663991e520db77-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "Melikka":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/melikka-profile_image-891ac6aca8ddb2e7-300x300.png", UriKind.Absolute));
                    //    break;
                    //case "Southern__Belle":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/southern__belle-profile_image-df4be96d9a6b4d2f-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "KristaKitsune":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/kristakitsune-profile_image-1d1837f76ca1249a-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "KarmicSlingshot":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/karmicslingshot-profile_image-952f0f5f74e6d321-300x300.png", UriKind.Absolute));
                    //    break;
                    //case "Flamers1500":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/flamers1500-profile_image-79f84b4f1901c4b3-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "Zerfyx":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/zerfyx-profile_image-e96e4dabe0401d07-300x300.jpeg", UriKind.Absolute));
                    //    break; 
                    #endregion
                }
                // Console.WriteLine($"Online Event {toast.DisplayName} - {toast.IsLive} Playing {toast.Game}");

                PlayOnlineSound();
                DisplayToast();
      

                // Display the Online followedUser count in the NotifyIcon Tooltip
                notifyIcon.Text = $"TwitchAlert ({MKTwitch.followedUsers.Count(i => i.IsStreaming)} Online)";
            };

            // Subscribe to the MKTwitch Offline event so we hear about
            // any streamer who stops streaming
            MKTwitch.OffLine += (s, e) => {
                toast.DisplayName = e.User.Name;
                toast.Game = e.User.Game;
                toast.Viewers = e.User.NumViewers;
                toast.StreamCreatedAt = e.User.StreamCreatedAt;
                toast.IsLive = false;
                toast.Thumbnail = e.User.Thumbnail;
                toast.Link = e.User.Link;
                switch (e.User.Name)
                {
                    #region Hardcoded Thumbnails
                    //case "OpheliaNoir":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/ophelianoir-profile_image-3e6a95b7f0ff98da-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "EpicGamingParty":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/epicgamingparty-profile_image-d438e8a824c62a1d-300x300.png", UriKind.Absolute));
                    //    break;
                    //case "Trisarahtops_":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/trisarahtops_-profile_image-39038cf775e9afe5-300x300.png", UriKind.Absolute));
                    //    break;
                    //case "Trystan34":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/trystan34-profile_image-f4af5b396d44fa7f-300x300.png",UriKind.Absolute));
                    //    break;
                    //case "Thuhmp":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/thuhmp-profile_image-a083cfbe25d22c28-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "zhivcheg":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/zhivcheg-profile_image-7c2966f36cc208e9-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "WhatMinjaPlays":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/whatminjaplays-profile_image-73edf252ce515cd5-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "Yaybutts":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/yaybutts-profile_image-cfb96080e30765c7-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "MersiCR":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/mersicr-profile_image-d3663991e520db77-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "Melikka":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/melikka-profile_image-891ac6aca8ddb2e7-300x300.png", UriKind.Absolute));
                    //    break;
                    //case "Southern__Belle":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/southern__belle-profile_image-df4be96d9a6b4d2f-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "KristaKitsune":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/kristakitsune-profile_image-1d1837f76ca1249a-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "KarmicSlingshot":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/karmicslingshot-profile_image-952f0f5f74e6d321-300x300.png", UriKind.Absolute));
                    //    break;
                    //case "Flamers1500":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/flamers1500-profile_image-79f84b4f1901c4b3-300x300.jpeg", UriKind.Absolute));
                    //    break;
                    //case "Zerfyx":
                    //    toast.Thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/zerfyx-profile_image-e96e4dabe0401d07-300x300.jpeg", UriKind.Absolute));
                    //    break; 
                    #endregion
                }
                // Console.WriteLine($"Online Event {toast.DisplayName} - {toast.IsLive} Playing {toast.Game}");

                // Display the Online followedUser count in the NotifyIcon Tooltip
                notifyIcon.Text = $"TwitchAlert ({MKTwitch.followedUsers.Count(i=>i.IsStreaming)} Online)";

                PlayOfflineSound();
                DisplayToast();
            };
            MKTwitch.Start(USER_NAME);
        }


        #region Windows Events
        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Top = toast.BottomPosition = SystemParameters.WorkArea.Height;
            Console.WriteLine($"Loaded - this.Top {this.Top}");
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
            //this.Top = SystemParameters.WorkArea.Height;
            Console.WriteLine($"ContentRendered - this.Top {this.Top}");

            //toast.TopPosition = SystemParameters.WorkArea.Height - txtNobodyOnline.Height;
            //toast.LeftPosition = this.Left = SystemParameters.WorkArea.Width - txtNobodyOnline.Width;
            Console.WriteLine($"ContentRendered - TopPosition = {toast.TopPosition}");
  
            //            this.Left = width - this.ActualWidth;
        }

        private void window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.settingsLeft = this.Left;
            Properties.Settings.Default.Save();
            DisposeNotifyIcon();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        #endregion

        public async Task DisplayToast(bool AllOffline = false)
        {
            this.Topmost = true;
            if (AllOffline)
            {
                txtNobodyOnline.Visibility = Visibility.Visible;
                toast.TopPosition = SystemParameters.WorkArea.Height - (txtNobodyOnline.Height);
                Console.WriteLine($"TopPosition = {toast.TopPosition}");
                this.Top = SystemParameters.WorkArea.Height;// - txtNobodyOnline.Height;
              //  this.Left = SystemParameters.WorkArea.Width - txtNobodyOnline.Width;
            }

           // toast.TopPosition = SystemParameters.WorkArea.Height - toastBorder.ActualHeight;
            await StartAnimationAsync(SlideUpStoryboard);
            await Task.Delay(4000);
            await StartAnimationAsync(SlideDownStoryboard);
            txtNobodyOnline.Visibility = Visibility.Collapsed;
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
            var top = this.Top;
            Console.WriteLine($"toastBorder.ActualHeight = {(sender as Border).ActualHeight}\ntoastBorder.Height = {(sender as Border).Height}");
            toast.TopPosition = SystemParameters.WorkArea.Height - (sender as Border).ActualHeight;
        }
    }
}
