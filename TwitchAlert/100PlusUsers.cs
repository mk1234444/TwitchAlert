using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchAlert.classes;

namespace TwitchAlert.classes
{
    public static partial class MKTwitch
    {
        public struct GetALLUsersFollowers_Result
        {
            public object Followers;
            public object Streamers;
        }

        public async static Task<GetALLUsersFollowers_Result> GetALLUsersFollowers(string userName)
        {
            Twitch.Root followers = new Twitch.Root();
            TwitchStreamers.RootObject streamers = new TwitchStreamers.RootObject();
            int numFollowers;
            int offset = 0;

            while (true)
            {
                var u = await GetUsersFollowedChannelsAsync(userName, 100, offset);
                if (u.follows.Count == 0) break;

                var str = await GetStreamers(u);
                followers.follows.AddRange(u.follows);
                streamers.streams.AddRange(str.streams);
                offset += 100;
            }

           return new GetALLUsersFollowers_Result() { Followers = followers, Streamers = streamers };
        }
    }
}
