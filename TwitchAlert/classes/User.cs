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
        public string Name { get; set; }
        public bool IsStreaming { get; set; }
        public int NumViewers { get; set; }
        public string Game { get; set; }
        public string StreamCreatedAt { get; set; }
        public string ThumbnailPath { get; set; }
        public BitmapImage Thumbnail { get; set; }
        public string Link { get; set; }
        public string Status { get; set; }
        public int OfflineCount { get; set; }
        public override string ToString() => Name;
        
    }
}
