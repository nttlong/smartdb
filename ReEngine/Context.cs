using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository;
using Microsoft.AspNetCore.Http;

namespace ReEngine
{
    public class Context
    {
        static bool isTriggerWatchView = false;
        static readonly object objLockWatchView = new object();
        static readonly object objLock=new object();
        public static Hashtable Loggers = new Hashtable();
        public Context()
        {
            this.Errors = new List<ProcessErrorItem>();
            if (!isTriggerWatchView)
            {
                lock (objLockWatchView)
                {
                    if (!isTriggerWatchView)
                    {
                        ScriprLoader.WatchHtmlView();
                        isTriggerWatchView = true;

                    }
                }
                
            }
            this.AbsUrl = ReEngine.HttpUtils.GetAbsUrl();
            HttpContext = ReEngine.HttpUtils.HttpContext;
            this.WebAppInfo = ReEngine.WebHost.Get();
        }
        public string AppName { get; set; }
        public string PageId { get; set; }
        public string Token { get; set; }
        public string Tenancy { get; set; }
        public HttpContext HttpContext { get; set; }
        public WebApplication WebAppInfo { get; }
        public ApplicationInfo Application { get; set; }
        public object User { get; set; }
        public List<ProcessErrorItem> Errors { get; set; }
        /// <summary>
        /// Root Physical Directory Path of web host
        /// </summary>
        public string BaseAppDirectory
        {
            get
            {
                return ScriprLoader.BaseAppDirectory;
            }
        }
        public ILog Logger
        {
            get
            {
                var instanceName = string.Format("{0}/{1}", this.AppName, this.PageId??this.ScriptPath);
                if (Loggers[instanceName] == null)
                {
                    lock (objLock)
                    {
                        if (Loggers[instanceName] == null)
                        {
                            var file = Path.Join("logs", this.AppName, this.ScriptPath.Replace('/', Path.DirectorySeparatorChar)).Replace('@', Path.DirectorySeparatorChar) + ".txt"; ;
                            if (this.PageId != null)
                            {
                              file=  Path.Join("logs", this.AppName, this.PageId.Replace('/', Path.DirectorySeparatorChar)).Replace('@', Path.DirectorySeparatorChar) + ".txt";
                            }
                            PatternLayout layout = new PatternLayout("%date{MMM/dd/yyyy HH:mm:ss,fff} [%thread] %-5level %logger %ndc – %message%newline");
                            LevelMatchFilter filter = new LevelMatchFilter();
                            filter.LevelToMatch = Level.All;
                            filter.ActivateOptions();
                            RollingFileAppender appender = new RollingFileAppender();
                            appender.File = file;
                            appender.ImmediateFlush = true;
                            appender.AppendToFile = true;
                            appender.RollingStyle = RollingFileAppender.RollingMode.Date;
                            appender.DatePattern = "-yyyy-MM-dd";
                            appender.LockingModel = new FileAppender.MinimalLock();
                            appender.Name = string.Format("{0}Appender", instanceName);
                            appender.AddFilter(filter);
                            appender.Layout = layout;
                            appender.ActivateOptions();
                            string repositoryName = string.Format("{0}Repository",instanceName);
                            ILoggerRepository repository = LoggerManager.CreateRepository(repositoryName);
                            string loggerName = string.Format("{0}Logger", instanceName);
                            BasicConfigurator.Configure(repository, appender);
                            Loggers [instanceName] = LogManager.GetLogger(repositoryName, loggerName);
                        }
                    }
                }
                return Loggers[instanceName] as ILog;
            }
        }
        public object TaskInfo { get; set; }
        public string AbsUrl { get; set; }
        public string ScriptPath { get; set; }
        public string LanguageCode { get; set; }
        public string DbSchema { get; set; }
        public string ViewId { get; set; }

        public string GetMimeTypeFromExtensionFile(string Ext)
        {
            return ScriprLoader.GetMimeType(Ext);
        }
        public string GetMimeTypeFromFileName(string FileName)
        {
            return ScriprLoader.GetMimeType(Path.GetExtension(FileName));
        }
        public object LoadProvider(string ProviderPath)
        {
            return ScriprLoader.LoadProvider(ProviderPath);
        }
        public string Res(string Key,string Value = null)
        {
            Value = Value ?? Key;
            return ScriprLoader.GetLanguageResourceItem(this.LanguageCode, this.AppName, this.PageId, this.PageId, Key, Value);
        }
        public string AppRes(string Key, string Value = null)
        {
            Value = Value ?? Key;
            return ScriprLoader.GetLanguageResourceItem(this.LanguageCode, this.AppName, this.PageId, "-", Key, Value);
        }
        public string GRes(string Key, string Value = null)
        {
            Value = Value ?? Key;
            return ScriprLoader.GetLanguageResourceItem(this.LanguageCode, "-", this.PageId, "-", Key, Value);
        }
        public T GetSession<T>(string key, object DefaultValue = null)
        {
            object ret;
            var bytes = new byte[] { };
            var SessionProperty = this.HttpContext.GetType().GetProperty("Session");
            var Session = SessionProperty.GetValue(this.HttpContext);
            var TryGetValueMethod = SessionProperty.PropertyType.GetMethod("TryGetValue");
            var Parameters = new object[] {
                this.DbSchema + "/" + key,  null

            };
            var retCheck = (bool)TryGetValueMethod.Invoke(Session, Parameters);
            if (!retCheck)
            {
                if (DefaultValue != null)
                {
                    return (T)DefaultValue;
                }
                return default(T);
            }
            else
            {
                bytes = Parameters[1] as byte[];
                var strValue = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                ret = Convert.ChangeType(strValue, typeof(T));
                return (T)ret;
            }


        }
        public List<ViewInfo> Views
        {
            get
            {
                return ScriprLoader.Views;
            }
        }

        public IAuth Auth { get; set; }
    }
}
