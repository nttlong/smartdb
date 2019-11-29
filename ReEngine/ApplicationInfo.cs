using System;
using System.Collections.Generic;
using System.IO;

namespace ReEngine
{
    public class ApplicationInfo
    {
        private string _dir;
        static FileSystemWatcher watcher = null;
        private Action _onResourceChange;

        public bool IsMultiTenancy { get; set; }
        public string HostDir { get; set; }
        public void OnResourceChange(Action OnChange)
        {
            this._onResourceChange = OnChange;
        }
        public string Dir
        {
            get
            {
                if (watcher != null) 
                IntallWatchDir();
                return _dir;
            }
            set
            {
                _dir = value;
                if (watcher != null) return;
                IntallWatchDir();
            }
        }

        private void IntallWatchDir()
        {
            watcher = new FileSystemWatcher();
            var watchDir = string.Join(Path.DirectorySeparatorChar, ReEngine.Config.RootDir, _dir.Replace('/', Path.DirectorySeparatorChar)).TrimEnd(Path.DirectorySeparatorChar);
            if (Directory.Exists(watchDir))
            {
                watcher.Path = watchDir;
                watcher.NotifyFilter = NotifyFilters.LastWrite |
                    NotifyFilters.CreationTime;

                watcher.Filter = "*.*";
                watcher.IncludeSubdirectories = true;
                watcher.Changed += (e, s) =>
                {
                    if (this._onResourceChange != null)
                    {
                        this._onResourceChange();
                    }
                };
                watcher.EnableRaisingEvents = true;
            }
        }

        public string IndexPage { get; set; }
        public string Name { get; set; }
        public string LoginUrl { get; set; }
       
    }
}