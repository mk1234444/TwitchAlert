using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TwitchAlert.classes
{
    public class User
    {
        /// <summary>
        /// Name of the followed Twitcher
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Bool indicating if the followed Twitcher current streaming
        /// </summary>
        public bool IsStreaming { get; set; }
        /// <summary>
        /// Number of viewers the followed Twitcher currently has
        /// </summary>
        public int NumViewers { get; set; }
        /// <summary>
        /// The name of the game the followed Twitcher is currently playing
        /// </summary>
        public string Game { get; set; }
        /// <summary>
        /// Time the streamer started streaming
        /// </summary>
        public string StreamCreatedAt { get; set; }
        public string ThumbnailPath { get; set; }
        public BitmapImage Thumbnail { get; set; }
        public string Link { get; set; }
        public string Status { get; set; }
        /// <summary>
        /// Used to store consecutive 'Offline' reports. A count of 2 means user is *actually* offline.
        /// Used to compensate for Twitch misreporting Offline state
        /// </summary>
        public int OfflineCount { get; set; }
        public int GameChangeCount { get; set; }
        public int StatusChangeCount { get; set; }
        public override string ToString() => Name;
        
    }
}
