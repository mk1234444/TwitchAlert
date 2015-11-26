using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TwitchAlert.classes
{
    public static class MKTwitch
    {
        /// <summary>
        /// Clunky synchronisation mechanism. Meke network calls async instead
        /// </summary>
        public static bool ShowingOnlineUsers = false;

        static DispatcherTimer timer;
        public static string UserName { get; set; }


        public class MKTwitchEventArgs:EventArgs
        {
            public User User { get; set; }
        }

        public static event EventHandler<MKTwitchEventArgs> Online;
        public static event EventHandler<MKTwitchEventArgs> OffLine;
        private static void OnOnline (User user)
        {
            EventHandler<MKTwitchEventArgs> handler = Online;
            if (handler != null)
            {
                handler.Invoke(null, new MKTwitchEventArgs {User = user});
            }
        }

        private static void OnOffline(User user)
        {
            EventHandler<MKTwitchEventArgs> handler = OffLine;
            if (handler != null)
            {
                handler.Invoke(null, new MKTwitchEventArgs { User = user});
            }
        }


        public static List<User> followedUsers = new List<User>();
        const string twitchUrl = "https://api.twitch.tv/kraken/";

        public static void  Start(string userName)
        {
            UserName = userName;
            SetupStreamTracker( userName);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(20);
            timer.Tick+=(s,e) => 
            {
                if (followedUsers.Count == 0) return;
                foreach (var u in followedUsers)
                {
                    string game;
                    int numViewers;
                    string createdAt;
                    var isLive = IsUserLive(u.Name,out numViewers,out game,out createdAt);

                    // Update createdAt, game and numViewers which sometimes lag
                    if (isLive)
                    {
                        var arr = createdAt.Split('T');                         // Get the created time (without the traling Z) 
                        u.StreamCreatedAt = arr[1].Remove(arr[1].Length - 1);   // and update our version
                        u.NumViewers = numViewers;
                        u.Game = game;
                    }
                    if (ShowingOnlineUsers) continue;
                    // isLive state is unchanged from last time we checked so do nothing
                    if (isLive == u.IsStreaming) continue;

                    Console.WriteLine($"\n{u.Name} is {(isLive ? "Live " : "Not Live")}{(isLive ? "with " + u.Game : "")}");

                    // IsLive status has changed so update our copy
                    u.IsStreaming = isLive;            

                    // Trigger either Online or the Offline event to alert any subscribers of the change
                    if (isLive)
                        OnOnline(u);
                    else
                        OnOffline(u);
                }
                Console.Write(".");
            };
            timer.Start();
        }


        private async static void SetupStreamTracker( string userName)
        {
            await Task.Delay(500);
            var users = GetUsersFollowedChannels(userName);
            if (users == null) return;
            foreach (var f in users.follows)
            {
                int viewers;
                string game = "";
                string createdAt = "";
                var isUserLive = IsUserLive(f.channel.display_name, out viewers, out game,out createdAt);

                // Remove the date and the trailing Z from the createdAt string
                if (createdAt != "")
                {
                    var arr = createdAt.Split('T');
                    createdAt = arr[1].Remove(arr[1].Length - 1);
                }

                // If the Thumbnail (logo) Image has already been cached to file then get it from there.
                // Else download it.
                var cachedFilename = ImageSaver.GetCachedImageFilename(f.channel.logo);
                var bm = (!string.IsNullOrEmpty(cachedFilename) && File.Exists(cachedFilename)) ? new BitmapImage(new Uri(cachedFilename)) : DownloadImage(f.channel.logo);

                followedUsers.Add(new User { Name = f.channel.display_name, IsStreaming = isUserLive, NumViewers = viewers, Game = f.channel.game,StreamCreatedAt = createdAt, ThumbnailPath = f.channel.logo, Thumbnail = bm ,Link = f.channel.url});
                if (isUserLive) Console.WriteLine($"\n{f.channel.display_name} is {(isUserLive ? "Live " : "Not Live")}{(isUserLive ? "with " + f.channel.game : "")}");
            }

            foreach(var user in followedUsers.Where(i=>i.IsStreaming))
            {
                OnOnline(user);
                await Task.Delay(6000);
            }
        }


        public static void TriggerOnline(User user)
        {
            OnOnline(user);
        }
        public static void TriggerOffline(User user)
        {
            OnOnline(user);
        }

        static Twitch.Root GetFollowers(string user, int limit = 25, string sortDirection = "DESC")
        {
            string url = $"{twitchUrl}channels/{user}/follows?direction={sortDirection}&limit={limit}&offset=0";
            return JsonConvert.DeserializeObject<Twitch.Root>(Get(url));
        }

        static Twitch.Root GetUsersFollowedChannels(string user, int limit = 25, string sortDirection = "DESC")
        {
            //GET https://api.twitch.tv/kraken/users/test_user1/follows/channels
            string url = $"{twitchUrl}users/{user}/follows/channels?direction={sortDirection}&limit={limit}&offset=0";
            return JsonConvert.DeserializeObject<Twitch.Root>(Get(url));
        }

        static Twitch.Root GetStreams(string user, int limit = 25, string sortDirection = "DESC")
        {
            //"https://api.twitch.tv/kraken/streams/" + sUsername;
            string url = $"{twitchUrl}streams/{user}";
            return JsonConvert.DeserializeObject<Twitch.Root>(Get(url));
        }

        static Twitch.User GetUser(string user)
        {
            //GET https://api.twitch.tv/kraken/user
            string url = $"{twitchUrl}users/{user}";
            return JsonConvert.DeserializeObject<Twitch.User>(Get(url));
        }

        /// <summary>
        /// Returns true if User userName is currently streaming
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>bool</returns>
        static bool IsUserLive(string userName)
        {
            string url = "https://api.twitch.tv/kraken/streams/" + userName;
            var jsonString = Get(url);
            if (jsonString == null) return false;

            var user = JsonConvert.DeserializeObject<TwitchStream.Root>(jsonString);
            return user.stream != null;
        }

        /// <summary>
        /// Returns true if User userName is currently streaming. Also returns the
        /// Number of Viewers and the Game in the out variables numViewers and game
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="numViewers"></param>
        /// <param name="game"></param>
        /// <returns>bool, out numViewers, out game</returns>
        static bool IsUserLive(string userName, out int numViewers, out string game)
        {
            //string url = "https://api.twitch.tv/kraken/streams/" + userName;
            numViewers = 0;
            game = "";
            string createdAt = "";
            return IsUserLive(userName, out numViewers, out game, out createdAt);
        }

        static bool IsUserLive(string userName, out int numViewers, out string game, out string createdAt)
        {
            createdAt = "";
            numViewers = 0;
            game = "";
            string url = "https://api.twitch.tv/kraken/streams/" + userName;
            var user = JsonConvert.DeserializeObject<TwitchStream.Root>(Get(url));
            if (user?.stream == null) return false;

            createdAt = user.stream.created_at;
            numViewers = user.stream.viewers;
            game = user.stream.game;
            return true;
        }

        /// <summary>
        /// Gets JSON response as a string
        /// </summary>
        /// <param name="fullyFormedUrl"></param>
        /// <returns>string</returns>
        static string Get(string fullyFormedUrl)
        {
            HttpWebRequest wRequest = (HttpWebRequest)WebRequest.Create(fullyFormedUrl);
            wRequest.ContentType = "application/json";
            wRequest.Accept = "application/vnd.twitchtv.v3+json";
            wRequest.Method = "GET";
            string res = "";

            try
            {
                using (var wResponse = wRequest.GetResponse().GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(wResponse))
                    {
                        res = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MKTwitch.Get() Failed with {ex.Message}.");
            }
            return res;
        }


        // Declare delegate.
        delegate void RemoveHandlersDelegate(BitmapImage bm, EventHandler handlerCompleted, EventHandler<ExceptionEventArgs> handlerDownloadFailed, EventHandler<ExceptionEventArgs> handlerDecodeFailed);
        public static BitmapImage DownloadImage(string imageUrl)
        {
            if (imageUrl == null) throw new ArgumentNullException("imageUrl cannot be null");
            //  Console.WriteLine("DownloadImage() called with url {0}",imageUrl);
            EventHandler handlerCompleted = null;
            EventHandler<ExceptionEventArgs> handlerDownloadFailed = null;
            EventHandler<ExceptionEventArgs> handlerDecodeFailed = null;

            // Instantiate the delegate type using an anonymous lambda.
            RemoveHandlersDelegate rhd = (BitmapImage bm, EventHandler Completed, EventHandler<ExceptionEventArgs> DownloadFailed, EventHandler<ExceptionEventArgs> DecodeFailed) =>
            {
                bm.DownloadCompleted -= Completed;
                bm.DownloadFailed -= DownloadFailed;
                bm.DecodeFailed -= DecodeFailed;
            };

            handlerDecodeFailed = (s2, e1) =>
            {
                var ourImg = s2 as BitmapImage;
                Console.WriteLine("DownloadImage() Failed to decode image {0}", ourImg.UriSource);
                if (ourImg != null)
                {
                    rhd(ourImg, handlerCompleted, handlerDownloadFailed, handlerDecodeFailed);
                }
            };

            handlerDownloadFailed = (s3, e2) =>
            {
                var ourImg = s3 as BitmapImage;
                Console.WriteLine("DownloadImage() Failed to download image {0}", ourImg.UriSource);

                if (ourImg != null)
                {
                    rhd(ourImg, handlerCompleted, handlerDownloadFailed, handlerDecodeFailed);
                }
            };

            // Freeze the image in its DownloadCompleted event handler
            handlerCompleted = (s1, e) =>
            {
                var ourImg = s1 as BitmapImage;
                if (ourImg != null)
                {
                    Console.WriteLine("DownloadImage() Completed {0}", ourImg.UriSource);
                    rhd(ourImg, handlerCompleted, handlerDownloadFailed, handlerDecodeFailed);
                    if (ourImg.CanFreeze) ourImg.Freeze();
                    ImageCacher.CacheIfNotCached(ourImg, ourImg.UriSource.OriginalString);
                }
            };

            var img = new BitmapImage();

            img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.DecodePixelHeight = 74;
                img.CreateOptions |= BitmapCreateOptions.IgnoreColorProfile;
                img.UriSource = new Uri(imageUrl);
                img.DownloadCompleted += handlerCompleted;
                img.DownloadFailed += handlerDownloadFailed;
                img.DecodeFailed += handlerDecodeFailed;
            img.EndInit();
            return img;
        }
    }
}
