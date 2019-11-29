using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReEngine.Web
{
    public static class clsExtension
    {
        public static ReEngine.ApplicationInfo GetApp(this RazorPageBase Page)
        {
            return HttpUtils.GetApp();
        }
    }
}
