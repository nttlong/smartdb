using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace ReEngine
{
    public static class ScriprLoader
    {
        static Hashtable CachWatchFolder = new Hashtable();
        static Hashtable CachesAjaxPost = new Hashtable();
        static Hashtable CachesAjaxMethod = new Hashtable();
        static readonly object objLockCachesAjaxPost = new object();
        static Hashtable Caches = new Hashtable();
        static Hashtable CachesWathPath = new Hashtable();
        internal static Hashtable CacheMethods = new Hashtable();
        private static Assembly[] AllAsm;
        private static string _BaseAppDirectory;

        internal static void WatchHtmlView()
        {
            var log = log4net.LogManager.GetLogger(typeof(ScriprLoader));
            try
            {
                var Dir = Path.Join(BaseAppDirectory, "apps");
                var htmlFiles = Directory.EnumerateFiles(Dir, "*.html", SearchOption.AllDirectories);
                ScriprLoader.Views = new List<ViewInfo>();
                foreach (var file in htmlFiles)
                {
                    var view = ScriprLoader.GetViewFormFile(file);
                    if (view != null)
                    {
                        ScriprLoader.Views.Add(view);
                    }
                }
                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = Dir;
                watcher.NotifyFilter = NotifyFilters.LastWrite |
                    NotifyFilters.CreationTime;

                watcher.Filter = "*.html";
                watcher.IncludeSubdirectories = true;
                watcher.Changed += new FileSystemEventHandler(WathcHtmlViewInApps);
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                log.Debug(ex);
                
            }
        }

        public static void RunScript(string ScriptPath)
        {
            var log = log4net.LogManager.GetLogger(typeof(ScriprLoader));
            var mdl=ReEngine.ScriprLoader.LoadMethod(ScriptPath + ".csx", "Start",(error,context)=> {
                if (error != null)
                {
                    log.Debug(error);
                }
                else
                {
                    try
                    {
                        context.Method.Invoke(null, new object[] { });
                    }
                    catch (Exception ex)
                    {
                        log.Debug(ex);
                    }
                }
            });
            mdl.Method.Invoke(null, new object[] { });
        }

        private static void WathcHtmlViewInApps(object sender, FileSystemEventArgs e)
        {
            ViewInfo view = ScriprLoader.GetViewFormFile(e.FullPath);
            if (view != null)
            {
                var mView = ScriprLoader.Views.FirstOrDefault(p => string.Equals(p.ViewPath, view.ViewPath, StringComparison.OrdinalIgnoreCase));
                if (mView != null)
                {
                    mView.Id = view.Id;
                    mView.Name = view.Name;
                    mView.Description = view.Description;

                }
                else
                {
                    ScriprLoader.Views.Add(view);
                }
            }
        }

        private static ViewInfo GetViewFormFile(string fullPath)
        {
            var rPath = Path.GetRelativePath(BaseAppDirectory, fullPath);
            var appName = rPath.Split(Path.DirectorySeparatorChar)[1];
            var description = "";
            var viewName = "";
            string readText = File.ReadAllText(fullPath);
            var indexOfViewId = readText.IndexOf(@"view-id=""");
            if (indexOfViewId == -1) return null;
            indexOfViewId += @"view-id=""".Length;
            var endIndexOfView = readText.IndexOf('"', indexOfViewId);
            if (endIndexOfView == -1)
            {
                return null;
            }
            var viewId = readText.Substring(indexOfViewId, endIndexOfView - indexOfViewId);
            var indexOfViewName = readText.IndexOf(@"view-name=""");
            if (indexOfViewName == -1)
            {
                return null;
            }
            indexOfViewName += @"view-name=""".Length;
            var endIndexOfViewName = readText.IndexOf('"', indexOfViewName);
            viewName = readText.Substring(indexOfViewName, endIndexOfViewName - indexOfViewName);
            var indexOfViewDescription = readText.IndexOf(@"view-description=""");
            if (indexOfViewDescription > -1)
            {
                indexOfViewDescription += @"view-description=""".Length;
                var endIndexOfViewDescription = readText.IndexOf('"', indexOfViewDescription);
                description = readText.Substring(indexOfViewDescription, endIndexOfViewDescription - indexOfViewDescription);
            }
            return new ViewInfo
            {
                Id = viewId,
                Name = viewName,
                Description = description,
                AppName = appName,
                ViewPath = rPath.Replace("\\","/").Substring(("apps/" + appName + "/pages").Length, rPath.Length - ("apps/" + appName + "/pages").Length).TrimStart('/')
            };

        }

        public static object LanguageItemProvider { get; private set; }

        private static MethodInfo getLanguageItem;
        private static object lockGetLanguageItemMethod = new object();
        static readonly object objLock = new object();
        static readonly object objLockMethod = new object();

        public static string BaseAppDirectory
        {
            get
            {
                if (_BaseAppDirectory == null)
                {
                    AllAsm = Assembly
                    .GetEntryAssembly()
                    .GetReferencedAssemblies()
                    .Select(Assembly.Load).ToArray();
                    // FileStream fileStream = new FileStream(x, FileMode.Open);
                    _BaseAppDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var indexOfBin = BaseAppDirectory.IndexOf(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar);
                    if (indexOfBin > -1)
                    {
                        _BaseAppDirectory = _BaseAppDirectory.Substring(0, indexOfBin);
                    }
                }
                return _BaseAppDirectory;
            }
        }

        public static List<ViewInfo> Views { get; private set; }

        public static GetMethodResult Load(string appDirectory, string path)
        {
            if (CacheMethods[appDirectory + "/" + path] == null)
            {
                lock (objLockMethod)
                {
                    if (CacheMethods[appDirectory + "/" + path] == null)
                    {
                        var typeName = path.Split("@")[1];
                        var methodName = path.Split("@")[0];
                        var rType = LoadScript(appDirectory, typeName, path);
                        var mt = rType.Type.GetMethods().FirstOrDefault(p => string.Equals(p.Name, methodName, StringComparison.OrdinalIgnoreCase));
                        CacheMethods[appDirectory + "/" + path] = new GetMethodResult
                        {
                            Method = mt,
                            Type = rType.Type,
                            Source = rType.Source,
                            TypeName = rType.TypeName,
                            AppDirectory = appDirectory,
                            SourcePath = path
                        };
                    }
                }
            }
            return CacheMethods[appDirectory + "/" + path] as GetMethodResult;
        }

        private static SourceTypeInfo LoadScript(string appName, string typeName, string path)
        {
            var ret = new SourceTypeInfo
            {

            };
            var pathToFile = Path.Join(appName, "ServerScripts", typeName);
            if (Caches[pathToFile.ToLower()] == null)
            {
                var dirs = typeName.Split(".");
                var subDirs = new List<string>();
                for (var i = 0; i < dirs.Length - 1; i++)
                {
                    subDirs.Add(dirs[i]);
                }
                var watchPath = Path.Join(appName, "ServerScripts");
                var loadPath = Path.Join(Path.Join(appName, "ServerScripts", String.Join(Path.DirectorySeparatorChar, subDirs.ToArray())), dirs[dirs.Length - 1]);
                ret.Source = loadPath;
                lock (objLock)
                {
                    if (Caches[pathToFile.ToLower()] == null)
                    {

                        var className = dirs[dirs.Length - 1];
                        var curenrDirIndex = AppDomain.CurrentDomain.BaseDirectory.IndexOf(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar);
                        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
                        if (curenrDirIndex > -1)
                        {
                            currentDir = AppDomain.CurrentDomain.BaseDirectory.Substring(0, curenrDirIndex);
                        }

                        if (CachesWathPath[watchPath] == null)
                        {
                            CachesWathPath[watchPath] = true;
                            watchFile(Path.Join(appName, "ServerScripts"));
                        }
                        var x = loadPath + ".csx";


                        using (StreamReader reader = new StreamReader(x))
                        {

                            string line = reader.ReadToEnd();
                            var isHasMainClass = line.IndexOf("public static class Main\r\n") > -1;
                            if (isHasMainClass)
                            {
                                line += Environment.NewLine + "return typeof(Main);";
                            }
                            else
                            {
                                line += Environment.NewLine + "return typeof(" + className.ToLower() + ");";
                            }
                            var result = CSharpScript.EvaluateAsync(line, ScriptOptions.Default
                                .WithFilePath(x));
                            //.AddReferences(all));
                            Caches[pathToFile.ToLower()] = new ScriptingInfo
                            {
                                FullPath = x,
                                RetType = result.Result as Type,
                                CachePath = appName + "/" + path
                            };
                        }
                    }
                }
            }
            ret.Type = ((ScriptingInfo)Caches[pathToFile.ToLower()]).RetType;
            ret.TypeName = typeName;
            return ret;
        }
        /// <summary>
        /// Load script source and watch change file
        /// </summary>
        /// <param name="path">Relative path to source code from root folder</param>
        /// <param name="method">Method name</param>
        /// <returns></returns>
        /// <exception cref="Errors.ScriptSourceWasNotFound"/>
        /// <exception cref="Errors.ScriptSourceError"/>
        public static DynamicMethodInfo LoadMethod(string path, string method,Action<Exception,DynamicMethodInfo> OnChange=null)
        {
            DynamicMethodInfo retInfo = null; ;
            if (CachesAjaxPost[path.ToLower()] == null)
            {
                lock (objLockCachesAjaxPost)
                {
                    if (CachesAjaxPost[path.ToLower()] == null)
                    {
                        var folderPath = Path.GetDirectoryName(string.Join(Path.DirectorySeparatorChar, BaseAppDirectory, path));
                        if (Directory.Exists(folderPath))
                        {
                            if (CachWatchFolder[folderPath.ToLower()] == null)
                            {
                                FileSystemWatcher watcher = new FileSystemWatcher();
                                watcher.Path = folderPath;
                                watcher.NotifyFilter = NotifyFilters.LastWrite;
                                watcher.Filter = "*.csx~*";
                                watcher.IncludeSubdirectories = true;
                                watcher.Changed += Watcher_Changed_File_Change;

                                watcher.EnableRaisingEvents = true;
                                CachWatchFolder[folderPath.ToLower()] = watcher;
                                if (OnChange!=null)
                                {
                                    watcher.Changed += (S, E) =>
                                    {
                                        try
                                        {
                                            var ret = LoadMethod(path, method);
                                            OnChange(null,ret);
                                        }
                                        catch (Exception ex)
                                        {

                                            OnChange(ex, null);
                                        }
                                        
                                    };
                                }
                            }
                        }
                        var className = Path.GetFileNameWithoutExtension(path);
                        if(!File.Exists(Path.Join(BaseAppDirectory, path))){
                            throw (new Errors.ScriptSourceWasNotFound(string.Format("'{0}' was not found", Path.Join(BaseAppDirectory, path))));
                        }
                        using (StreamReader reader = new StreamReader(Path.Join(BaseAppDirectory, path)))
                        {

                            string line = reader.ReadToEnd();
                            var isHasMainClass = line.IndexOf("public static class Main\r\n") > -1;
                            if (isHasMainClass)
                            {
                                line += Environment.NewLine + "return typeof(Main);";
                            }
                            else
                            {
                                line += Environment.NewLine + "return typeof(" + className.ToLower() + ");";
                            }

                            try
                            {


                                


                                var result = CSharpScript.EvaluateAsync(line, ScriptOptions.Default
                                                        .WithFilePath(Path.Join(BaseAppDirectory, path))
                                                        .WithOptimizationLevel(OptimizationLevel.Debug));
                                var scriptor = new DynamicScriptInfo()
                                {
                                    Type = result.Result as Type,
                                };
                                //var membershipAttr = scriptor.Type.GetCustomAttribute<ReEngine.MembershipAttribute>();
                                //scriptor.MembershipType = membershipAttr;
                                CachesAjaxPost[path.ToLower()] = scriptor;
                            }
                            catch(CompilationErrorException ex)
                            {
                                throw (new Errors.ScriptSourceError(ex.Message));
                            }
                            catch (Exception ex)
                            {

                                throw(ex);
                            }

                        }
                    }
                }
            }
            if (CachesAjaxMethod[path.ToLower() + "/" + method.ToLower()] == null)
            {
                lock (objLockCachesAjaxPost)
                {
                    var ret = ((DynamicScriptInfo)CachesAjaxPost[path.ToLower()]).Type.GetMethods()
                        .FirstOrDefault(p => string.Equals(p.Name, method, StringComparison.OrdinalIgnoreCase));
                    CachesAjaxMethod[path.ToLower() + "/" + method.ToLower()] = new DynamicMethodInfo {
                        Method=ret,
                       // MembershipType= ((DynamicScriptInfo)CachesAjaxPost[path.ToLower()]).MembershipType
                    };
                }
            }
            retInfo= CachesAjaxMethod[path.ToLower() + "/" + method.ToLower()] as DynamicMethodInfo;
            return CachesAjaxMethod[path.ToLower() + "/" + method.ToLower()] as DynamicMethodInfo;
        }

        private static void Watcher_Changed_File_Change(object sender, FileSystemEventArgs e)
        {
            CachesAjaxPost.Clear();
            CachesAjaxMethod.Clear();
        }

        private static void watchFile(string Dir)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = Dir;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            //watcher.Filter = "*.csx";
            watcher.IncludeSubdirectories = true;
            watcher.Changed += new FileSystemEventHandler(Watcher_Changed);
            watcher.EnableRaisingEvents = true;

        }



        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Caches.Clear();
            CacheMethods.Clear();
        }

        public static object CreateContext(Type contextParamType, Context context)
        {
            if (contextParamType == typeof(Context))
            {
                return context;
            }
            var contextParam = contextParamType.Assembly.CreateInstance(contextParamType.FullName);
            contextParamType.GetProperty("AppName").SetValue(contextParam, context.AppName);
            contextParamType.GetProperty("PageId").SetValue(contextParam, context.PageId);
            contextParamType.GetProperty("Token").SetValue(contextParam, context.Token);
            contextParamType.GetProperty("Tenancy").SetValue(contextParam, context.Tenancy);
            contextParamType.GetProperty("HttpContext").SetValue(contextParam, context.HttpContext);
            contextParamType.GetProperty("Application").SetValue(contextParam, context.Application);
            return contextParam;
        }

        public static string GetContent(string apiPath)
        {
            return apiPath;
        }
        public static string GetMimeType(string extension)
        {
            if (extension == null)
                throw new ArgumentNullException("extension");

            if (extension.StartsWith("."))
                extension = extension.Substring(1);


            switch (extension.ToLower())
            {
                #region Big freaking list of mime types
                case "323": return "text/h323";
                case "3g2": return "video/3gpp2";
                case "3gp": return "video/3gpp";
                case "3gp2": return "video/3gpp2";
                case "3gpp": return "video/3gpp";
                case "7z": return "application/x-7z-compressed";
                case "aa": return "audio/audible";
                case "aac": return "audio/aac";
                case "aaf": return "application/octet-stream";
                case "aax": return "audio/vnd.audible.aax";
                case "ac3": return "audio/ac3";
                case "aca": return "application/octet-stream";
                case "accda": return "application/msaccess.addin";
                case "accdb": return "application/msaccess";
                case "accdc": return "application/msaccess.cab";
                case "accde": return "application/msaccess";
                case "accdr": return "application/msaccess.runtime";
                case "accdt": return "application/msaccess";
                case "accdw": return "application/msaccess.webapplication";
                case "accft": return "application/msaccess.ftemplate";
                case "acx": return "application/internet-property-stream";
                case "addin": return "text/xml";
                case "ade": return "application/msaccess";
                case "adobebridge": return "application/x-bridge-url";
                case "adp": return "application/msaccess";
                case "adt": return "audio/vnd.dlna.adts";
                case "adts": return "audio/aac";
                case "afm": return "application/octet-stream";
                case "ai": return "application/postscript";
                case "aif": return "audio/x-aiff";
                case "aifc": return "audio/aiff";
                case "aiff": return "audio/aiff";
                case "air": return "application/vnd.adobe.air-application-installer-package+zip";
                case "amc": return "application/x-mpeg";
                case "application": return "application/x-ms-application";
                case "art": return "image/x-jg";
                case "asa": return "application/xml";
                case "asax": return "application/xml";
                case "ascx": return "application/xml";
                case "asd": return "application/octet-stream";
                case "asf": return "video/x-ms-asf";
                case "ashx": return "application/xml";
                case "asi": return "application/octet-stream";
                case "asm": return "text/plain";
                case "asmx": return "application/xml";
                case "aspx": return "application/xml";
                case "asr": return "video/x-ms-asf";
                case "asx": return "video/x-ms-asf";
                case "atom": return "application/atom+xml";
                case "au": return "audio/basic";
                case "avi": return "video/x-msvideo";
                case "axs": return "application/olescript";
                case "bas": return "text/plain";
                case "bcpio": return "application/x-bcpio";
                case "bin": return "application/octet-stream";
                case "bmp": return "image/bmp";
                case "c": return "text/plain";
                case "cab": return "application/octet-stream";
                case "caf": return "audio/x-caf";
                case "calx": return "application/vnd.ms-office.calx";
                case "cat": return "application/vnd.ms-pki.seccat";
                case "cc": return "text/plain";
                case "cd": return "text/plain";
                case "cdda": return "audio/aiff";
                case "cdf": return "application/x-cdf";
                case "cer": return "application/x-x509-ca-cert";
                case "chm": return "application/octet-stream";
                case "class": return "application/x-java-applet";
                case "clp": return "application/x-msclip";
                case "cmx": return "image/x-cmx";
                case "cnf": return "text/plain";
                case "cod": return "image/cis-cod";
                case "config": return "application/xml";
                case "contact": return "text/x-ms-contact";
                case "coverage": return "application/xml";
                case "cpio": return "application/x-cpio";
                case "cpp": return "text/plain";
                case "crd": return "application/x-mscardfile";
                case "crl": return "application/pkix-crl";
                case "crt": return "application/x-x509-ca-cert";
                case "cs": return "text/plain";
                case "csdproj": return "text/plain";
                case "csh": return "application/x-csh";
                case "csproj": return "text/plain";
                case "css": return "text/css";
                case "csv": return "text/csv";
                case "cur": return "application/octet-stream";
                case "cxx": return "text/plain";
                case "dat": return "application/octet-stream";
                case "datasource": return "application/xml";
                case "dbproj": return "text/plain";
                case "dcr": return "application/x-director";
                case "def": return "text/plain";
                case "deploy": return "application/octet-stream";
                case "der": return "application/x-x509-ca-cert";
                case "dgml": return "application/xml";
                case "dib": return "image/bmp";
                case "dif": return "video/x-dv";
                case "dir": return "application/x-director";
                case "disco": return "text/xml";
                case "dll": return "application/x-msdownload";
                case "dll.config": return "text/xml";
                case "dlm": return "text/dlm";
                case "doc": return "application/msword";
                case "docm": return "application/vnd.ms-word.document.macroenabled.12";
                case "docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "dot": return "application/msword";
                case "dotm": return "application/vnd.ms-word.template.macroenabled.12";
                case "dotx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.template";
                case "dsp": return "application/octet-stream";
                case "dsw": return "text/plain";
                case "dtd": return "text/xml";
                case "dtsconfig": return "text/xml";
                case "dv": return "video/x-dv";
                case "dvi": return "application/x-dvi";
                case "dwf": return "drawing/x-dwf";
                case "dwp": return "application/octet-stream";
                case "dxr": return "application/x-director";
                case "eml": return "message/rfc822";
                case "emz": return "application/octet-stream";
                case "eot": return "application/octet-stream";
                case "eps": return "application/postscript";
                case "etl": return "application/etl";
                case "etx": return "text/x-setext";
                case "evy": return "application/envoy";
                case "exe": return "application/octet-stream";
                case "exe.config": return "text/xml";
                case "fdf": return "application/vnd.fdf";
                case "fif": return "application/fractals";
                case "filters": return "application/xml";
                case "fla": return "application/octet-stream";
                case "flr": return "x-world/x-vrml";
                case "flv": return "video/x-flv";
                case "fsscript": return "application/fsharp-script";
                case "fsx": return "application/fsharp-script";
                case "generictest": return "application/xml";
                case "gif": return "image/gif";
                case "group": return "text/x-ms-group";
                case "gsm": return "audio/x-gsm";
                case "gtar": return "application/x-gtar";
                case "gz": return "application/x-gzip";
                case "h": return "text/plain";
                case "hdf": return "application/x-hdf";
                case "hdml": return "text/x-hdml";
                case "hhc": return "application/x-oleobject";
                case "hhk": return "application/octet-stream";
                case "hhp": return "application/octet-stream";
                case "hlp": return "application/winhlp";
                case "hpp": return "text/plain";
                case "hqx": return "application/mac-binhex40";
                case "hta": return "application/hta";
                case "htc": return "text/x-component";
                case "htm": return "text/html";
                case "html": return "text/html";
                case "htt": return "text/webviewhtml";
                case "hxa": return "application/xml";
                case "hxc": return "application/xml";
                case "hxd": return "application/octet-stream";
                case "hxe": return "application/xml";
                case "hxf": return "application/xml";
                case "hxh": return "application/octet-stream";
                case "hxi": return "application/octet-stream";
                case "hxk": return "application/xml";
                case "hxq": return "application/octet-stream";
                case "hxr": return "application/octet-stream";
                case "hxs": return "application/octet-stream";
                case "hxt": return "text/html";
                case "hxv": return "application/xml";
                case "hxw": return "application/octet-stream";
                case "hxx": return "text/plain";
                case "i": return "text/plain";
                case "ico": return "image/x-icon";
                case "ics": return "application/octet-stream";
                case "idl": return "text/plain";
                case "ief": return "image/ief";
                case "iii": return "application/x-iphone";
                case "inc": return "text/plain";
                case "inf": return "application/octet-stream";
                case "inl": return "text/plain";
                case "ins": return "application/x-internet-signup";
                case "ipa": return "application/x-itunes-ipa";
                case "ipg": return "application/x-itunes-ipg";
                case "ipproj": return "text/plain";
                case "ipsw": return "application/x-itunes-ipsw";
                case "iqy": return "text/x-ms-iqy";
                case "isp": return "application/x-internet-signup";
                case "ite": return "application/x-itunes-ite";
                case "itlp": return "application/x-itunes-itlp";
                case "itms": return "application/x-itunes-itms";
                case "itpc": return "application/x-itunes-itpc";
                case "ivf": return "video/x-ivf";
                case "jar": return "application/java-archive";
                case "java": return "application/octet-stream";
                case "jck": return "application/liquidmotion";
                case "jcz": return "application/liquidmotion";
                case "jfif": return "image/pjpeg";
                case "jnlp": return "application/x-java-jnlp-file";
                case "jpb": return "application/octet-stream";
                case "jpe": return "image/jpeg";
                case "jpeg": return "image/jpeg";
                case "jpg": return "image/jpeg";
                case "js": return "application/x-javascript";
                case "jsx": return "text/jscript";
                case "jsxbin": return "text/plain";
                case "latex": return "application/x-latex";
                case "library-ms": return "application/windows-library+xml";
                case "lit": return "application/x-ms-reader";
                case "loadtest": return "application/xml";
                case "lpk": return "application/octet-stream";
                case "lsf": return "video/x-la-asf";
                case "lst": return "text/plain";
                case "lsx": return "video/x-la-asf";
                case "lzh": return "application/octet-stream";
                case "m13": return "application/x-msmediaview";
                case "m14": return "application/x-msmediaview";
                case "m1v": return "video/mpeg";
                case "m2t": return "video/vnd.dlna.mpeg-tts";
                case "m2ts": return "video/vnd.dlna.mpeg-tts";
                case "m2v": return "video/mpeg";
                case "m3u": return "audio/x-mpegurl";
                case "m3u8": return "audio/x-mpegurl";
                case "m4a": return "audio/m4a";
                case "m4b": return "audio/m4b";
                case "m4p": return "audio/m4p";
                case "m4r": return "audio/x-m4r";
                case "m4v": return "video/x-m4v";
                case "mac": return "image/x-macpaint";
                case "mak": return "text/plain";
                case "man": return "application/x-troff-man";
                case "manifest": return "application/x-ms-manifest";
                case "map": return "text/plain";
                case "master": return "application/xml";
                case "mda": return "application/msaccess";
                case "mdb": return "application/x-msaccess";
                case "mde": return "application/msaccess";
                case "mdp": return "application/octet-stream";
                case "me": return "application/x-troff-me";
                case "mfp": return "application/x-shockwave-flash";
                case "mht": return "message/rfc822";
                case "mhtml": return "message/rfc822";
                case "mid": return "audio/mid";
                case "midi": return "audio/mid";
                case "mix": return "application/octet-stream";
                case "mk": return "text/plain";
                case "mmf": return "application/x-smaf";
                case "mno": return "text/xml";
                case "mny": return "application/x-msmoney";
                case "mod": return "video/mpeg";
                case "mov": return "video/quicktime";
                case "movie": return "video/x-sgi-movie";
                case "mp2": return "video/mpeg";
                case "mp2v": return "video/mpeg";
                case "mp3": return "audio/mpeg";
                case "mp4": return "video/mp4";
                case "mp4v": return "video/mp4";
                case "mpa": return "video/mpeg";
                case "mpe": return "video/mpeg";
                case "mpeg": return "video/mpeg";
                case "mpf": return "application/vnd.ms-mediapackage";
                case "mpg": return "video/mpeg";
                case "mpp": return "application/vnd.ms-project";
                case "mpv2": return "video/mpeg";
                case "mqv": return "video/quicktime";
                case "ms": return "application/x-troff-ms";
                case "msi": return "application/octet-stream";
                case "mso": return "application/octet-stream";
                case "mts": return "video/vnd.dlna.mpeg-tts";
                case "mtx": return "application/xml";
                case "mvb": return "application/x-msmediaview";
                case "mvc": return "application/x-miva-compiled";
                case "mxp": return "application/x-mmxp";
                case "nc": return "application/x-netcdf";
                case "nsc": return "video/x-ms-asf";
                case "nws": return "message/rfc822";
                case "ocx": return "application/octet-stream";
                case "oda": return "application/oda";
                case "odc": return "text/x-ms-odc";
                case "odh": return "text/plain";
                case "odl": return "text/plain";
                case "odp": return "application/vnd.oasis.opendocument.presentation";
                case "ods": return "application/oleobject";
                case "odt": return "application/vnd.oasis.opendocument.text";
                case "one": return "application/onenote";
                case "onea": return "application/onenote";
                case "onepkg": return "application/onenote";
                case "onetmp": return "application/onenote";
                case "onetoc": return "application/onenote";
                case "onetoc2": return "application/onenote";
                case "orderedtest": return "application/xml";
                case "osdx": return "application/opensearchdescription+xml";
                case "p10": return "application/pkcs10";
                case "p12": return "application/x-pkcs12";
                case "p7b": return "application/x-pkcs7-certificates";
                case "p7c": return "application/pkcs7-mime";
                case "p7m": return "application/pkcs7-mime";
                case "p7r": return "application/x-pkcs7-certreqresp";
                case "p7s": return "application/pkcs7-signature";
                case "pbm": return "image/x-portable-bitmap";
                case "pcast": return "application/x-podcast";
                case "pct": return "image/pict";
                case "pcx": return "application/octet-stream";
                case "pcz": return "application/octet-stream";
                case "pdf": return "application/pdf";
                case "pfb": return "application/octet-stream";
                case "pfm": return "application/octet-stream";
                case "pfx": return "application/x-pkcs12";
                case "pgm": return "image/x-portable-graymap";
                case "pic": return "image/pict";
                case "pict": return "image/pict";
                case "pkgdef": return "text/plain";
                case "pkgundef": return "text/plain";
                case "pko": return "application/vnd.ms-pki.pko";
                case "pls": return "audio/scpls";
                case "pma": return "application/x-perfmon";
                case "pmc": return "application/x-perfmon";
                case "pml": return "application/x-perfmon";
                case "pmr": return "application/x-perfmon";
                case "pmw": return "application/x-perfmon";
                case "png": return "image/png";
                case "pnm": return "image/x-portable-anymap";
                case "pnt": return "image/x-macpaint";
                case "pntg": return "image/x-macpaint";
                case "pnz": return "image/png";
                case "pot": return "application/vnd.ms-powerpoint";
                case "potm": return "application/vnd.ms-powerpoint.template.macroenabled.12";
                case "potx": return "application/vnd.openxmlformats-officedocument.presentationml.template";
                case "ppa": return "application/vnd.ms-powerpoint";
                case "ppam": return "application/vnd.ms-powerpoint.addin.macroenabled.12";
                case "ppm": return "image/x-portable-pixmap";
                case "pps": return "application/vnd.ms-powerpoint";
                case "ppsm": return "application/vnd.ms-powerpoint.slideshow.macroenabled.12";
                case "ppsx": return "application/vnd.openxmlformats-officedocument.presentationml.slideshow";
                case "ppt": return "application/vnd.ms-powerpoint";
                case "pptm": return "application/vnd.ms-powerpoint.presentation.macroenabled.12";
                case "pptx": return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case "prf": return "application/pics-rules";
                case "prm": return "application/octet-stream";
                case "prx": return "application/octet-stream";
                case "ps": return "application/postscript";
                case "psc1": return "application/powershell";
                case "psd": return "application/octet-stream";
                case "psess": return "application/xml";
                case "psm": return "application/octet-stream";
                case "psp": return "application/octet-stream";
                case "pub": return "application/x-mspublisher";
                case "pwz": return "application/vnd.ms-powerpoint";
                case "qht": return "text/x-html-insertion";
                case "qhtm": return "text/x-html-insertion";
                case "qt": return "video/quicktime";
                case "qti": return "image/x-quicktime";
                case "qtif": return "image/x-quicktime";
                case "qtl": return "application/x-quicktimeplayer";
                case "qxd": return "application/octet-stream";
                case "ra": return "audio/x-pn-realaudio";
                case "ram": return "audio/x-pn-realaudio";
                case "rar": return "application/octet-stream";
                case "ras": return "image/x-cmu-raster";
                case "rat": return "application/rat-file";
                case "rc": return "text/plain";
                case "rc2": return "text/plain";
                case "rct": return "text/plain";
                case "rdlc": return "application/xml";
                case "resx": return "application/xml";
                case "rf": return "image/vnd.rn-realflash";
                case "rgb": return "image/x-rgb";
                case "rgs": return "text/plain";
                case "rm": return "application/vnd.rn-realmedia";
                case "rmi": return "audio/mid";
                case "rmp": return "application/vnd.rn-rn_music_package";
                case "roff": return "application/x-troff";
                case "rpm": return "audio/x-pn-realaudio-plugin";
                case "rqy": return "text/x-ms-rqy";
                case "rtf": return "application/rtf";
                case "rtx": return "text/richtext";
                case "ruleset": return "application/xml";
                case "s": return "text/plain";
                case "safariextz": return "application/x-safari-safariextz";
                case "scd": return "application/x-msschedule";
                case "sct": return "text/scriptlet";
                case "sd2": return "audio/x-sd2";
                case "sdp": return "application/sdp";
                case "sea": return "application/octet-stream";
                case "searchconnector-ms": return "application/windows-search-connector+xml";
                case "setpay": return "application/set-payment-initiation";
                case "setreg": return "application/set-registration-initiation";
                case "settings": return "application/xml";
                case "sgimb": return "application/x-sgimb";
                case "sgml": return "text/sgml";
                case "sh": return "application/x-sh";
                case "shar": return "application/x-shar";
                case "shtml": return "text/html";
                case "sit": return "application/x-stuffit";
                case "sitemap": return "application/xml";
                case "skin": return "application/xml";
                case "sldm": return "application/vnd.ms-powerpoint.slide.macroenabled.12";
                case "sldx": return "application/vnd.openxmlformats-officedocument.presentationml.slide";
                case "slk": return "application/vnd.ms-excel";
                case "sln": return "text/plain";
                case "slupkg-ms": return "application/x-ms-license";
                case "smd": return "audio/x-smd";
                case "smi": return "application/octet-stream";
                case "smx": return "audio/x-smd";
                case "smz": return "audio/x-smd";
                case "snd": return "audio/basic";
                case "snippet": return "application/xml";
                case "snp": return "application/octet-stream";
                case "sol": return "text/plain";
                case "sor": return "text/plain";
                case "spc": return "application/x-pkcs7-certificates";
                case "spl": return "application/futuresplash";
                case "src": return "application/x-wais-source";
                case "srf": return "text/plain";
                case "ssisdeploymentmanifest": return "text/xml";
                case "ssm": return "application/streamingmedia";
                case "sst": return "application/vnd.ms-pki.certstore";
                case "stl": return "application/vnd.ms-pki.stl";
                case "sv4cpio": return "application/x-sv4cpio";
                case "sv4crc": return "application/x-sv4crc";
                case "svc": return "application/xml";
                case "svg": return "image/svg+xml";
                case "swf": return "application/x-shockwave-flash";
                case "t": return "application/x-troff";
                case "tar": return "application/x-tar";
                case "tcl": return "application/x-tcl";
                case "testrunconfig": return "application/xml";
                case "testsettings": return "application/xml";
                case "tex": return "application/x-tex";
                case "texi": return "application/x-texinfo";
                case "texinfo": return "application/x-texinfo";
                case "tgz": return "application/x-compressed";
                case "thmx": return "application/vnd.ms-officetheme";
                case "thn": return "application/octet-stream";
                case "tif": return "image/tiff";
                case "tiff": return "image/tiff";
                case "tlh": return "text/plain";
                case "tli": return "text/plain";
                case "toc": return "application/octet-stream";
                case "tr": return "application/x-troff";
                case "trm": return "application/x-msterminal";
                case "trx": return "application/xml";
                case "ts": return "video/vnd.dlna.mpeg-tts";
                case "tsv": return "text/tab-separated-values";
                case "ttf": return "application/octet-stream";
                case "tts": return "video/vnd.dlna.mpeg-tts";
                case "txt": return "text/plain";
                case "u32": return "application/octet-stream";
                case "uls": return "text/iuls";
                case "user": return "text/plain";
                case "ustar": return "application/x-ustar";
                case "vb": return "text/plain";
                case "vbdproj": return "text/plain";
                case "vbk": return "video/mpeg";
                case "vbproj": return "text/plain";
                case "vbs": return "text/vbscript";
                case "vcf": return "text/x-vcard";
                case "vcproj": return "application/xml";
                case "vcs": return "text/plain";
                case "vcxproj": return "application/xml";
                case "vddproj": return "text/plain";
                case "vdp": return "text/plain";
                case "vdproj": return "text/plain";
                case "vdx": return "application/vnd.ms-visio.viewer";
                case "vml": return "text/xml";
                case "vscontent": return "application/xml";
                case "vsct": return "text/xml";
                case "vsd": return "application/vnd.visio";
                case "vsi": return "application/ms-vsi";
                case "vsix": return "application/vsix";
                case "vsixlangpack": return "text/xml";
                case "vsixmanifest": return "text/xml";
                case "vsmdi": return "application/xml";
                case "vspscc": return "text/plain";
                case "vss": return "application/vnd.visio";
                case "vsscc": return "text/plain";
                case "vssettings": return "text/xml";
                case "vssscc": return "text/plain";
                case "vst": return "application/vnd.visio";
                case "vstemplate": return "text/xml";
                case "vsto": return "application/x-ms-vsto";
                case "vsw": return "application/vnd.visio";
                case "vsx": return "application/vnd.visio";
                case "vtx": return "application/vnd.visio";
                case "wav": return "audio/wav";
                case "wave": return "audio/wav";
                case "wax": return "audio/x-ms-wax";
                case "wbk": return "application/msword";
                case "wbmp": return "image/vnd.wap.wbmp";
                case "wcm": return "application/vnd.ms-works";
                case "wdb": return "application/vnd.ms-works";
                case "wdp": return "image/vnd.ms-photo";
                case "webarchive": return "application/x-safari-webarchive";
                case "webtest": return "application/xml";
                case "wiq": return "application/xml";
                case "wiz": return "application/msword";
                case "wks": return "application/vnd.ms-works";
                case "wlmp": return "application/wlmoviemaker";
                case "wlpginstall": return "application/x-wlpg-detect";
                case "wlpginstall3": return "application/x-wlpg3-detect";
                case "wm": return "video/x-ms-wm";
                case "wma": return "audio/x-ms-wma";
                case "wmd": return "application/x-ms-wmd";
                case "wmf": return "application/x-msmetafile";
                case "wml": return "text/vnd.wap.wml";
                case "wmlc": return "application/vnd.wap.wmlc";
                case "wmls": return "text/vnd.wap.wmlscript";
                case "wmlsc": return "application/vnd.wap.wmlscriptc";
                case "wmp": return "video/x-ms-wmp";
                case "wmv": return "video/x-ms-wmv";
                case "wmx": return "video/x-ms-wmx";
                case "wmz": return "application/x-ms-wmz";
                case "wpl": return "application/vnd.ms-wpl";
                case "wps": return "application/vnd.ms-works";
                case "wri": return "application/x-mswrite";
                case "wrl": return "x-world/x-vrml";
                case "wrz": return "x-world/x-vrml";
                case "wsc": return "text/scriptlet";
                case "wsdl": return "text/xml";
                case "wvx": return "video/x-ms-wvx";
                case "x": return "application/directx";
                case "xaf": return "x-world/x-vrml";
                case "xaml": return "application/xaml+xml";
                case "xap": return "application/x-silverlight-app";
                case "xbap": return "application/x-ms-xbap";
                case "xbm": return "image/x-xbitmap";
                case "xdr": return "text/plain";
                case "xht": return "application/xhtml+xml";
                case "xhtml": return "application/xhtml+xml";
                case "xla": return "application/vnd.ms-excel";
                case "xlam": return "application/vnd.ms-excel.addin.macroenabled.12";
                case "xlc": return "application/vnd.ms-excel";
                case "xld": return "application/vnd.ms-excel";
                case "xlk": return "application/vnd.ms-excel";
                case "xll": return "application/vnd.ms-excel";
                case "xlm": return "application/vnd.ms-excel";
                case "xls": return "application/vnd.ms-excel";
                case "xlsb": return "application/vnd.ms-excel.sheet.binary.macroenabled.12";
                case "xlsm": return "application/vnd.ms-excel.sheet.macroenabled.12";
                case "xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case "xlt": return "application/vnd.ms-excel";
                case "xltm": return "application/vnd.ms-excel.template.macroenabled.12";
                case "xltx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.template";
                case "xlw": return "application/vnd.ms-excel";
                case "xml": return "text/xml";
                case "xmta": return "application/xml";
                case "xof": return "x-world/x-vrml";
                case "xoml": return "text/plain";
                case "xpm": return "image/x-xpixmap";
                case "xps": return "application/vnd.ms-xpsdocument";
                case "xrm-ms": return "text/xml";
                case "xsc": return "application/xml";
                case "xsd": return "text/xml";
                case "xsf": return "text/xml";
                case "xsl": return "text/xml";
                case "xslt": return "text/xml";
                case "xsn": return "application/octet-stream";
                case "xss": return "application/xml";
                case "xtp": return "application/octet-stream";
                case "xwd": return "image/x-xwindowdump";
                case "z": return "application/x-compress";
                case "zip": return "application/x-zip-compressed";
                case "json":return "application/json";
                #endregion
                default: return "application/octet-stream";
            }
        }
        public static object LoadProvider(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var items = value.Trim(' ').TrimStart(' ').TrimEnd(' ').Split(",");
            var asm = System.Runtime.Loader.AssemblyLoadContext.Default
                   .LoadFromAssemblyPath(Path.Combine(AppContext.BaseDirectory, items[1] + ".dll"));
            //var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(p => p.GetName().Name.ToLower() == items[1].ToLower());
            if (asm == null)
            {
                throw new Exception(string.Format("'{0}' was not found", items[1]));
            }
            var type = asm.GetExportedTypes().FirstOrDefault(p => string.Equals(p.Name, items[0], StringComparison.OrdinalIgnoreCase));
            var ret = asm.CreateInstance(type.FullName);
            return ret;
        }
        public static string GetProviderValueByKey(string Key)
        {
            var xmlConfifFilePath = Path.Combine(BaseAppDirectory, "apps.xml");

            XDocument xdoc = XDocument.Load(xmlConfifFilePath);
            var providerEle = xdoc.Root.Elements().Where(p => p.Name.LocalName == Key).FirstOrDefault();
            if (providerEle == null)
            {
                throw new Exception(string.Format("'{1}' xml node not was not found in {0}", xmlConfifFilePath, Key));
            }
            return providerEle.Attribute("value").Value;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public static string GetLanguageResourceItem(
            string LanguageCode,
            string AppName,
            string RefPageId,
            string PageId,
            string Key,
            string Value)
        {
            if (getLanguageItem == null)
            {
                lock (lockGetLanguageItemMethod)
                {
                    if (getLanguageItem == null)
                    {
                        LanguageItemProvider = LoadProvider(GetProviderValueByKey("language-provider"));
                        getLanguageItem = LanguageItemProvider.GetType().GetMethod("GetLanguageItem");
                    }
                }

            }
            //string LanguageCode, string AppName,string RefPageId, string PageId, string Key, string Value
            return getLanguageItem.Invoke(LanguageItemProvider, new object[]
                 {
                    LanguageCode,
                    AppName,
                    RefPageId,
                    PageId,
                    Key,
                    Value

                 }) as string;
        }
    }
}
