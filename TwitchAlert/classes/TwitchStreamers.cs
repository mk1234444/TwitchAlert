using System.Collections.Generic;
using Newtonsoft.Json;

namespace TwitchAlert.classes
{
    class TwitchStreamers
    {
        public class Links
        {
            [JsonProperty("self")]
            public string Self { get; set; }
        }

        public class Preview
        {
            [JsonProperty("small")]
            public string Small { get; set; }
            [JsonProperty("medium")]
            public string Medium { get; set; }
            [JsonProperty("large")]
            public string Large { get; set; }
            [JsonProperty("template")]
            public string Template { get; set; }
        }

        public class Links2
        {
            [JsonProperty("self")]
            public string Self { get; set; }
            [JsonProperty("follows")]
            public string Follows { get; set; }
            [JsonProperty("commercial")]
            public string Commercial { get; set; }
            [JsonProperty("stream_key")]
            public string StreamKey { get; set; }
            [JsonProperty("chat")]
            public string Chat { get; set; }
            [JsonProperty("features")]
            public string Features { get; set; }
            [JsonProperty("subscriptions")]
            public string Subscriptions { get; set; }
            [JsonProperty("editors")]
            public string Editors { get; set; }
            [JsonProperty("videos")]
            public string Videos { get; set; }
            [JsonProperty("teams")]
            public string Teams { get; set; }
        }

        public class Channel
        {
            [JsonProperty("_links")]
            public Links2 Links { get; set; }
            [JsonProperty("background")]
            public object Background { get; set; }
            [JsonProperty("banner")]
            public object Banner { get; set; }
            [JsonProperty("broadcaster_language")]
            public string BroadcasterLanguage { get; set; }
            [JsonProperty("display_name")]
            public string DisplayName { get; set; }
            [JsonProperty("game")]
            public string Game { get; set; }
            [JsonProperty("logo")]
            public string Logo { get; set; }
            [JsonProperty("mature")]
            public bool? Mature { get; set; }
            [JsonProperty("status")]
            public string Status { get; set; }
            [JsonProperty("partner")]
            public bool Partner { get; set; }
            [JsonProperty("url")]
            public string Url { get; set; }
            [JsonProperty("video_banner")]
            public string VideoBanner { get; set; }
            [JsonProperty("_id")]
            public int Id { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("created_at")]
            public string CreatedAt { get; set; }
            [JsonProperty("updated_at")]
            public string UpdatedAt { get; set; }
            [JsonProperty("delay")]
            public object Delay { get; set; }
            [JsonProperty("followers")]
            public int Followers { get; set; }
            [JsonProperty("profile_banner")]
            public string ProfileBanner { get; set; }
            [JsonProperty("profile_banner_background_color")]
            public string ProfileBannerBackgroundColor { get; set; }
            [JsonProperty("views")]
            public int Views { get; set; }
            [JsonProperty("language")]
            public string Language { get; set; }
            public override string ToString() => DisplayName;
        }

        public class Stream
        {
            [JsonProperty("_id")]
            public object Id { get; set; }
            [JsonProperty("game")]
            public string Game { get; set; }
            [JsonProperty("viewers")]
            public int Viewers { get; set; }
            [JsonProperty("created_at")]
            public string CreatedAt { get; set; }
            [JsonProperty("video_height")]
            public int VideoHeight { get; set; }
            [JsonProperty("average_fps")]
            public double AverageFps { get; set; }
            [JsonProperty("is_playlist")]
            public bool IsPlaylist { get; set; }
            [JsonProperty("_links")]
            public Links Links { get; set; }
            [JsonProperty("preview")]
            public Preview Preview { get; set; }
            [JsonProperty("channel")]
            public Channel Channel { get; set; }


            public override string ToString() => Channel.Name + " " + Game;
        }

        public class Links3
        {
            [JsonProperty("self")]
            public string Self { get; set; }
            [JsonProperty("next")]
            public string Next { get; set; }
            [JsonProperty("featured")]
            public string Featured { get; set; }
            [JsonProperty("summary")]
            public string Summary { get; set; }
            [JsonProperty("followed")]
            public string Followed { get; set; }
        }

        public class RootObject
        {
            public RootObject()
            {              
                Streams = new List<Stream>();
            }
            [JsonProperty("streams")]
            public List<Stream> Streams { get; set; }
            [JsonProperty("_total")]
            public int Total { get; set; }
            [JsonProperty("_links")]
            public Links3 Links { get; set; }
        }
    }
}
