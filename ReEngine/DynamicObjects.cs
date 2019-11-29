using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace ReEngine
{
    public class DynamicObjects
    {
        static Hashtable ServeStaticFileCache = new Hashtable();
        private static readonly object objLockServeStaticFile = new object();
        private static FileSystemWatcher m_Watcher;
        private static object objWatcherLock = new object();
        public static DynamicTemplateInfo GetInfo(ApplicationInfo app, HttpContext context, string fileName)
        {
            if (m_Watcher == null)
            {
                if (m_Watcher == null)
                {
                    lock (objWatcherLock)
                    {
                        if (m_Watcher == null)
                        {
                            m_Watcher = new System.IO.FileSystemWatcher();
                            var file = new FileInfo(fileName);
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
                        }
                    }
                }
            }
            if (ServeStaticFileCache[fileName] == null)
            {
                lock (objLockServeStaticFile)
                {
                    if (ServeStaticFileCache[fileName] == null)
                    {
                        if (!File.Exists(fileName))
                        {
                            ServeStaticFileCache[fileName] = false;
                        }
                        else
                        {
                            var tmp = new DynamicTemplateInfo();
                            if (File.Exists(fileName + ".csx"))
                            {
                                var relPath = Path.GetRelativePath(ReEngine.WebHost.Get().AppsDirectory, fileName);
                                var methodInfo = ReEngine.ScriprLoader.LoadMethod(relPath + ".csx", "GetModel");
                                tmp.InstanceType = methodInfo.Method.ReflectedType;
                                tmp.GetModelMethod = methodInfo.Method;
                            }
                            tmp.Content = File.ReadAllText(fileName);
                          
                            ServeStaticFileCache[fileName] = tmp;
                        }
                    }
                }
            }
            return ServeStaticFileCache[fileName] as DynamicTemplateInfo;
        }
    }
}
