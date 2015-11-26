using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TwitchAlert.classes
{
    public static class ImageSaver
    {
        /// <summary>
        /// Save the BitmapImage img to the File filename as a .bmp
        /// </summary>
        /// <param name="img"></param>
        /// <param name="fileName"></param>
        public static void SaveToBmp(BitmapImage img, string fileName)
        {
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            using (var filestream = new FileStream(fileName, FileMode.Create))
            {
                encoder.Save(filestream);
            }
        }

        /// <summary>
        /// Save the BitmapImage img to the File filename as a .png
        /// </summary>
        /// <param name="img"></param>
        /// <param name="fileName"></param>
        public static void SaveToPng(BitmapImage img, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            using (var filestream = new FileStream(fileName, FileMode.Create))
            {
                encoder.Save(filestream);
            }
        }

        /// <summary>
        /// Save the BitmapImage img to the File filename as a jpg
        /// </summary>
        /// <param name="img"></param>
        /// <param name="fileName"></param>
        public static void SaveToJpg(BitmapImage img, string fileName)
        {
            fileName = RemoveExtraColonFromPath(fileName);
            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            try
            {
                using (var filestream = new FileStream(fileName, FileMode.Create))
                {
                    encoder.Save(filestream);
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void SaveUsingEncoder(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);

            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }

        const string IMAGE_CACHE_DIRECTORY_NAME = "thumbnail_cache/";
        /// <summary>
        /// Converts the imageUrl into a valid filename
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <returns>string</returns>
        public static string GetCachedImageFilename(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return "";
            var p = Directory.GetCurrentDirectory() + @"\" + IMAGE_CACHE_DIRECTORY_NAME ;
            var path = RemoveExtraColonFromPath(p + imageUrl.Replace("http://", "").Replace("https://", "").Replace(@"/", "_").Replace("?", "_"));
            if (path.Length > 259)
            {
                var s = path.Remove(0, (p).Length).Remove(0, path.Length - 250).Insert(0, "_short_");
                path = p + s;
            }
            return path;
        }

        private static string RemoveExtraColonFromPath(string path)
        {
            var x = path.Substring(2);

            if (x.Contains(":"))
            {
                x = x.Replace(":", "");
            }
            return path.Substring(0, 2) + x;
        }
    }
}
