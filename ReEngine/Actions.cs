using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ReEngine
{
    public class Actions
    {
        static Hashtable CacheActions = new Hashtable();
        static Hashtable RevertCache = new Hashtable();
        public static string RegisterAction(string Name, Func<ActionContext, object> handler)
        {

            string api_path = HttpUtils.GetCurrentPageId();
            var key = (HttpUtils.GetApp().Name + "/" + api_path.Replace("/", "-") + "-" + Name).ToLower();
            RevertCache[HttpUtils.HttpContext.Request.Path.Value + "/" + Name] = (HttpUtils.GetApp().Name + "/" + api_path.Replace("/", "-") + "-" + Name).ToLower();
            CacheActions[key] = handler;
            return key;
        }
        public static string GetActionId(string Name)
        {
            return RevertCache[HttpUtils.HttpContext.Request.Path.Value + "/" + Name].ToString();
        }
        public static Func<ActionContext, object> GetActionRegister(string AppName, string id)
        {
            return CacheActions[(AppName + "/" + id).ToLower()] as Func<ActionContext, object>;
        }
    }
}
