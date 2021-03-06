﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using NYoutubeDL;
using NYoutubeDL.Models;
using Util.IO;
using VideoLibrary;
using YoutubeSearch;

namespace StreamModule
{
    /// <summary>
    /// Manages operations on youtube videos
    /// </summary>
    public static class YTManager
    {
        private const string imageUrl = "https://img.youtube.com/vi/";

        private const string thumbnailQuality = "/sddefault.jpg";

        /// <summary>
        /// deletes all cached videos
        /// </summary>
        public static void clearVideoCache(string whiteList = "")
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" +
                            DataManagement.PersistentData.defaultFolderName + @"\" + DataManagement.PersistentData.videoCacheFolder;

            if (Directory.Exists(folder))
            {
                foreach (var file in Directory.GetFiles(folder))
                {
                    if (file != whiteList)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// returns id of youtube video from given url
        /// </summary>
        /// <param name="url">full url to youtube video</param>
        /// <remarks>fallback for SearchQueryTaskAsync</remarks>
        /// <seealso cref="VideoSearch.SearchQueryTaskAsync"/>
        /// <returns>null if no url was entered</returns>
        public static string getIdFromUrl(string url)
        {
            if ((url.Contains("youtube") || url.Contains("youtu.be")) && url.Contains("https://") || url.Contains("http://"))
            {
                const string delimiter = "watch?v=";
                const string shortDelimiter = ".be/";

                if(url.Contains(delimiter))
                    return url.Substring(url.LastIndexOf(delimiter) + delimiter.Length);
                else if (url.Contains(shortDelimiter))
                    return url.Substring(url.LastIndexOf(shortDelimiter) + shortDelimiter.Length);
            }

            return null;         
        }

        /// <summary>
        /// get url to Thumbnail from a given video url
        /// </summary>
        /// <param name="url">Url to yt video</param>
        /// <returns>direct url to thumbnail</returns>
        public static string getUrlToThumbnail(string url)
        {
            string id;
            if((id = getIdFromUrl(url)) != null){
                return imageUrl + id + thumbnailQuality;
            }

            return "";
        }

        /// <summary>
        /// get the title from an url
        /// </summary>
        /// <param name="url">url to video</param>
        /// <remarks>fallback for SearchQueryTaskAsync</remarks>
        /// <seealso cref="VideoSearch.SearchQueryTaskAsync"/>
        /// <returns>task string representing the title</returns>
        public static async Task<string> GetTitleAsync(string url)
        {
            var api = "https://noembed.com/embed?url=" + url;
            return GetArgs(await new WebClient().DownloadStringTaskAsync(api), "title");
        }


        private static string GetArgs(string args, string key)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(args);
            string ret;

            if (dict.TryGetValue(key, out ret))
            {
                return ret;
            }
            else
            {
                return "";
            }
        }


        /// <summary>
        /// Gets general information about non-YT video
        /// </summary>
        /// <param name="url">url to video</param>
        /// <returns>Title, Thumbnail-Uri, duration</returns>
        public static async Task<DownloadInfo> GetGeneralInfoAsync(string url)
        {
            NYoutubeDL.YoutubeDL dl = new YoutubeDL();
            dl.VideoUrl = url;

            dl.RetrieveAllInfo = true;
            await dl.PrepareDownloadAsync();
            
            return dl.Info;
        }

    }
}