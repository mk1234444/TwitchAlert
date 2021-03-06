﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.Serialization;
using System.Globalization;
using System.Windows;

namespace TwitchAlert.classes
{
    public static partial class MKTwitch
    {
        internal class MKTwitchTestException : Exception, ISerializable
        {
            public MKTwitchTestException() { }
            public MKTwitchTestException(string message) : base(message) { }
            public MKTwitchTestException(string message, Exception inner) : base(message, inner) { }

            protected MKTwitchTestException(SerializationInfo info, StreamingContext context)
            { }
        }

        internal class MKTwitchUpdateFollowedUsersTestException : Exception, ISerializable
        {
            public MKTwitchUpdateFollowedUsersTestException() { }
            public MKTwitchUpdateFollowedUsersTestException(string message) : base(message) { }
            public MKTwitchUpdateFollowedUsersTestException(string message, Exception inner) : base(message, inner) { }

            protected MKTwitchUpdateFollowedUsersTestException(SerializationInfo info, StreamingContext context)
            { }
        }

        internal class MKTwitchGetAllUsersFollowersTestException : Exception, ISerializable
        {
            public MKTwitchGetAllUsersFollowersTestException() { }
            public MKTwitchGetAllUsersFollowersTestException(string message) : base(message) { }
            public MKTwitchGetAllUsersFollowersTestException(string message, Exception inner) : base(message, inner) { }

            protected MKTwitchGetAllUsersFollowersTestException(SerializationInfo info, StreamingContext context)
            { }
        }

        // private static bool SetupStreamTrackerFailed = false;

        /// <summary>
        /// Used to break the popup cycle loop
        /// </summary>
        public static bool CancelPopupCycle = false;
        static bool skipPopupsAtStart = false;

        /// <summary>
        /// Indicates we are in the process of changing the UserName
        /// </summary>
        public static bool IsChangingUser;


        /// <summary>
        /// Indicates if we are already in the middle of a popup cycle.
        /// Can be used to prevent timed updated from trying to display
        /// a popups during this time.
        /// </summary>
        public static bool IsPopupCycleRunning;


        /// <summary>
        /// Indicates that the Start() Method has been run. Start() has to
        /// be run once to setup the Timer and stuff
        /// </summary>
        public static bool IsStarted;

        /// <summary>
        /// Indicates that stream information is being updated
        /// </summary>
        private static bool IsUpdating;

        static DispatcherTimer timer, kludgeTimer;

        public static DispatcherTimer MKTwitchTimer { get { return timer; } }
        public static DispatcherTimer MKKludgeTimer { get { return kludgeTimer; } }
        public static string UserName { get; set; }
        public static DateTime LastPull { get; internal set; }

        #region Custom EventArgs
        public class MKTwitchUpdatingEventArgs : EventArgs
        {
            public bool IsUpdating;
        }

        public class MKTwitchEventArgs : EventArgs
        {
            public User User { get; set; }
            /// <summary>
            /// The bew Game text
            /// </summary>
            public string NewGame { get; set; }
            public string OldGame { get; set; }
            /// <summary>
            /// The new Status text
            /// </summary>
            public string NewStatus { get; set; }
            public string OldStatus { get; set; }
            /// <summary>
            /// Indicates if the Toast should actually be displayed
            /// </summary>
            public bool DisplayToast { get; set; }
        }

        public class MKTwitchFollowedUsersEventArgs:EventArgs
        {
            public List<User> FollowedUsers;
        }
        #endregion

        #region Events
        /// <summary>
        /// Invoked when a streamer goes from Offline to Online
        /// </summary>
        public static event EventHandler<MKTwitchEventArgs> Online;
        /// <summary>
        /// Invoked when a streamer goes from Online to Offline
        /// </summary>
        public static event EventHandler<MKTwitchEventArgs> OffLine;
        /// <summary>
        /// Invoked when a streamer update check has started
        /// </summary>
        public static event EventHandler<MKTwitchUpdatingEventArgs> UpdateStarted;
        /// <summary>
        /// Invoked when a streamer update check has completed
        /// </summary>
        public static event EventHandler<MKTwitchUpdatingEventArgs> UpdateCompleted;
        /// <summary>
        /// Invoked whenever the followedUsers collection has changed
        /// </summary>
        public static event EventHandler<MKTwitchFollowedUsersEventArgs> FollowedUsersChanged;
        /// <summary>
        /// Invoked when the MKTwitch.Start() method has completed
        /// </summary>
        public static event EventHandler StartCompleted;
        /// <summary>
        /// Invoked when a streamer changes his Game
        /// </summary>
        public static event EventHandler<MKTwitchEventArgs> GameChanged;
        /// <summary>
        /// Invoked when a streamer changes his Status
        /// </summary>
        public static event EventHandler<MKTwitchEventArgs> StatusChanged;
        /// <summary>
        /// Invoked when the user adds a new streamer to his followed list
        /// </summary>
        public static event EventHandler<MKTwitchEventArgs> Followed;
        /// <summary>
        /// Invoked when the user removes a streamer from his followed list
        /// </summary>
        public static event EventHandler<MKTwitchEventArgs> Unfollowed;
        #endregion

        #region Event Trigger Methods
        /// <summary>
        /// Invoked when a followed user goes Online (Starts Streaming)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="displayToast"></param>
        private static void OnOnline(User user,bool displayToast=true)
        {
            EventHandler<MKTwitchEventArgs> handler = Online;
            handler?.Invoke(null, new MKTwitchEventArgs { User = user, DisplayToast=displayToast });
        }

        /// <summary>
        /// Invoked when a streamer goes Offline (Stops Streaming)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="displayToast"></param>
        private static void OnOffline(User user,bool displayToast=true)
        {
            EventHandler<MKTwitchEventArgs> handler = OffLine;
            handler?.Invoke(null, new MKTwitchEventArgs{ User = user, DisplayToast = displayToast});
        }

        /// <summary>
        /// Invoked when the call to GetStreamers() is starting. The class's IsUpdating property will be
        /// set to isUpdating (true for OnUpdateStarted). **Is the 'isUpdating' flag needed? Can't remember**
        /// </summary>
        /// <param name="isUpdating"></param>
        private static void OnUpdateStarted(bool isUpdating)
        {
            EventHandler<MKTwitchUpdatingEventArgs> handler = UpdateStarted;
            handler?.Invoke(null, new MKTwitchUpdatingEventArgs{ IsUpdating = isUpdating});
        }

        /// <summary>
        /// Invoked when the call to GetStreamers() is completed. The class's IsUpdating property will be
        /// set to isUpdating (false for OnUpdateCompleted). **Is the 'isUpdating' flag needed? Can't remember**
        /// </summary>
        /// <param name="isUpdating"></param>
        private static void OnUpdateCompleted(bool isUpdating)
        {
            EventHandler<MKTwitchUpdatingEventArgs> handler = UpdateCompleted;
            handler?.Invoke(null, new MKTwitchUpdatingEventArgs { IsUpdating = IsUpdating });
        }

        /// <summary>
        /// Invoked when the FollowedUsers collection has changed.
        /// </summary>
        private static void OnFollowedUsersChanged()
        {
            EventHandler<MKTwitchFollowedUsersEventArgs> handler = FollowedUsersChanged;
            handler?.Invoke(null, new MKTwitchFollowedUsersEventArgs { FollowedUsers = followedStreamers });
        }

        /// <summary>
        /// Invoked when the MKTwitch.Start() method has completed
        /// </summary>
        private static void OnStartCompleted()
        {
            EventHandler handler = StartCompleted;
            handler?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Invoked when a streamer changes their Game
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newGame"></param>
        private static void OnGameChanged(User user, string newGame, string oldGame)
        {
            EventHandler<MKTwitchEventArgs> handler = GameChanged;
            handler?.Invoke(null, new MKTwitchEventArgs() {User = user, NewGame = newGame, OldGame = oldGame});
        }

        /// <summary>
        /// Invoked when a streamer changes their Status
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newStatus"></param>
        /// <param name="oldStatus"></param>
        private static void OnStatusChanged(User user, string newStatus, string oldStatus)
        {
            EventHandler<MKTwitchEventArgs> handler = StatusChanged;
            handler?.Invoke(null, new MKTwitchEventArgs() {User = user, NewStatus = newStatus, OldStatus = oldStatus });
        }

        /// <summary>
        /// Invoked whenever the user Follows a new streamer
        /// </summary>
        /// <param name="user"></param>
        private static void OnFollowed(User user)
        {
            EventHandler<MKTwitchEventArgs> handler = Followed;
            handler?.Invoke(null, new MKTwitchEventArgs() { User = user });
        }

        /// <summary>
        /// Invoked whenever the user UnFollows a streamer
        /// </summary>
        /// <param name="user"></param>
        private static void OnUnfollowed(User user)
        {
            EventHandler<MKTwitchEventArgs> handler = Unfollowed;
            handler?.Invoke(null, new MKTwitchEventArgs() { User = user });
        }

        #endregion

        /// <summary>
        /// List of streamers the user follows
        /// </summary>
        public static List<User> followedStreamers = new List<User>();
        const string twitchUrl = "https://api.twitch.tv/kraken/";

        /// <summary>
        /// Fills the followedUser collection with the people that 'userName' follows
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static async Task ChangeUser(string userName)
        {
            // if the new username is the same as the old one then do nothing
            if (UserName == userName) return;
            IsChangingUser = true;
            UserName = userName;
            timer.Stop();
            int loopCount = 0;

            while(IsUpdating)
            {
                if (loopCount < 5)
                {
                    await Task.Delay(500);
                    loopCount++;
                }
                else
                    break;     
            }

            // Assume a loopCount of 5 means that the IsUpdating flag is not getting cleared
            // then skip the SeupStreamTracker() and just restart the timer
            if (loopCount < 5)
            {
                followedStreamers.Clear();
                await SetupStreamTracker(userName);
            }
            timer.Start();
            IsChangingUser = false;
        }

        /// <summary>
        /// Starts the MKTwitch engine, filling the followedUser collection and
        /// starting up the Timer. If successful the IsStarted property is set to true
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="skipToastAtStart"></param>
        public async static Task Start(string userName, bool skipToastAtStart=false)
        {
            skipPopupsAtStart = skipToastAtStart;
            UserName = userName;
            await SetupStreamTracker( userName);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(40);
            int dotCount = 0;
            timer.Tick+= async(s,e) => 
            {
                if (followedStreamers.Count == 0) return;
              //  OnUpdateStarted(IsUpdating = true);
                timer.Stop();
                try {
                    await Update();
                    if (dotCount++ < 50)
                        Console.Write(".");
                    else
                    {
                        Console.WriteLine(".");
                        dotCount = 0;
                    }
                }
                catch(Exception ex)
                {
                    string m = $"{nameof(timer.Tick)} await Update() threw Exception: of type {ex.GetType().Name}. ex.Message is '{ex.Message}'";
                    Console.WriteLine(m);
                    Log.WriteLog(m, "TimerException.log");
                }
                finally
                {
                    timer.Start();
                  
                }
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
          
            };
            timer.Start();
            StartKludgeTimer();
            IsStarted = true;
            OnStartCompleted();
        }

        /// <summary>
        /// KludgeTimer added to restart the main timer when it becomes permenantly disabled after some
//      /// network error. Stopgap fix. Needs fixing properly
        /// </summary>
        private static void StartKludgeTimer()
        {
            kludgeTimer = new DispatcherTimer();
            kludgeTimer.Interval = TimeSpan.FromSeconds(1);
            int timerCount = 0;
            int dotCount = 0;
            kludgeTimer.Tick += (s, e) => {
                if (timer.IsEnabled)
                {
                    timerCount = 0;
                    // Console.Write(".");
                    if (dotCount >= 60)
                    {
                       // Console.WriteLine(".");
                        dotCount = 0;
                    }
                    else
                    {
                       // Console.Write(".");
                        dotCount++;
                    }
                }
                else // timer disabled
                {
                    if (timerCount++ < 120)
                    {
                        string message = $"timer disabled. timerCount={timerCount}";
                        Log.WriteLog(message, "kludgeTimerLog.txt");
                        Console.WriteLine(message);
                        return;
                    }
                    timer.Start();
                    timerCount = 0;
                    string s1 = "KludgeTimer is starting main timer";
                    Log.WriteLog(s1, "kludgeTimerLog.txt");
                    Console.WriteLine(s1);
                }
            };
            kludgeTimer.Start();
        }




        /// <summary>
        /// Updates the streaming info for followed users, triggering any Online/Offline/GameChanged/StatusChanged events where needed
        /// </summary>
        /// <returns></returns>
        private static async Task Update()
        {
            //            throw new MKTwitchTestException("Thrown in Update() before UpdateFollowedUsers() call");
            try
            {
                await UpdateFollowedUsers(UserName);

            }
            catch(MKTwitchUpdateFollowedUsersTestException ex)
            {
                Log.WriteLog(ex.Message, "DebugLog.txt");
                Console.WriteLine(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.Message, "DebugLog.txt");
                Console.WriteLine(ex.Message);
            }



            // ************ 14/8/2017 *****************
            // ****************************************
            // Try/Catch added to catch index Exception
            TwitchStreamers.RootObject streamers = null;
            try
            {
                OnUpdateStarted(true);
                streamers = await GetStreamers();
            }
            catch(Exception ex)
            {
                Log.WriteLog(ex.Message, "DebugLog.txt");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                OnUpdateCompleted(false); 
            }
            if (streamers == null) return;


            var nonStreamers = followedStreamers.Where(i => !streamers.Streams.Any(x => x.Channel.DisplayName == i.Name));

            // Do the streamers bit first
            foreach (var streamer in streamers.Streams)
            {
                // Get the followed user who is now streaming
                var followed = followedStreamers.First(i => i.Name == streamer.Channel.DisplayName);

                // Update his info
                followed.StreamCreatedAt = (DateTime.Parse(streamer.CreatedAt, new CultureInfo("en-GB", true), DateTimeStyles.AssumeUniversal)).ToLongTimeString();
                followed.NumViewers = streamer.Viewers;

                // if user was already streaming then check if Game or Status have changed. If they 
                // have then throw a popup else just continue
                if (followed.IsStreaming)
                {
                    if (followed.Game != streamer.Game)
                    {
                        var oldGame = followed.Game;
                        followed.GameChangeCount++;
                        Console.WriteLine($"\n{followed.Name} followed.Game = {oldGame} streamer.channel.game = {streamer.Channel.Game} streamer.game = {streamer.Game} **followed.GameChangeCount = {followed.GameChangeCount}**");
                   
                        // Game has to be different for two consecutive pulls before we change our version of it
                        if (followed.GameChangeCount < 2) continue;
                        followed.Game = streamer.Game;
                        followed.GameChangeCount = 0;           // Reset the count
                        OnGameChanged(followed, streamer.Game, oldGame);
                    }
                    if (followed.Status != streamer.Channel.Status)
                    {
                        followed.StatusChangeCount++;
                        Console.WriteLine($"\n{followed.Name} followed.Status = {followed.Status} streamer.channel.status = {streamer.Channel.Status} **followed.StatusChangeCount = {followed.StatusChangeCount}**");
                  
                        // Status has to be different for two consecutive pulls before we change our version of it
                        if (followed.StatusChangeCount < 2) continue;
                        var oldStatus = followed.Status;
                        followed.Status = streamer.Channel.Status;
                        followed.StatusChangeCount = 0;         // Reset the count
                        OnStatusChanged(followed, streamer.Channel.Status, oldStatus);
                    }
                    followed.OfflineCount = 0;
                    continue;
                }


                // user has started streaming so...
                // set his isStreaming property to true
                followed.IsStreaming = true;
                // and throw up a popup
                if(!CancelPopupCycle) OnOnline(followed);
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
                    Console.WriteLine($"\n{ns.Name}'s offlineCount is {ns.OfflineCount}");
                    ns.IsStreaming = false;
                    if(!CancelPopupCycle) OnOffline(ns);
                    Console.WriteLine($"\n{ ns.Name} has gone Offline");
                    await Task.Delay(6000);
                }
            }
        }


        private static void FollowedUsersToConsole()
        {
            followedStreamers.ForEach(u => {
                Console.WriteLine(u.Name);
                Console.WriteLine(u.Status);
                Console.WriteLine(u.Game);
                Console.WriteLine(u.NumViewers);
                Console.WriteLine(u.ThumbnailPath);
                Console.WriteLine(u.IsStreaming);
                Console.WriteLine(u.StreamCreatedAt);
            });
        }

        //******************************************************************************
        // ********** THE UpdateFollowedUsers() METHODS ARE COMPLETELY FUCKED **********
        // *** THEY ARE STOPPING THE 'CHANGE USERNAME' FUNCTIONALITY FROM WORKING ***
        // *******************************  IDIOT!! ************************************
        // *****************************************************************************
        // Think it's fixed but needs testing

        /// <summary>
        /// Check to see if we have Followed/Unfollowed any streamers
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// <exception cref="MKTwitchUpdateFollowedUsersTestException"></exception>
        public static async Task UpdateFollowedUsers(string userName)
        {
            GetALLUsersFollowers_Result result = default(GetALLUsersFollowers_Result);
            try
            {
                //throw new NullReferenceException("Test Exeption");
                result = await GetALLUsersFollowers(userName);
            }
            catch(Exception ex)
            {
                Log.WriteLog($"{nameof(UpdateFollowedUsers)} threw an Exception when calling {nameof(GetALLUsersFollowers)}. ex.Message = {ex.Message}","DebugLog.txt");
                throw new MKTwitchUpdateFollowedUsersTestException($"{nameof(GetALLUsersFollowers)} call failed in the {nameof(UpdateFollowedUsers)} method. ex.Message = {ex.Message}");
            }

            var followed = (Twitch.Root)result.Followed;       
            if (followed == null) return;
            await UpdateFollowedUsers(followed);
        }

        private static async Task UpdateFollowedUsers(Twitch.Root user)
        {
            if (user.follows.Count == 0)
            {
                Log.WriteLog("Received a followedUser count of 0.\nSkipping UpdateFollowedUsers", "DebugLog.txt");
               // MessageBox.Show("received a followedUser count of 0.\nSkipping UpdateFollowedUsers");
                return;
            }
            if (followedStreamers.Count == 0) return;

            bool followedUsersChanged = false;
            if (user == null) return;

            // Check if we've followed another streamer and if so add them to our followed collection
            foreach (var u in user.follows)
            {
                if (followedStreamers.Any(i => i.Name == u.channel.display_name) == false)
                {
                    followedUsersChanged = true;
                    var newUser = await CreateUserFromTwitchFollow(u);
                    followedStreamers.Add(newUser);
                    OnFollowed(newUser);
                }
            }

            var toRemove = new List<User>();
            // Check for unfollows here
            foreach (var u in followedStreamers)
            {
                if (user.follows.Any(i => i.channel.display_name == u.Name) == false)
                {
                    toRemove.Add(u);
                    followedUsersChanged = true;
                }
            }
            // Remove unfollows if any
            foreach (var rem in toRemove)
            {
                followedStreamers.Remove(rem);
                OnUnfollowed(rem);
            }

            if(followedUsersChanged)
                OnFollowedUsersChanged();
        }


        private static async Task<User> CreateUserFromTwitchFollow(Twitch.Follow follow)
        {
            IsUserLiveData isUserLiveData = await IsUserLiveAsync(follow.channel.display_name);
            bool isUserLive = isUserLiveData.IsLive;

            // Remove the date and the trailing Z from the createdAt string
            if (isUserLiveData.CreatedAt != "")
            {
                //var arr = isUserLiveData.CreatedAt.Split('T');
                //isUserLiveData.CreatedAt = arr[1].Remove(arr[1].Length - 1);
                isUserLiveData.CreatedAt = (DateTime.Parse(isUserLiveData.CreatedAt, new CultureInfo("en-GB", true), DateTimeStyles.AssumeUniversal)).ToLongTimeString();
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
        /// Get userNames' followed channels and store them in the followedStreamers collection
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private async static Task SetupStreamTracker( string userName)
        {
            CancelPopupCycle = false;

            var fs = await GetALLUsersFollowers(userName);
            var users = (Twitch.Root)fs.Followed;
            var streamers = (TwitchStreamers.RootObject)fs.Streamers;
            // If this is the first run (MKTwitch.Start() hasnt completed) then skip
            // any followed user update check this time.
            if(MKTwitch.IsStarted)
                await UpdateFollowedUsers(users);

            // Clear followedUsers collection before we start to refill it with new users
            followedStreamers.Clear();

            foreach (var followedUser in users.follows)
            {
                bool isUserLive = false;
                string createdAt = "";
                int numViewers=0;

                if (streamers !=null && streamers.Streams.Count > 0)
                {
                    // if this followedUser is also a streamer then get his/her streamer object from streamers
                    // so we can get his streaming information
                    var streamer = streamers.Streams.FirstOrDefault(i => i.Channel.DisplayName == followedUser.channel.display_name);
                    if (streamer != null)
                    {
                        isUserLive = true;

                        createdAt = (DateTime.Parse(streamer.CreatedAt, new CultureInfo("en-GB", true), DateTimeStyles.AssumeUniversal)).ToLongTimeString();
                        numViewers = streamer.Viewers;
                    }
                }

                BitmapImage bm = null;
                // If user does not have a logo then use the Twitch default
                if (followedUser.channel.logo == null)
                {
                    bm = new BitmapImage(new Uri(@"/TwitchAlert;component/Images/404_user_150x150.png", UriKind.Relative));
                }
                else
                {
                    // If the Thumbnail (logo) Image has already been cached to file then get it from there.
                    // Else download it.
                    var cachedFilename = ImageSaver.GetCachedImageFilename(followedUser.channel.logo);
                    bm = (!string.IsNullOrEmpty(cachedFilename) && File.Exists(cachedFilename)) ? new BitmapImage(new Uri(cachedFilename)) : DownloadImage(followedUser.channel.logo);
                }

                // followedUsers.Add(new User { Name = f.channel.display_name, IsStreaming = isUserLive, NumViewers = isUserLiveData.NumViewers, Game = f.channel.game,StreamCreatedAt = isUserLiveData.CreatedAt, ThumbnailPath = f.channel.logo, Thumbnail = bm ,Link = f.channel.url, Status = f.channel.status});
                 followedStreamers.Add(new User { Name = followedUser.channel.display_name, IsStreaming = isUserLive, NumViewers = numViewers, Game = followedUser.channel.game,StreamCreatedAt = createdAt, ThumbnailPath = followedUser.channel.logo, Thumbnail = bm ,Link = followedUser.channel.url, Status = followedUser.channel.status});

                if (isUserLive) Console.WriteLine($"\n{followedUser.channel.display_name} is {(isUserLive ? "Live " : "Not Live")}{(isUserLive ? "with " + followedUser.channel.game : "")}");
            }

            OnFollowedUsersChanged();

            if (skipPopupsAtStart==false)
            {
                IsPopupCycleRunning = true;
                foreach (var user in followedStreamers.Where(i => i.IsStreaming))
                {
                    if (CancelPopupCycle)
                        OnOnline(user,false);
                    else
                    { 
                        OnOnline(user,true);
                        await Task.Delay(6000);
                    }
                }
                CancelPopupCycle = false;
                IsPopupCycleRunning = false;
            }
        }

        public static bool IsTimerEnabled()
        {
            if (timer == null) return false;
            return timer.IsEnabled;
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
        static Twitch.Root GetUsersFollowedChannels(string userName, int limit = 100,int offset=0, string sortDirection = "DESC")
        {
            //GET https://api.twitch.tv/kraken/users/test_user1/follows/channels
            string url = $"{twitchUrl}users/{userName}/follows/channels?direction={sortDirection}&limit={limit}&offset={offset}&sortby=created_at";
            return JsonConvert.DeserializeObject<Twitch.Root>(Get(url));
        }

        /// <summary>
        /// Returns all the people that userName follows
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="limit"></param>
        /// <param name="sortDirection"></param>
        /// <returns>Twitch.Root</returns>
        static async Task<Twitch.Root> GetUsersFollowedChannelsAsync(string userName, int limit = 100, int offset = 0, string sortDirection = "DESC")
        {
            //GET https://api.twitch.tv/kraken/users/test_user1/follows/channels
            string url = $"{twitchUrl}users/{userName}/follows/channels?direction={sortDirection}&limit={limit}&offset={offset}&sortby=created_at";
            var result = JsonConvert.DeserializeObject<Twitch.Root>(await GetAsync(url));
            return result;
        }

        public async static Task<bool> UserExists(string userName)
        {
            //GET https://api.twitch.tv/kraken/user
            string url = $"{twitchUrl}users/{userName}";
            var res = await GetAsync(url);
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
                iuld.IsLive = user?.stream != null;
               
                // iuld.CreatedAt = user?.stream?.created_at == null ? "" : user.stream.created_at;
                iuld.CreatedAt = user?.stream?.created_at ?? "";
               
                // iuld.Game = user?.stream?.game == null ? "" : user.stream.game;
                iuld.Game = user?.stream?.game ?? "";

                iuld.NumViewers = user?.stream == null ? 0 : user.stream.viewers;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return iuld;
        }

        /// <summary>
        /// Returns all of the people in the followedUsers collection who are currently streaming
        /// </summary>
        /// <returns>Task<TwitchStreamers.RootObject></returns>
        private static async Task<TwitchStreamers.RootObject> GetStreamers()
        {
           // throw new MKTwitchTestException("Thrown in GetStreamers()");

            string url = "https://api.twitch.tv/kraken/streams?channel=";


            string users = followedStreamers.Aggregate("", (current, u) => current + (u.Name + ","));

           // Log.WriteLog($"GetStreamers() Debug: users = {users}", "GetStreamersDebug.txt");

            url += users.Remove(users.Length - 1);

           // Log.WriteLog($"GetStreamers() Debug: url = {url}", "GetStreamersDebug.txt");

            string streamers = null;

            try
            {
                streamers = await GetAsync(url);
            }

            catch (Exception ex)
            {
             //   Log.WriteLog($"GetStreamers() Debug: {ex.Message}", "GetStreamersDebug.txt");
                throw ex;
            }

            return JsonConvert.DeserializeObject<TwitchStreamers.RootObject>(streamers);
        }

        /// <summary>
        /// Returns all of the people in the 'users' collection who are currently streaming
        /// </summary>
        /// <param name="users"></param>
        /// <returns>Task<TwitchStreamers.RootObject></returns>
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
            //wRequest.ContentType = "application/json";
            wRequest.Accept = "application/vnd.twitchtv.v3+json";
            wRequest.Method = "GET";
            wRequest.Headers.Add("Client-ID", Properties.Settings.Default.settingsCI);

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
            //wRequest.ContentType = "application/json"; // This line started breaking it
            wRequest.Accept = "application/vnd.twitchtv.v3+json";
            wRequest.Method = "GET";
            wRequest.Headers.Add("Client-ID", Properties.Settings.Default.settingsCI);

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
                Console.WriteLine($"MKTwitch.GetAsync() Failed with {ex.Message}.");
            
                //MessageBox.Show($"MKTwitch.GetAsync() Failed with {ex.Message}.");
                throw;
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
                // Kludge to trigger ImageCacher constructor
                var t = ImageCacher.DirectoryExists;

                var ourImg = s2 as BitmapImage;
                Console.WriteLine("DownloadImage() Failed to decode image {0}", ourImg.UriSource);
                if (ourImg != null)
                {
                    rhd(ourImg, handlerCompleted, handlerDownloadFailed, handlerDecodeFailed);
                }
            };
            handlerDownloadFailed = (s3, e2) =>
            {
                // Kludge to trigger ImageCacher constructor
                var t = ImageCacher.DirectoryExists;

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
                   // Console.WriteLine("DownloadImage() Completed {0}", ourImg.UriSource);
                    rhd(ourImg, handlerCompleted, handlerDownloadFailed, handlerDecodeFailed);
                    if (ourImg.CanFreeze) ourImg.Freeze();
                    ImageCacher.CacheIfNotCached(ourImg, ourImg.UriSource.OriginalString);
                }
            };

            var img = new BitmapImage();

            img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.DecodePixelHeight = 74;
                img.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                img.UriSource = new Uri(imageUrl);
                img.DownloadCompleted += handlerCompleted;
                img.DownloadFailed += handlerDownloadFailed;
                img.DecodeFailed += handlerDecodeFailed;
            img.EndInit();
            return img;
        }
    }
}
