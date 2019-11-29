//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;

//namespace ReEngine.Web
//{
//    public class ActionContext
//    {
//        public Hashtable PostData { get; internal set; }
//        public T GetPostData<T>()
//        {
//            if (PostData.Keys.Count == 0)
//            {
//                return default(T);
//            }
//            var ret = Activator.CreateInstance<T>();
//            var pros = typeof(T).GetProperties().Where(p => p.CanWrite)
//                .Join(PostData.Cast<DictionaryEntry>(), p => p.Name.ToLower(), q => q.Key.ToString().ToLower(), (p, q) => new
//                {
//                    Pro=p,
//                    Value = q.Value,
//                    Type=Nullable.GetUnderlyingType(p.PropertyType)??p.PropertyType

//                });
//            foreach(var x in pros)
//            {
//                if (x.Value is Newtonsoft.Json.Linq.JObject)
//                {
//                    var v = x.Value as Newtonsoft.Json.Linq.JObject;
//                    x.Pro.SetValue(ret, v);
//                }
//                else if (x.Value is Newtonsoft.Json.Linq.JArray)
//                {
//                    var v = x.Value as Newtonsoft.Json.Linq.JArray;
//                    x.Pro.SetValue(ret, v);
//                }
//                else
//                {
//                    x.Pro.SetValue(ret, x.Value);
                 
//                }
//            }
//            return ret;
//        }
//    }
//}
