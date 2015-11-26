using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace TwitchAlert.classes
{
    public static class ImageCacher
    {
        //internal class ImageItem
        //{
        //    public string Source { get; set; }
        //    public string Url { get; set; }
        //    public string Filename { get; set; }
        //}

        const string IMAGE_CACHE_DIRECTORY_NAME = "thumbnail_cache/";

    //    public static List<ImageItem> Images { get; set; }

        static ImageCacher()
        {
            //Images = new List<ImageItem>();
            if (!Directory.Exists(IMAGE_CACHE_DIRECTORY_NAME)) Directory.CreateDirectory(IMAGE_CACHE_DIRECTORY_NAME);
        }

        /// <summary>
        /// Scan the image_cache Directory and recreate the Images collection
        /// </summary>
        //public static void RescanImageDirectory()
        //{
        //    var files = Directory.EnumerateFiles(IMAGE_CACHE_DIRECTORY_NAME, "*.jpg|*.jpeg|*.bmp|*.png", SearchOption.TopDirectoryOnly);
        //}

        //public static void AddImage(string source,string url,string filename)
        //{
        //    if (source == null || url == null || filename == null) throw new ArgumentException("Arguments in ImageCacher.AddImage() cannot be null");
        //    if(!Images.Any(i=>i.Source == source && i.Url == url && i.Filename == filename))
        //        Images.Add(new ImageItem { Source = source, Url = url, Filename = filename });
        //    else
        //        Console.WriteLine($"ImageCacher.AddImage() image already exists in cache\nSource:{source}\nUrl:{url}\nFilename:{filename}\n");
        //}

        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="source"></param>
        /// <param name="url"></param>
        /// <param name="filename"></param>
        //public static void RemoveImage(string source, string url, string filename)
        //{
        //    var im = Images.FirstOrDefault(i => i.Source == source && i.Url == url && i.Filename == filename);
        //    if (im != null)
        //    {
        //        Images.Remove(im);
        //    }
        //}

        /// <summary>
        /// Seach the cache to see if the image for the url is already in the cache
        /// </summary>
        /// <param name="source"></param>
        /// <param name="url"></param>
        /// <returns>string</returns>
        //public static string FindImage(string source,string url)
        //{
        //    var image = Images.FirstOrDefault(i => i.Source == source && i.Url == url);
        //    return image?.Filename;
        //}

        ///// <summary>
        ///// Iterate through the collection adding any thumbnails to the cache that arent already there
        ///// </summary>
        ///// <param name="collection"></param>
        //public void UpdateCultureCache(IEnumerable<Culture> collection)
        //{
        //    foreach (var i in collection.Where(x=>x.Source.Contains("Guardian")))
        //    {
        //        if (i.ThumbnailPath.StartsWith("http"))
        //        {
        //            string filename = IMAGE_CACHE_DIRECTORY_NAME + i.ThumbnailPath.Replace("http://", "").Replace(@"/","_");
        //            if (i.ThumbnailPath.EndsWith(".png") && !File.Exists(filename))
        //            {
        //                ImageSaver.SaveToPng(i.Thumbnail, filename);
        //                Images.Add(new ImageItem { Source = i.Source, Filename = filename, Url = i.ThumbnailPath });
        //            }
        //            else if ((i.ThumbnailPath.EndsWith(".jpg") || i.ThumbnailPath.EndsWith(".jpeg")) && !File.Exists(filename))
        //            {
        //                ImageSaver.SaveToJpg(i.Thumbnail, filename);
        //                Images.Add(new ImageItem { Source = i.Source, Filename = filename, Url = i.ThumbnailPath });
        //            }
        //            else if (i.ThumbnailPath.EndsWith(".bmp") && !File.Exists(filename))
        //            {
        //                ImageSaver.SaveToBmp(i.Thumbnail, filename);
        //                Images.Add(new ImageItem { Source = i.Source, Filename = filename, Url = i.ThumbnailPath });
        //            }
        //        }
        //    }
        //}



        /// <summary>
        /// Test to see if this BitmapImage has been cached to disk, and if not,
        /// cache it
        /// </summary>
        /// <param name="img"></param>
        /// <param name="thumbnailPath"></param>
        public static void CacheIfNotCached(BitmapImage img, string thumbnailPath)
        {
            var filename = ImageSaver.GetCachedImageFilename(thumbnailPath);

            // If the file already exists or the in-built default image is being used then just return
            if (File.Exists(filename) || !thumbnailPath.StartsWith("http")) return;

            if (thumbnailPath.EndsWith(".png"))
            {
                ImageSaver.SaveToPng(img, filename);
            }
            else if (thumbnailPath.EndsWith(".jpg") || thumbnailPath.EndsWith(".jpeg") || thumbnailPath.Contains(".jpg?"))
            {
                ImageSaver.SaveToJpg(img, filename);
            }
            else if (thumbnailPath.EndsWith(".bmp"))
            {
                ImageSaver.SaveToBmp(img, filename);
            }
        }

        /// <summary>
        /// Purge the cacheDir Directory of files that are older than age
        /// </summary>
        /// <param name="cacheDir"></param>
        /// <param name="age"></param>
        /// <returns>numFilesDeleted</returns>
        public static int PurgeCache(string cacheDir, TimeSpan age)
        {
            DirectoryInfo dir = new DirectoryInfo(cacheDir);
            var files = dir.EnumerateFiles().Where(f=>DateTime.Now.Subtract(f.LastWriteTime) > age);
            var numFilesDeleted = files.Count();
            foreach (var file in files)
            {
                try
                {
                    file.Delete();
                }
                catch(Exception){ }
            }

            //Log.WriteLog($"Image cache files deleted {numFilesDeleted}","ImagePurgeLog.txt");
            return numFilesDeleted;
        }
    }
}
