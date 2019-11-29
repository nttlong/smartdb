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

namespace ReEngine
{
    public class ServiceContext
    {
        static Hashtable Loggers = new Hashtable();
        static readonly object objLock = new Object();
        /// <summary>
        /// Miliseconds delay time for next step in process
        /// </summary>
        public int DelayTimeToNext { get; set; }
        public ILog Logger
        {
            get
            {
                var dir = this.RelPath.Split("@")[1];
                var instanceName = dir;
                if (Loggers[instanceName] == null)
                {
                    lock (objLock)
                    {
                        if (Loggers[instanceName] == null)
                        {
                            PatternLayout layout = new PatternLayout("%date{MMM/dd/yyyy HH:mm:ss,fff} [%thread] %-5level %logger %ndc – %message%newline");
                            LevelMatchFilter filter = new LevelMatchFilter();
                            filter.LevelToMatch = Level.All;
                            filter.ActivateOptions();
                            RollingFileAppender appender = new RollingFileAppender();
                           
                            
                            appender.File = string.Join(Path.DirectorySeparatorChar, "logs","servicesScripts", dir, "logs.txt");
                            appender.ImmediateFlush = true;
                            appender.AppendToFile = true;
                            appender.RollingStyle = RollingFileAppender.RollingMode.Date;
                            appender.DatePattern = "-yyyy-MM-dd";
                            appender.LockingModel = new FileAppender.MinimalLock();
                            appender.Name = string.Format("{0}Appender", instanceName);
                            appender.AddFilter(filter);
                            appender.Layout = layout;
                            appender.ActivateOptions();
                            appender.MaximumFileSize = "200KB";
                            appender.MaxSizeRollBackups = 1024;
                            appender.AppendToFile = false;
                            appender.RollingStyle = RollingFileAppender.RollingMode.Composite;
                            appender.StaticLogFileName = true;
                            appender.DatePattern = "_MMddyyyy";
                            string repositoryName = string.Format("{0}Repository", instanceName);
                            ILoggerRepository repository = LoggerManager.CreateRepository(repositoryName);
                            string loggerName = string.Format("{0}Logger", instanceName);
                            BasicConfigurator.Configure(repository, appender);
                            Loggers[instanceName] = LogManager.GetLogger(repositoryName, loggerName);
                        }
                    }
                }
                return Loggers[instanceName] as ILog;
            }
        }
        /// <summary>
        /// Full source of service script
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// If any error micro service will be stop
        /// </summary>
        public bool IsError { get; set; }
        /// <summary>
        /// Relative path from Application directory
        /// </summary>
        public string RelPath { get; set; }
    }
}
