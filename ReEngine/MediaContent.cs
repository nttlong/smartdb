using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ReEngine
{
    /// <summary>
    /// Return media content
    /// </summary>
    public class MediaContent
    {
        /// <summary>
        /// Byte of content
        /// </summary>
        public byte[] Data { get; set; }
        /// <summary>
        /// Mime type
        /// </summary>
        public string MimeType { get; set; }
        /// <summary>
        /// Get mime type form file path
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetMimeTypeFromFilePath(string fileName)
        {
            return ReEngine.ScriprLoader.GetMimeType(Path.GetExtension(fileName));
        }
    }
    /// <summary>
    /// Media file
    /// </summary>
    public class MediaFile
    {
        /// <summary>
        /// Absolute physical path
        /// </summary>
        public string FileName { get; set; }
    }
    /// <summary>
    /// Redirect action
    /// </summary>
    public class Redirect
    {
        /// <summary>
        /// Url to redirect
        /// </summary>
        public string Url { get; set; }
    }
    /// <summary>
    /// Download a big file
    /// </summary>
    public class AsyncMediaContent
    {
        /// <summary>
        /// Streaming must be acsyn get
        /// </summary>
        public Task<Stream> AysncStream { get; set; }
        public string MimeType { get; set; }
        /// <summary>
        /// Will be download If this proptery is not null 
        /// </summary>
        public string FileName { get; set; }
    }
}
