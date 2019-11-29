using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ReEngine
{
    public class StaticObjects
    {
        static Hashtable ServeStaticFileCache = new Hashtable();
        private static readonly object objLockServeStaticFile=new object();
        private static FileSystemWatcher m_Watcher;
        private static object objWatcherLock =new object();
        static Hashtable objWatchers = new Hashtable();
        public static async Task GetStaticFileInApp(HttpContext context, string fileName)
        {
           
            if (ServeStaticFileCache[fileName] == null)
            {
                lock (objLockServeStaticFile)
                {
                    if (ServeStaticFileCache[fileName] == null)
                    {
                        if (!File.Exists(fileName))
                        {
                            

                            ServeStaticFileCache[fileName] = new StatictFileCacheInfo()
                            {
                                Buffer = UnicodeEncoding.UTF8.GetBytes("File not found"),
                                MimeType = "text/html"
                            };
                        }
                        else
                        {

                            var file = new FileInfo(fileName);

                            if (objWatchers[file.Directory.FullName] == null)
                            {
                                lock (objWatcherLock)
                                {
                                    if (objWatchers[file.Directory.FullName] == null)
                                    {
                                        m_Watcher = new System.IO.FileSystemWatcher();
                                        m_Watcher.Path = file.Directory.FullName;
                                        m_Watcher.NotifyFilter = NotifyFilters.LastAccess |
                                                                 NotifyFilters.LastWrite |
                                                                 NotifyFilters.FileName |
                                                                 NotifyFilters.DirectoryName;
                                        m_Watcher.EnableRaisingEvents = true;
                                        m_Watcher.Filter = "*.*";
                                        m_Watcher.Changed += (sender, evt) =>
                                        {
                                            ServeStaticFileCache.Clear();
                                        };
                                        objWatchers[file.Directory.FullName] = m_Watcher;
                                    }
                                }
                            }
                            var buffer = File.ReadAllBytes(file.FullName);
                            var mimeType = ReEngine.ScriprLoader.GetMimeType(file.Extension);
                            ServeStaticFileCache[fileName] = new StatictFileCacheInfo
                            {
                                MimeType = mimeType,
                                Buffer = File.ReadAllBytes(file.FullName)
                            };
                        }
                    }
                }
            }
            StatictFileCacheInfo info = ServeStaticFileCache[fileName] as StatictFileCacheInfo;
            context.Response.StatusCode =200;
            context.Response.ContentType = info.MimeType;

            context.Response.ContentLength = info.Buffer.Length;

            using (var stream = context.Response.Body)
            {
                await stream.WriteAsync(info.Buffer, 0, info.Buffer.Length);
                await stream.FlushAsync();
            }
        }
    }
}
