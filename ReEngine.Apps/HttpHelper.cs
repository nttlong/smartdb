//using Microsoft.AspNetCore.Html;
//using Microsoft.AspNetCore.Http;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Net.Http;
//using System.Text;

//namespace ReEngine.Web
//{
//    public static class HttpUtils
//    {
//        public static HttpContext HttpContext
//        {
//            get
//            {
//                return ReEngine.HttpUtils.HttpContext;
//            }
//        }

//        public static string GetCurrentPageId()
//        {
//            return ReEngine.HttpUtils.GetCurrentPageId();
            
//        }


//        /// <summary>
//        /// Get Current language code
//        /// </summary>
//        /// <returns></returns>
//        public static string GetLanguageCode()
//        {
//            return ReEngine.Localize.GetLanguageCode(HttpUtils.HttpContext, "en");
//        }
//        /// <summary>
//        /// Get view path of page
//        /// </summary>
//        /// <returns></returns>
//        public static string GetViewPath()
//        {
//            return HttpUtils.HttpContext.Items[Constants.PageKey].ToString();
//        }
//        /// <summary>
//        /// Get current Application info
//        /// </summary>
//        /// <returns></returns>
//        public static ReEngine.ApplicationInfo GetApp()
//        {
//            return HttpUtils.HttpContext.Items[Constants.AppKey] as ReEngine.ApplicationInfo;
//        }
//        /// <summary>
//        /// Get full url of this web site
//        /// </summary>
//        /// <returns></returns>
//        [System.Diagnostics.DebuggerStepThrough]
//        public static string GetAbsUrl()
//        {
//            return HttpUtils.HttpContext.Request.Scheme + "://" + HttpUtils.HttpContext.Request.Host;
//        }
//        [System.Diagnostics.DebuggerStepThrough]
//        public static string ToAbsUrl(params string[] Args)
//        {
//            return GetAbsUrl() + "/" + string.Join('/', Args);
//        }
//        [System.Diagnostics.DebuggerStepThrough]
//        public static string GetTenancy()
//        {
//            if (GetApp().IsMultiTenancy)
//            {
//                return HttpUtils.HttpContext.Request.Path.Value.Split('/')[1];
//            }
//            else
//            {
//                return "";
//            }
//        }
//        /// <summary>
//        /// Get full url of current Applcation
//        /// </summary>
//        /// <returns></returns>
//        public static string GetAbsUrlOfApp()
//        {
//            string appDir = ReEngine.WebHost.Get().AppsDirectory.Substring(ReEngine.WebHost.Get().StaticDirectory.Length, ReEngine.WebHost.Get().AppsDirectory.Length - ReEngine.WebHost.Get().StaticDirectory.Length);
//            appDir = appDir.TrimStart('/').TrimEnd('/');
//            if (!GetApp().IsMultiTenancy)
//            {
//                if (!string.IsNullOrEmpty(GetApp().HostDir))
//                {
//                    return GetAbsUrl() + "/" + GetApp().HostDir;
//                }
//                else
//                {
//                    return GetAbsUrl();
//                }
//            }
//            else
//            {
//                if (!string.IsNullOrEmpty(GetApp().HostDir))
//                {
//                    return GetAbsUrl()+"/"+GetTenancy() + "/" + GetApp().HostDir;
//                }
//                else
//                {
//                    return GetAbsUrl() + "/" + GetTenancy();
//                }
//            }
//        }
//        /// <summary>
//        /// Get Absolute url of current request including query string
//        /// </summary>
//        /// <returns></returns>
//        public static string GetFullUrlOfCurrentRequest()
//        {
//            if (HttpContext.Request.Query.Count == 0)
//            {
//                return GetAbsUrl() + HttpContext.Request.Path.Value;
//            }
//            else
//            {
//                return GetAbsUrl() + HttpContext.Request.Path.Value + "?" + HttpContext.Request.QueryString;
//            }
//        }
//        /// <summary>
//        /// Get login url
//        /// <para>
//        /// if LoginUrl of current app in Config.xml start with "~/"  will return full url of web site and login url. 
//        /// Exmaple: Web host at https://localhost:5000 and LoginUrl of current app in Config.xml is '~/mylogin'. 
//        /// The method will return https://localhost:5000/mylogin 
//        /// </para>
//        /// <para>
//        /// if LoginUrl of current app in Config.xml is in another domain the method will return LoginUrl 
//        /// </para>
//        /// <para>
//        /// default it will return full url of current app (see  GetAbsUrlOfApp) and Loginurl
//        /// </para>
//        /// </summary>
//        /// <para>if LoginUrl of current app in Config.xml start with "~/"  will return full url of web site and login url </para>
//        /// <returns></returns>
//        public static string GetLoginUrlOfApp()
//        {
//            if (GetApp().LoginUrl.IndexOf("~/") == 0)
//            {
//                return GetAbsUrl() + "/" + GetApp().LoginUrl.Substring(2, GetApp().LoginUrl.Length - 1);
//            }
//            else if (GetApp().LoginUrl.IndexOf("://") > -1)
//            {
//                return GetApp().LoginUrl;
//            }
//            else
//            {
//                return GetAbsUrlOfApp() + "/" + GetApp().LoginUrl;
//            }
//        }
//        public static string GetStaticResourceUrlOfApp()
//        {
            
            
//            string appDir = ReEngine.WebHost.Get().AppsDirectory.Substring(ReEngine.WebHost.Get().StaticDirectory.Length, ReEngine.WebHost.Get().AppsDirectory.Length- ReEngine.WebHost.Get().StaticDirectory.Length);
//            appDir = appDir.TrimStart('/').TrimEnd('/');
//            return GetAbsUrl() + "/" + appDir + "/" + GetApp().Dir;
//            //if (string.IsNullOrEmpty(GetApp().HostDir))
//            //{

//            //}
//            //else
//            //{
//            //    return GetAbsUrl()+"/"+ appDir + "/" + GetApp().HostDir;
//            //}
//        }
//        public static IHtmlContent ToJSON(object Data)
//        {
//            return  new HtmlString(Newtonsoft.Json.JsonConvert.SerializeObject(Data));
//        }
//    }
//}
