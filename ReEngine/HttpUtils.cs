using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReEngine
{
    internal class Constants
    {
        public const string AppKey = "__application__";
        public const string PageKey = "__page_path__";
    }
    public static class HttpUtils
    {
        private static IHttpContextAccessor _accessor;
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {

            _accessor = httpContextAccessor;
        }
        /// <summary>
        /// Get current page id
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentPageId()
        {
            return HttpContext.Items[Constants.PageKey].ToString();
        }

        /// <summary>
        /// Curent http context
        /// </summary>
        public static HttpContext HttpContext
        {
            get
            {
                if (_accessor == null)
                {
                    throw new Exception("It looks like yoy forgot call \r\n" +
                        "ReEngine.ReEngine.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());\r\n" +
                        "at 'Configure' in Startup.cs");
                }
                return _accessor.HttpContext;
            }
        }
        /// <summary>
        /// get session has been store in http Session
        /// <para>
        /// if key was not found in http session and defaultvalue has been set.
        /// Default value will be store in session with sepcifiy key
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetSessionValue<T>(string Key, T defaultValue = default(T))
        {
            var bff = new byte[] { };
            if (HttpUtils.HttpContext.Session != null)
            {
                if (HttpUtils.HttpContext.Session.TryGetValue(Key, out bff))
                {
                    var strJson = System.Text.UTF8Encoding.UTF8.GetString(bff);
                    var ret = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(strJson);
                    return ret;
                }
                else
                {
                    if ((object)defaultValue != null)
                    {
                        var strJSON = Newtonsoft.Json.JsonConvert.SerializeObject((object)defaultValue);
                        HttpUtils.HttpContext.Session.Set(Key, System.Text.UTF8Encoding.UTF8.GetBytes(strJSON));
                        return defaultValue;
                    }

                }
            }
            return default(T);
        }
        /// <summary>
        /// Set Session Value by key
        /// <para>
        /// if the value is null nothing to do
        /// </para>
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public static void SetSessionValue(string Key, object Value)
        {
            if (Value != null)
            {
                var strJSON = Newtonsoft.Json.JsonConvert.SerializeObject(Value);
                HttpUtils.HttpContext.Session.Set(Key, System.Text.UTF8Encoding.UTF8.GetBytes(strJSON));
            }
        }
        public static void SetAppToCurrentContext(ApplicationInfo applicationInfo)
        {
            HttpContext.Items[Constants.AppKey] = applicationInfo;
        }
        /// <summary>
        /// Get Current language code
        /// </summary>
        /// <returns></returns>
        public static string GetLanguageCode()
        {
            return GetSessionValue<string>(Localize.LANGUAGECODE_KEY,ReEngine.WebHost.Get().LocalizeDefault.Language);
        }
        /// <summary>
        /// Get view path of page
        /// </summary>
        /// <returns></returns>
        public static string GetViewPath()
        {
            return HttpUtils.HttpContext.Items[Constants.PageKey].ToString();
        }
        /// <summary>
        /// Get current Application info
        /// </summary>
        /// <returns></returns>
        public static ReEngine.ApplicationInfo GetApp()
        {
            return HttpUtils.HttpContext.Items[Constants.AppKey] as ReEngine.ApplicationInfo;
        }
        /// <summary>
        /// Get full url of this web site
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough]
        public static string GetAbsUrl()
        {
            return HttpUtils.HttpContext.Request.Scheme + "://" + HttpUtils.HttpContext.Request.Host;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public static string ToAbsUrl(params string[] Args)
        {
            return GetAbsUrl() + "/" + string.Join('/', Args);
        }
        [System.Diagnostics.DebuggerStepThrough]
        public static string GetTenancy()
        {
            if (GetApp().IsMultiTenancy)
            {
                return HttpUtils.HttpContext.Request.Path.Value.Split('/')[1];
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// Get full url of current Applcation
        /// </summary>
        /// <returns></returns>
        public static string GetAbsUrlOfApp()
        {
            string appDir = ReEngine.WebHost.Get().AppsDirectory.Substring(ReEngine.WebHost.Get().StaticDirectory.Length, ReEngine.WebHost.Get().AppsDirectory.Length - ReEngine.WebHost.Get().StaticDirectory.Length);
            appDir = appDir.TrimStart('/').TrimEnd('/');
            if (!GetApp().IsMultiTenancy)
            {
                if (!string.IsNullOrEmpty(GetApp().HostDir))
                {
                    return GetAbsUrl() + "/" + GetApp().HostDir;
                }
                else
                {
                    return GetAbsUrl();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(GetApp().HostDir))
                {
                    return GetAbsUrl() + "/" + GetTenancy() + "/" + GetApp().HostDir;
                }
                else
                {
                    return GetAbsUrl() + "/" + GetTenancy();
                }
            }
        }
        /// <summary>
        /// Set relative url to http current context
        /// </summary>
        /// <param name="refFilePath"></param>
        public static void SetPagePathToCurrentContext(string refFilePath)
        {
            HttpContext.Items.Add(Constants.PageKey, refFilePath);
        }

        /// <summary>
        /// Get Absolute url of current request including query string
        /// </summary>
        /// <returns></returns>
        public static string GetFullUrlOfCurrentRequest()
        {
            if (HttpContext.Request.Query.Count == 0)
            {
                return GetAbsUrl() + HttpContext.Request.Path.Value;
            }
            else
            {
                return GetAbsUrl() + HttpContext.Request.Path.Value + "?" + HttpContext.Request.QueryString;
            }
        }
        /// <summary>
        /// Get login url
        /// <para>
        /// if LoginUrl of current app in Config.xml start with "~/"  will return full url of web site and login url. 
        /// Exmaple: Web host at https://localhost:5000 and LoginUrl of current app in Config.xml is '~/mylogin'. 
        /// The method will return https://localhost:5000/mylogin 
        /// </para>
        /// <para>
        /// if LoginUrl of current app in Config.xml is in another domain the method will return LoginUrl 
        /// </para>
        /// <para>
        /// default it will return full url of current app (see  GetAbsUrlOfApp) and Loginurl
        /// </para>
        /// </summary>
        /// <para>if LoginUrl of current app in Config.xml start with "~/"  will return full url of web site and login url </para>
        /// <returns></returns>
        public static string GetLoginUrlOfApp()
        {
            if (GetApp().LoginUrl.IndexOf("~/") == 0)
            {
                return GetAbsUrl() + "/" + GetApp().LoginUrl.Substring(2, GetApp().LoginUrl.Length - 1);
            }
            else if (GetApp().LoginUrl.IndexOf("://") > -1)
            {
                return GetApp().LoginUrl;
            }
            else
            {
                return GetAbsUrlOfApp() + "/" + GetApp().LoginUrl;
            }
        }
        public static string GetStaticResourceUrlOfApp()
        {


            string appDir = ReEngine.WebHost.Get().AppsDirectory.Substring(ReEngine.WebHost.Get().StaticDirectory.Length, ReEngine.WebHost.Get().AppsDirectory.Length - ReEngine.WebHost.Get().StaticDirectory.Length);
            appDir = appDir.TrimStart('/').TrimEnd('/');
            return GetAbsUrl() + "/" + appDir + "/" + GetApp().Dir;
            //if (string.IsNullOrEmpty(GetApp().HostDir))
            //{

            //}
            //else
            //{
            //    return GetAbsUrl()+"/"+ appDir + "/" + GetApp().HostDir;
            //}
        }
        public static IHtmlContent ToJSON(object Data)
        {
            return new HtmlString(Newtonsoft.Json.JsonConvert.SerializeObject(Data));
        }
    }
}

