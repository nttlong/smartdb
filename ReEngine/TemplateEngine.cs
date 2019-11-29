﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace ReEngine
{
    public static class TemplateEngine
    {
        internal static Hashtable CacheTemplate = new Hashtable();
        public static string CheckFile(string dir, string value)
        {
            string filePath = string.Join(
                Path.DirectorySeparatorChar,
                ReEngine.Config.RootDir,
                ReEngine.WebHost.Get().AppsDirectory,
                dir,
                value.TrimStart('/').TrimEnd('/'))
                .Replace('/', Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar);
            if (File.Exists(filePath))
            {
                ReEngine.Localize.CacheFile(value, filePath);
                return filePath;
            }
            else if (File.Exists(filePath + ".html"))
            {
                ReEngine.Localize.CacheFile(value, filePath + ".html");
                return filePath + ".html";
            }
            else if (File.Exists(filePath + ".cshtml"))
            {
                ReEngine.Localize.CacheFile(value, filePath + ".cshtml");
                return filePath + ".cshtml";
            }
            else
            {
                filePath = string.Join(
                    Path.DirectorySeparatorChar,
                    ReEngine.Config.RootDir,
                     ReEngine.WebHost.Get().AppsDirectory,
                    dir,
                    value.TrimStart('/').TrimEnd('/'),
                    "index.html").Replace('/', Path.DirectorySeparatorChar);
                ReEngine.Localize.CacheFile(value, filePath);
                if (File.Exists(filePath))
                {
                    return filePath;
                }
                else
                {
                    filePath = string.Join(
                    Path.DirectorySeparatorChar,
                    ReEngine.Config.RootDir,
                    ReEngine.WebHost.Get().AppsDirectory,
                    dir,
                    value.TrimStart('/').TrimEnd('/'),
                    "index.cshtml").Replace('/', Path.DirectorySeparatorChar);
                    if (File.Exists(filePath))
                    {
                        ReEngine.Localize.CacheFile(value, filePath);
                        return filePath;
                    }
                }
            }
            return null;
        }

        internal static void PushToCache(string path, HandlerRequest handlerRequest)
        {
            CacheTemplate[path.ToLower()] = handlerRequest;
        }
    }
    
}
