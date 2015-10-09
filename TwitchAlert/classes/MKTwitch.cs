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
using System.Windows;

namespace TwitchAlert.classes
{
    public static class MKTwitch
    {
        /// <summary>
        /// Indicates that the Start() Method has been run. Start() has to
        /// be run once tosetup the Timer and stuff
        /// </summary>
        public static bool IsStarted;
        private static bool IsUpdating;

        static DispatcherTimer timer;
        public static string UserName { get; set; }

        #region Custom EventArgs
        public class MKTwitchUpdatingEventArgs : EventArgs
        {
            public bool IsUpdating;
        }

        public class MKTwitchEventArgs : EventArgs
        {
            public User User { get; set; }
        }

        public class MKTwitchFollowedUsersEventArgs:EventArgs
        {
            public List<User> FollowedUsers;
        }
        #endregion

        #region Events
        public static event EventHandler<MKTwitchEventArgs> Online;
        public static event EventHandler<MKTwitchEventArgs> OffLine;
        public static event EventHandler<MKTwitchUpdatingEventArgs> Updating;
        public static event EventHandler<MKTwitchFollowedUsersEventArgs> FollowedUsersChanged;
        #endregion

        #region Event Trigger Methods
        private static void OnOnline(User user)
        {
            EventHandler<MKTwitchEventArgs> handler = Online;
            if (handler != null)
            {
                handler.Invoke(null, new MKTwitchEventArgs { User = user });
            }
        }

        private static void OnOffline(User user)
        {
            EventHandler<MKTwitchEventArgs> handler = OffLine;
            if (handler != null)
            {
                handler.Invoke(null, new MKTwitchEventArgs { User = user });
            }
        }

        private static void OnUpdating(bool isUpdating)
        {
            EventHandler<MKTwitchUpdatingEventArgs> handler = Updating;
            if (handler != null)
            {
                handler.Invoke(null, new MKTwitchUpdatingEventArgs { IsUpdating = isUpdating });
            }
        }

        private static void OnFollowedUsersChanged()
        {
            EventHandler<MKTwitchFollowedUsersEventArgs> handler = FollowedUsersChanged;
            if (handler != null)
            {
                handler.Invoke(null, new MKTwitchFollowedUsersEventArgs { FollowedUsers = followedUsers });
            }
        }
        #endregion

        public static List<User> followedUsers = new List<User>();
        const string twitchUrl = "https://api.twitch.tv/kraken/";

        public static async Task ChangeUser(string userName)
        {
            UserName = userName;
            timer.Stop();
            while(IsUpdating)
            {
                await Task.Delay(500);
            }
            await SetupStreamTracker(userName);
            timer.Start();

        }
        public async static void Start(string userName)
        {
            UserName = userName;
            await SetupStreamTracker( userName);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(20);
            timer.Tick+= async(s,e) => 
            {
                if (followedUsers.Count == 0) return;
                OnUpdating(IsUpdating = true);

                await Update();

                Console.Write(".");
                OnUpdating(IsUpdating = false);
                return;









                //var streamers = await GetStreamers();
                //if (streamers == null) return;

                //var nonStreamers = followedUsers.Where(i => !streamers.streams.Any(x => x.channel.display_name == i.Name));

                //// Do the streamers bit first
                //foreach(var streamer in streamers.streams)
                //{
                //    // Get the followed user who is now streaming
                //    var followed = followedUsers.First(i => i.Name == streamer.channel.display_name);
                //    // Update his info
                //    followed.StreamCreatedAt = streamer.created_at.Split('T')[1].Replace("Z", "");
                //    followed.NumViewers = streamer.viewers;
                //    followed.Game = streamer.game;

                //    // if user was already streaming then just continue
                //    if (followed.IsStreaming)
                //    {
                //        followed.OfflineCount = 0;
                //        continue;
                //    }

                //    // user has started streaming so...
                //    // set his isStreaming property to true
                //    followed.IsStreaming = true;
                //    // and throw up a popup
                //    OnOnline(followed);
                //    Console.WriteLine($"\n{ followed.Name} is live with {followed.Game}");
                //    await Task.Delay(6000);
                //}

                //// Do the non-streamers bit
                //foreach(var ns in nonStreamers)
                //{
                //    // If user is going from streaming to offline then throw up a popup
                //    // and set his isStreaming property to false
                //    if(ns.IsStreaming)
                //    {
                //        // Kludge to compensate for the fact that Twitch sometimes says a streamer has gone
                //        // offline when in fact they are still online
                //        ns.OfflineCount++;
                //        if (ns.OfflineCount < 2)
                //            continue;
                //        Console.WriteLine($"{ns.Name}'s offlineCount is {ns.OfflineCount}");
                //        ns.IsStreaming = false;
                //        OnOffline(ns);
                //        Console.WriteLine($"\n{ ns.Name} has gone Offline");
                //        await Task.Delay(6000);
                //    }
                //}

                #region Old
                //foreach (var u in followedUsers)
                //{              
                //    var iuld = await IsUserLiveAsync(u.Name);
                //    var isLive = iuld.IsLive;
                //    string createdAt = iuld.CreatedAt;
                //    // Update createdAt, game and numViewers which sometimes lag
                //    if (isLive)
                //    {
                //        var arr = createdAt.Split('T');                         // Get the created time (without the traling Z) 
                //        u.StreamCreatedAt = arr[1].Remove(arr[1].Length - 1);   // and update our version
                //        u.NumViewers = iuld.NumViewers;
                //        u.Game = iuld.Game;
                //    }

                //    // Kludge to compensate for the fact that Twitch sometimes says a streamer has gone
                //    // offline when in fact they are still online
                //    if(u.IsStreaming && isLive==false)
                //    {
                //        u.OfflineCount++;
                //        if (u.OfflineCount < 2)
                //            continue;
                //        Console.WriteLine($"{u.Name}'s offlineCount is {u.OfflineCount}");
                //    }

                //    //if (ShowingOnlineUsers) continue;
                //    // isLive state is unchanged from last time we checked so do nothing
                //    if (isLive == u.IsStreaming)
                //    {
                //        u.OfflineCount = 0;
                //        continue;
                //    }
                //    u.OfflineCount = 0;
                //    Console.WriteLine($"\n{u.Name} is {(isLive ? "Live " : "Not Live")}{(isLive ? "with " + u.Game : "")}");

                //    // IsLive status has changed so update our copy
                //    u.IsStreaming = isLive;

                //    // Trigger either Online or the Offline event to alert any subscribers of the change
                //    // NOTE: There could be a (rare) problem here if many users start/stop streaming within one
                //    //       tick as all of their popups will not have finished showing by the time the next
                //    //       tick event occurs. A fix would be to turn the timer off at the start of its event
                //    //       and turn it back on again at the end.
                //    if (isLive)
                //    {
                //        OnOnline(u);
                //        await Task.Delay(6000);
                //    }
                //    else
                //    {
                //        OnOffline(u);
                //        await Task.Delay(6000);
                //    }
                //} 
                #endregion

                //Console.Write(".");
                //OnUpdating(IsUpdating = false);
            };
            timer.Start();
            IsStarted = true;
        }


        private static async Task Update()
        {
            var streamers = await GetStreamers();
            if (streamers == null) return;

            var nonStreamers = followedUsers.Where(i => !streamers.streams.Any(x => x.channel.display_name == i.Name));

            // Do the streamers bit first
            foreach (var streamer in streamers.streams)
            {
                // Get the followed user who is now streaming
                var followed = followedUsers.First(i => i.Name == streamer.channel.display_name);
                // Update his info
                followed.StreamCreatedAt = streamer.created_at.Split('T')[1].Replace("Z", "");
                followed.NumViewers = streamer.viewers;
                followed.Game = streamer.game;

                // if user was already streaming then just continue
                if (followed.IsStreaming)
                {
                    followed.OfflineCount = 0;
                    continue;
                }

                // user has started streaming so...
                // set his isStreaming property to true
                followed.IsStreaming = true;
                // and throw up a popup
                OnOnline(followed);
                Console.WriteLine($"\n{ followed.Name} is live with {followed.Game}");
                await Task.Delay(6000);
            }

            // Do the non-streamers bit
            foreach (var ns in nonStreamers)
            {
                // If user is going from streaming to offline then throw up a popup
                // and set his isStreaming property to false
                if (ns.IsStreaming)
                {
                    // Kludge to compensate for the fact that Twitch sometimes says a streamer has gone
                    // offline when in fact they are still online
                    ns.OfflineCount++;
                    if (ns.OfflineCount < 2)
                        continue;
                    Console.WriteLine($"{ns.Name}'s offlineCount is {ns.OfflineCount}");
                    ns.IsStreaming = false;
                    OnOffline(ns);
                    Console.WriteLine($"\n{ ns.Name} has gone Offline");
                    await Task.Delay(6000);
                }
            }
        }



        private static void FollowedUsersToConsole()
        {
            followedUsers.ForEach(u => {
                Console.WriteLine(u.Name);
                Console.WriteLine(u.Status);
                Console.WriteLine(u.Game);
                Console.WriteLine(u.NumViewers);
                Console.WriteLine(u.ThumbnailPath);
                Console.WriteLine(u.IsStreaming);
                Console.WriteLine(u.StreamCreatedAt);
            });
        }


        //private static async Task RefreshFollowedList(string userName)
        //{
        //    var user = GetUsersFollowedChannels(userName);
        //    if (user == null) return;
        //    // Check if we've followed another streamer and if so add them to our followed collection
        //    foreach(var u in user.follows)
        //    {
        //        if(followedUsers.Any(i=>i.Name == u.channel.display_name) == false)
        //            followedUsers.Add(await CreateUserFromTwitchFollow(u));
        //    }
        //    // TODO: Check for unfollows here
        //}

        private static async Task<User> CreateUserFromTwitchFollow(Twitch.Follow follow)
        {
            IsUserLiveData isUserLiveData = await IsUserLiveAsync(follow.channel.display_name);
            bool isUserLive = isUserLiveData.IsLive;

            // Remove the date and the trailing Z from the createdAt string
            if (isUserLiveData.CreatedAt != "")
            {
                var arr = isUserLiveData.CreatedAt.Split('T');
                isUserLiveData.CreatedAt = arr[1].Remove(arr[1].Length - 1);
            }
            BitmapImage bm = null;
            // If user does not have a logo then use the Twitch default
            if (follow.channel.logo == null)
            {
                bm = new BitmapImage(new Uri(@"/TwitchAlert;component/Images/404_user_150x150.png", UriKind.Relative));
            }
            else
            {
                // If the Thumbnail (logo) Image has already been cached to file then get it from there.
                // Else download it.
                var cachedFilename = ImageSaver.GetCachedImageFilename(follow.channel.logo);
                bm = (!string.IsNullOrEmpty(cachedFilename) && File.Exists(cachedFilename)) ? new BitmapImage(new Uri(cachedFilename)) : DownloadImage(follow.channel.logo);
            }

            return new User { Name = follow.channel.display_name, IsStreaming = isUserLive, NumViewers = isUserLiveData.NumViewers, Game = follow.channel.game, StreamCreatedAt = isUserLiveData.CreatedAt, ThumbnailPath = follow.channel.logo, Thumbnail = bm, Link = follow.channel.url, Status = follow.channel.status };
        }


        /// <summary>
        /// Get userNames' followed channels and store them in the followedUsers collection
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private async static Task SetupStreamTracker( string userName)
        {
            var users = GetUsersFollowedChannels(userName);
            if (users == null) return;

            var streamers = await GetStreamers(users);
            if (streamers == null) return;



            followedUsers.Clear();
            foreach (var f in users.follows)
            {
                bool isUserLive = false;
                string createdAt = "";
                int numViewers=0;

                if (streamers.streams.Count > 0)
                {
                    var streamer = streamers.streams.FirstOrDefault(i => i.channel.display_name == f.channel.display_name);
                 
                    if (streamer != null)
                    {
                        isUserLive = true;
                        createdAt = streamer.channel.created_at.Split('T')[1].Replace("Z", "");
                        numViewers = streamer.viewers;
                    }
                }
                //IsUserLiveData isUserLiveData=default(IsUserLiveData);
         
                //isUserLiveData = await IsUserLiveAsync(f.channel.display_name);
                //isUserLive = isUserLiveData.IsLive;
           
                // Remove the date and the trailing Z from the createdAt string
                //if (isUserLiveData.CreatedAt != "")
                //{
                //    var arr = isUserLiveData.CreatedAt.Split('T');
                //    isUserLiveData.CreatedAt = arr[1].Remove(arr[1].Length - 1);
                //}

                BitmapImage bm = null;
                // If user does not have a logo then use the Twitch default
                if (f.channel.logo == null)
                {
                    bm = new BitmapImage(new Uri(@"/TwitchAlert;component/Images/404_user_150x150.png", UriKind.Relative));
                }
                else
                {
                    // If the Thumbnail (logo) Image has already been cached to file then get it from there.
                    // Else download it.
                    var cachedFilename = ImageSaver.GetCachedImageFilename(f.channel.logo);
                    bm = (!string.IsNullOrEmpty(cachedFilename) && File.Exists(cachedFilename)) ? new BitmapImage(new Uri(cachedFilename)) : DownloadImage(f.channel.logo);
                }

                // followedUsers.Add(new User { Name = f.channel.display_name, IsStreaming = isUserLive, NumViewers = isUserLiveData.NumViewers, Game = f.channel.game,StreamCreatedAt = isUserLiveData.CreatedAt, ThumbnailPath = f.channel.logo, Thumbnail = bm ,Link = f.channel.url, Status = f.channel.status});
                 followedUsers.Add(new User { Name = f.channel.display_name, IsStreaming = isUserLive, NumViewers = numViewers, Game = f.channel.game,StreamCreatedAt = createdAt, ThumbnailPath = f.channel.logo, Thumbnail = bm ,Link = f.channel.url, Status = f.channel.status});

                if (isUserLive) Console.WriteLine($"\n{f.channel.display_name} is {(isUserLive ? "Live " : "Not Live")}{(isUserLive ? "with " + f.channel.game : "")}");
            }

            OnFollowedUsersChanged();
            
            foreach(var user in followedUsers.Where(i=>i.IsStreaming))
            {
                OnOnline(user);
                await Task.Delay(6000);
            }
        }


        private async static Task SetupStreamTracker2(string userName)
        {
            // await Task.Delay(500);
            var users = GetUsersFollowedChannels(userName);
            if (users == null) return;

            var streamers = await GetStreamers();

            //OnFollowedUsersChanged();

            followedUsers.Clear();
            foreach (var f in users.follows)
            {
                bool isUserLive = false;
                IsUserLiveData isUserLiveData = default(IsUserLiveData);

                isUserLiveData = await IsUserLiveAsync(f.channel.display_name);
                isUserLive = isUserLiveData.IsLive;

                // Remove the date and the trailing Z from the createdAt string
                if (isUserLiveData.CreatedAt != "")
                {
                    var arr = isUserLiveData.CreatedAt.Split('T');
                    isUserLiveData.CreatedAt = arr[1].Remove(arr[1].Length - 1);
                }

                BitmapImage bm = null;
                // If user does not have a logo then use the Twitch default
                if (f.channel.logo == null)
                {
                    bm = new BitmapImage(new Uri(@"/TwitchAlert;component/Images/404_user_150x150.png", UriKind.Relative));
                }
                else
                {
                    // If the Thumbnail (logo) Image has already been cached to file then get it from there.
                    // Else download it.
                    var cachedFilename = ImageSaver.GetCachedImageFilename(f.channel.logo);
                    bm = (!string.IsNullOrEmpty(cachedFilename) && File.Exists(cachedFilename)) ? new BitmapImage(new Uri(cachedFilename)) : DownloadImage(f.channel.logo);
                }

                followedUsers.Add(new User { Name = f.channel.display_name, IsStreaming = isUserLive, NumViewers = isUserLiveData.NumViewers, Game = f.channel.game, StreamCreatedAt = isUserLiveData.CreatedAt, ThumbnailPath = f.channel.logo, Thumbnail = bm, Link = f.channel.url, Status = f.channel.status });

                if (isUserLive) Console.WriteLine($"\n{f.channel.display_name} is {(isUserLive ? "Live " : "Not Live")}{(isUserLive ? "with " + f.channel.game : "")}");
            }

            OnFollowedUsersChanged();

            foreach (var user in followedUsers.Where(i => i.IsStreaming))
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

        static Twitch.Root GetFollowers(string userName, int limit = 25, string sortDirection = "DESC")
        {
            string url = $"{twitchUrl}channels/{userName}/follows?direction={sortDirection}&limit={limit}&offset=0";
            return JsonConvert.DeserializeObject<Twitch.Root>(Get(url));
        }

        /// <summary>
        /// Returns all the people that userName follows
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="limit"></param>
        /// <param name="sortDirection"></param>
        /// <returns>Twitch.Root</returns>
        static Twitch.Root GetUsersFollowedChannels(string userName, int limit = 25, string sortDirection = "DESC")
        {
            //GET https://api.twitch.tv/kraken/users/test_user1/follows/channels
            string url = $"{twitchUrl}users/{userName}/follows/channels?direction={sortDirection}&limit={limit}&offset=0";

            return JsonConvert.DeserializeObject<Twitch.Root>(Get(url));
        }

        public static bool UserExists(string userName)
        {
            //GET https://api.twitch.tv/kraken/user
            string url = $"{twitchUrl}users/{userName}";
            var res = Get(url);
            return res != "mk404mk";
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
        /// Used by IsUserLiveAsync to pass values back to it callers.
        /// This is needed as async methods can't use out parameters
        /// </summary>
        struct IsUserLiveData
        {
           public bool IsLive;
           public string Game;
           public int NumViewers;
           public string CreatedAt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        static async Task<IsUserLiveData>IsUserLiveAsync(string userName)
        {
            var iuld = new IsUserLiveData();
            string url = "https://api.twitch.tv/kraken/streams/" + userName;
            try
            {
                var user = JsonConvert.DeserializeObject<TwitchStream.Root>(await GetAsync(url));
                //if (userName == "Trisarahtops_")
                //    Console.WriteLine($"{userName} stream is {(user.stream == null ? "null" : "Live")}");
                iuld.IsLive = user?.stream != null;
                iuld.CreatedAt = user?.stream?.created_at == null ? "" : user.stream.created_at;
                iuld.Game = user?.stream?.game == null ? "" : user.stream.game;
                iuld.NumViewers = user?.stream == null ? 0 : user.stream.viewers;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return iuld;
        }
        /// <summary>
        /// Returns all of the people in the followed collection who are currently streaming
        /// </summary>
        /// <returns></returns>
        private static async Task<TwitchStreamers.RootObject> GetStreamers()
        {
            //  https://api.twitch.tv/kraken/streams?channel=chan1,monstercat,chan3

            string url = "https://api.twitch.tv/kraken/streams?channel=";
            string users= followedUsers.Aggregate("", (current, u) => current + (u.Name + ","));
            url += users.Remove(users.Length - 1);

            return JsonConvert.DeserializeObject<TwitchStreamers.RootObject>(await GetAsync(url));
        }

        private static async Task<TwitchStreamers.RootObject> GetStreamers(Twitch.Root users)
        {
            string url = "https://api.twitch.tv/kraken/streams?channel=";
            string userNames = users.follows.Aggregate("", (current, u) => current + u.channel.display_name + ",");
            url += userNames.Remove(userNames.Length - 1);
            return JsonConvert.DeserializeObject<TwitchStreamers.RootObject>(await GetAsync(url));
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
                if (ex.Message.Contains("(404) Not Found")) res = "mk404mk";
                
            }
            return res;
        }

        /// <summary>
        /// Gets JSON response as a string
        /// </summary>
        /// <param name="fullyFormedUrl"></param>
        /// <returns>string</returns>
        static async Task<string> GetAsync(string fullyFormedUrl)
        {
            HttpWebRequest wRequest = (HttpWebRequest)WebRequest.Create(fullyFormedUrl);
            wRequest.ContentType = "application/json";
            wRequest.Accept = "application/vnd.twitchtv.v3+json";
            wRequest.Method = "GET";
            string res = "";

            try
            {
                using (var wResponse = (await wRequest.GetResponseAsync()).GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(wResponse))
                    {
                        res = await reader.ReadToEndAsync();
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
