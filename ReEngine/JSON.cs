using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReEngine
{
    public class JSON
    {
        private static readonly object objLockCache=new object();
        static Hashtable CacheContent = new Hashtable();
        static FileSystemWatcher w = null;
        public static T FromFile<T>(string FilePath)
        {
            string myJsonString = LoadFromCache(FilePath);
                
            var myJObject = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(myJsonString);
            return myJObject;
        }

        private static string LoadFromCache(string FilePath)
        {
            //File.ReadAllText(string.Join(Path.DirectorySeparatorChar,Config.RootDir,FilePath.Replace('/',Path.DirectorySeparatorChar)));
            var filePath = string.Join(Path.DirectorySeparatorChar, Config.RootDir, FilePath.Replace('/', Path.DirectorySeparatorChar));
            if (CacheContent[filePath] == null)
            {
                lock (objLockCache)
                {
                    if (CacheContent[filePath] == null)
                    {
                        if (w == null)
                        {
                            w = new FileSystemWatcher();
                            var directory = (new FileInfo(filePath)).Directory.FullName;
                            if (Directory.Exists(directory))
                            {
                                w.Path = directory;
                                w.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
                                w.EnableRaisingEvents = true;
                                w.Changed += (s,e)=> {
                                    CacheContent.Clear();
                                };
                                
                            }
                        }
                        CacheContent[filePath]= File.ReadAllText(string.Join(Path.DirectorySeparatorChar, Config.RootDir, FilePath.Replace('/', Path.DirectorySeparatorChar)));
                    }
                }
            }
            return CacheContent[filePath].ToString();
        }

        public static string ToString(object Data)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(Data);
        }
    }
}
