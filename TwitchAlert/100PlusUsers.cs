using System.Threading.Tasks;

namespace TwitchAlert.classes
{
    public static partial class MKTwitch
    {
        public struct GetALLUsersFollowers_Result
        {
            public object Followed;
            public object Streamers;
        }

        /// <summary>
        /// Retrieve followed streamers and the ones that are currently streaming
        /// and return them in a GetALLUsersFollowers_Result struct
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async static Task<GetALLUsersFollowers_Result> GetALLUsersFollowers(string userName)
        {
            Twitch.Root followed = new Twitch.Root();
            TwitchStreamers.RootObject streamers = new TwitchStreamers.RootObject();
            int offset = 0;

            while (true)
            {
                var u = await GetUsersFollowedChannelsAsync(userName, 100, offset);
                if (u.follows.Count == 0) break;

                var str = await GetStreamers(u);
                followed.follows.AddRange(u.follows);
                streamers.Streams.AddRange(str.Streams);
                offset += 100;
            }
            if(followed.follows.Count == 0)
            {
                System.Console.WriteLine();
            }

           return new GetALLUsersFollowers_Result() { Followed = followed, Streamers = streamers };
        }
    }
}
