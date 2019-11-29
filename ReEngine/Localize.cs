using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace ReEngine
{
    /// <summary>
    /// Manage language resource and localize
    /// </summary>
    public static class Localize
    {
        /// <summary>
        /// The Key of lamguage code store in HttpSession
        /// </summary>
        public const string LANGUAGECODE_KEY = "ReEngine.Localize.LanguageCode";
        static Hashtable Cache = new Hashtable();
        static Hashtable CacheFiles = new Hashtable();
        private static readonly object objLockWriteFile = new object();
        private static readonly object objSyncLang = new object();
        private static readonly object objCacheLangItem = new object();
        public static List<LocalizeInfo> LangItems = new List<LocalizeInfo>();
        static Hashtable cacheLangItem = new Hashtable();
        static clsLanguageCachData LanguageCachData = null;
        private readonly static object objLanguageCachDataLock = new object();
        private static readonly object objLockContextLangCache = new object();

        static Hashtable ContextLangCache = new Hashtable();
        static FileSystemWatcher fw = null;


        /// <summary>
        /// config.xml->web-application/apps-directory
        /// Đọc cấu hình file 
        /// </summary>

        /// <summary>
        /// Lấy đường dẫn đến file ngôn ngữ global
        /// <para>
        /// Get full physical path of global i18n
        /// </para>
        /// </summary>
        /// <param name="LangCode"></param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough()]
        public static string I18nGlobalFilePath(string LangCode)
        {
            return string.Join(Path.DirectorySeparatorChar, ReEngine.WebHost.Get().AppsDirectory, "i18n", LangCode + ".json");
        }
        [System.Diagnostics.DebuggerStepThrough()]
        public static string I18nGetLanguageSupportFilePath()
        {
            return string.Join(Path.DirectorySeparatorChar, ReEngine.WebHost.Get().AppsDirectory, "i18n", "supports.json");
        }
        public static void CacheFile(string url, string FilePath)
        {
            Cache[url] = FilePath;
        }
        public static Hashtable GetI18nGlobal(string LanguagCode)
        {
            return ReEngine.JSON.FromFile<Hashtable>(I18nGlobalFilePath(LanguagCode));
        }
        public static void GetItems(LocalizeInfo[] items)
        {
            items = items.Where(x => !LangItems.Where(p=>p!=null).Any(p => string.Equals(p.AppName, x.AppName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(p.Key, x.Key, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(p.LangCode, x.LangCode, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(p.PageId, x.PageId, StringComparison.OrdinalIgnoreCase))).ToArray();


            if (items.Length == 0)
            {
                return;
            }
            foreach (var langCode in GetLanguageSupports().Select(p => p.Code))
            {
                foreach (var item in items)
                {
                    var key = $"lang={langCode};app={item.AppName};page={item.PageId};key={item.Key}";
                    if (cacheLangItem[key] == null)
                    {
                        lock (objCacheLangItem)
                        {
                            if (cacheLangItem[key] == null)
                            {
                                var filePath = "";
                                if (item.AppName == "-")
                                {
                                    filePath = string.Join(Path.DirectorySeparatorChar, ReEngine.Config.RootDir,
                                        ReEngine.WebHost.Get().AppsDirectory.Replace('/',Path.DirectorySeparatorChar), 
                                        "i18n", langCode + ".json");
                                }
                                else if (item.PageId == "-")
                                {
                                    filePath = string.Join(
                                        Path.DirectorySeparatorChar, 
                                        ReEngine.Config.RootDir, 
                                        ReEngine.WebHost.Get().AppsDirectory.Replace('/', Path.DirectorySeparatorChar), 
                                        item.AppName, 
                                        "i18n", 
                                        langCode + ".json");
                                }
                                else
                                {
                                    filePath = string.Join(
                                        Path.DirectorySeparatorChar,
                                        ReEngine.Config.RootDir, 
                                        ReEngine.WebHost.Get().AppsDirectory.Replace('/', Path.DirectorySeparatorChar),
                                        item.AppName, item.PageId.Replace('/', Path.DirectorySeparatorChar) + "." + langCode + ".json");

                                }
                                SaveToJsonFile((new ResItem[] { new ResItem {
                                Key=item.Key.ToLower(),
                                Value=item.Value
                            } }).ToList(), filePath);
                                cacheLangItem[key] = item;
                            }
                        }
                    }
                }
            }
            LangItems.AddRange(items);

        }

        private static SaveToJsonFileResult SaveToJsonFile(List<ResItem> gResItems, string globalFilePath)
        {
            if (File.Exists(globalFilePath))
            {
                try
                {
                    var content = File.ReadAllText(globalFilePath);
                    if (content.IndexOf("\r\n") > -1)
                    {
                        content = content.Replace("\r\n", "");
                    }

                    var existGResData = Newtonsoft.Json.JsonConvert.DeserializeObject<SortedDictionary<string, LocalizeTranslate>>(content);
                    List<ResItem> existGResItem = new List<ResItem>();
                    var hashChange = false;
                    foreach (var item in gResItems)
                    {
                        if (existGResData.Count(p => string.Equals(p.Key, item.Key, StringComparison.OrdinalIgnoreCase)) == 0)
                        {
                            hashChange = true;
                            existGResData.Add(item.Key.ToLower(), new LocalizeTranslate
                            {
                                Default = item.Value,
                                Translate = item.Value
                            });
                        }
                    }


                    if (hashChange)
                    {
                        content = Newtonsoft.Json.JsonConvert.SerializeObject(existGResData);
                        File.WriteAllText(globalFilePath, content);
                    }
                    return new SaveToJsonFileResult
                    {
                        Content = content,
                        Data = existGResData,
                        Items = existGResData.Select(p => new ResItem
                        {
                            Key = p.Key,
                            Value = p.Value.Translate
                        }).ToList(),
                        ModifiedTime = DateTime.UtcNow
                    };
                }
                catch (Exception ex)
                {

                    throw (ex);
                }
            }
            else
            {
                var saveDict = new SortedDictionary<string, LocalizeTranslate>();
                var saveLst = gResItems.ToList();
                saveLst.OrderBy(p => p.Key).ToList().ForEach(p =>
                        {
                            saveDict[p.Key] = new LocalizeTranslate()
                            {
                                Default = p.Value,
                                Translate = p.Value
                            };
                            //saveDict[p.Key].Defaut = p.Value;
                            //saveDict[p.Key].Translate = p.Value;
                        });
                var content = Newtonsoft.Json.JsonConvert.SerializeObject(saveDict);
                var relPath = Path.GetRelativePath(ReEngine.Config.RootDir, globalFilePath);
                var relPathPecies = relPath.Split(Path.DirectorySeparatorChar);
                var dirName = ReEngine.Config.RootDir;
                for (var i = 0; i < relPathPecies.Length - 1; i++)
                {
                    dirName = string.Join(Path.DirectorySeparatorChar, dirName, relPathPecies[i]);
                    if (!Directory.Exists(dirName))
                    {
                        Directory.CreateDirectory(dirName);
                    }
                }
                using (StreamWriter sw = File.CreateText(globalFilePath))
                {
                    sw.Write(content);
                }
                return new SaveToJsonFileResult
                {
                    Content = content,
                    Data = saveDict,
                    Items = saveLst.OrderBy(p => p.Key).ToList(),
                    ModifiedTime = DateTime.UtcNow
                };
            }
        }
        /// <summary>
        /// Set language code to session with key is in LANGUAGECODE_KEY
        /// <seealso cref="ReEngine.Localize.LANGUAGECODE_KEY"/>
        /// </summary>
        /// <param name="HttpContext"></param>
        /// <param name="LanguageCode"></param>
        public static void SetLanguageCode(string LanguageCode)
        {
            HttpUtils.SetSessionValue(LANGUAGECODE_KEY, LanguageCode);
        }


        /// <summary>
        /// Get language code in Http Session
        /// <para>
        /// if session is new default language code in config.xml at web-application/default-localize/language will be return;
        /// </para>
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetLanguageCode()
        {
            string defualtLanguage = ReEngine.WebHost.Get().LocalizeDefault.Language;
            return HttpUtils.GetSessionValue<string>(LANGUAGECODE_KEY, defualtLanguage);

        }
        /// <summary>
        /// Get language support
        /// </summary>
        /// <returns></returns>
        public static LanguageSupportItem[] GetLanguageSupports()
        {
            var ret = ReEngine.JSON.FromFile<LanguageSupportItem[]>(I18nGetLanguageSupportFilePath());
            return ret;
        }
        public static string Res(string Value)
        {
            if (fw == null)
            {
                lock (objLockContextLangCache)
                {
                    if (fw == null)
                    {
                        fw = new FileSystemWatcher();
                        fw.NotifyFilter = NotifyFilters.CreationTime |
                                        NotifyFilters.DirectoryName |
                                        NotifyFilters.FileName |
                                        NotifyFilters.LastWrite;
                        fw.Filter = "*.json";
                        fw.IncludeSubdirectories = true;
                        fw.Path = string.Join(ReEngine.Config.RootDir, ReEngine.WebHost.Get().AppsDirectory);
                        fw.EnableRaisingEvents = true;
                        fw.Changed += (S, E) =>
                        {
                            var f = E.FullPath;
                            ContextLangCache.Clear();
                            LanguageCachData.Data.Clear();
                        };
                    }
                }
            }
            
            var key = Value.TrimStart(' ').TrimEnd(' ').Replace("  ", " ").ToLower();
            var pagePath = HttpUtils.HttpContext.Request.Path.Value.ToLower();
            var languageCode = HttpUtils.GetLanguageCode();
            if (ContextLangCache[pagePath] == null)
            {
                lock (objLockContextLangCache)
                {
                    if (ContextLangCache[pagePath] == null)
                    {
                        ContextLangCache[pagePath] = new Hashtable();
                    }
                }
            }
            if (((Hashtable)ContextLangCache[pagePath])[languageCode] == null)
            {
                lock (objLockContextLangCache)
                {
                    if (((Hashtable)ContextLangCache[pagePath])[languageCode] == null)
                    {
                        ((Hashtable)ContextLangCache[pagePath])[languageCode] = new Hashtable();
                    }
                }
            }
            if (((Hashtable)((Hashtable)ContextLangCache[pagePath])[languageCode])[key] == null)
            {
                lock (objLockContextLangCache)
                {
                    if (((Hashtable)((Hashtable)ContextLangCache[pagePath])[languageCode])[key] == null)
                    {
                        if (LanguageCachData == null)
                        {
                            lock (objLanguageCachDataLock)
                            {
                                if (LanguageCachData == null)
                                {
                                    LanguageCachData = new clsLanguageCachData();
                                    LanguageCachData.Data = new Hashtable();
                                }

                            }
                        }
                        if (TemplateEngine.CacheTemplate[HttpUtils.HttpContext.Request.Path.Value] is HandlerRequest)
                        {
                            HandlerRequest h = (HandlerRequest)TemplateEngine.CacheTemplate[HttpUtils.HttpContext.Request.Path.Value.ToLower()];
                            if (File.Exists(h.FileName + "." + HttpUtils.GetLanguageCode() + ".json"))
                            {

                                var content = File.ReadAllText(h.FileName + "." + HttpUtils.GetLanguageCode() + ".json");
                                if (content.IndexOf("\r\n") > -1)
                                {
                                    content = content.Replace("\r\n", "");
                                }

                                var existGResData = Newtonsoft.Json.JsonConvert.DeserializeObject<SortedDictionary<string, LocalizeTranslate>>(content);
                                if (existGResData.Count(p => string.Equals(p.Value.Default, Value, StringComparison.OrdinalIgnoreCase)) > 0)
                                {
                                    var item = existGResData.FirstOrDefault(p => string.Equals(p.Value.Default, Value, StringComparison.OrdinalIgnoreCase));
                                    ((Hashtable)((Hashtable)ContextLangCache[pagePath])[languageCode])[key] = item.Value.GetTranslateWithEscape();
                                    return ((Hashtable)((Hashtable)ContextLangCache[pagePath])[languageCode])[key].ToString();
                                }
                            }
                            else
                            {
                                ((Hashtable)((Hashtable)ContextLangCache[pagePath])[languageCode])[key] = Value;
                            }
                        }
                        if (LanguageCachData.Data[ReEngine.HttpUtils.GetLanguageCode()] == null)
                        {
                            lock (objLanguageCachDataLock)
                            {
                                if (LanguageCachData.Data[ReEngine.HttpUtils.GetLanguageCode()] == null)
                                {
                                    LanguageCachData.Data[ReEngine.HttpUtils.GetLanguageCode()] = new clsHashLanguageData();
                                }
                            }

                        }
                        var data = ((clsHashLanguageData)LanguageCachData.Data[ReEngine.HttpUtils.GetLanguageCode()]);
                        if (data.Global == null)
                        {
                            lock (objLanguageCachDataLock)
                            {
                                if (data.Global == null)
                                {
                                    data.Global = new Hashtable();
                                    var GlobalData = ReEngine.JSON.FromFile<SortedDictionary<string, LocalizeTranslate>>(
                                        string.Join(Path.DirectorySeparatorChar,
                                        ReEngine.WebHost.Get().AppsDirectory, "i18n",
                                        ReEngine.HttpUtils.GetLanguageCode() + ".json"
                                    ));
                                    foreach (var item in GlobalData)
                                    {
                                        data.Global[item.Value.Default.TrimStart(' ').TrimEnd(' ').Replace("  ", " ").ToLower()] = item.Value.GetTranslateWithEscape();
                                    }
                                }

                            }
                        }
                        if (data.App == null)
                        {
                            lock (objLanguageCachDataLock)
                            {
                                if (data.App == null)
                                {
                                    var appI18nPath = string.Join(Path.DirectorySeparatorChar,
                                        ReEngine.WebHost.Get().AppsDirectory,
                                        ReEngine.HttpUtils.GetApp().Dir,
                                        "i18n",
                                        ReEngine.HttpUtils.GetLanguageCode() + ".json");
                                    if (File.Exists(appI18nPath))
                                    {
                                        data.App = new Hashtable();
                                        var AppData = ReEngine.JSON.FromFile<SortedDictionary<string, LocalizeTranslate>>(
                                            appI18nPath
                                        );
                                        foreach (var item in AppData)
                                        {
                                            data.App[item.Value.Default.TrimStart(' ').TrimEnd(' ').Replace("  ", " ").ToLower()] = item.Value.GetTranslateWithEscape();
                                        }
                                    }
                                    else
                                    {
                                        data.App = new Hashtable();
                                    }
                                }


                            }
                        }
                        if (data.App[Value.TrimStart(' ').TrimEnd(' ').Replace("  ", " ").ToLower()] != null)
                        {
                            ((Hashtable)((Hashtable)ContextLangCache[pagePath])[languageCode])[key] = data.App[Value.TrimStart(' ').TrimEnd(' ').Replace("  ", " ").ToLower()].ToString();
                        }
                        else if (data.Global[Value.TrimStart(' ').TrimEnd(' ').Replace("  ", " ").ToLower()] != null)
                        {
                            ((Hashtable)((Hashtable)ContextLangCache[pagePath])[languageCode])[key] = data.Global[Value.TrimStart(' ').TrimEnd(' ').Replace("  ", " ").ToLower()].ToString();
                        }
                        else
                        {

                            ((Hashtable)((Hashtable)ContextLangCache[pagePath])[languageCode])[key] =LocalizeTranslate.EscapeContent(Value);
                        }
                    }
                }
            }
            return ((Hashtable)((Hashtable)ContextLangCache[pagePath])[languageCode])[key].ToString();
        }
        public static IHtmlContent ResRawText(string value)
        {
            return new HtmlString(Res(value));
        }

    }
    public class ResItem
    {
        public string Key;
        public string Value;
    }
    public class LocalizeInfo
    {
        public string LangCode;

        public string AppName;

        public string Key { get; set; }
        public string Value { get; set; }
        public string PageId { get; set; }

    }
    internal class LoclizeResObject
    {
        public Hashtable Resource;
    }
    public class LanguageSupportItem
    {
        public string Code;
        public string Name;
        public string NativeName;
        public string Description;
    }
}
