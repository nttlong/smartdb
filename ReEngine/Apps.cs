using System;
using System.Collections.Generic;
using System.Text;

namespace ReEngine
{
    public class Apps
    {
        public static string BaseAppDirectory
        {
            get
            {
                return ReEngine.ScriprLoader.BaseAppDirectory;
            }
        }



    }
    public static class WebHost
    {
        public static WebApplication Get()
        {
            return Config.GetValue<WebApplication>("config.xml", "web-application", true);
        }
    }
}
