using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
namespace ReEngine
{
    public static class Config
    {
        private static readonly object objCacheValues=new object();

        static Hashtable CacheValues = new Hashtable();
        static Hashtable CacheXDoc = new Hashtable();
        private static Hashtable Watcher=new Hashtable();

        public static string RootDir
        {
            get
            {
                return ScriprLoader.BaseAppDirectory;
            }
        }

        /// <summary>
        /// Load value from config file into an object with type in T
        /// <para>
        /// Example:
        /// </para>
        /// <para>
        /// Config file name is 'MyConfig.xml' (this function just run with xml file format) with content looks like below
        /// </para>
        /// <para>
        /// &lt;txt-value value='mytxt''/&gt;
        /// </para>
        /// <para>
        /// &lt;obj-value&gt;
        /// &lt;txt-value value="hello"/&gt;
        /// &lt;obj-value/&gt; 
        /// </para>
        /// <para>
        /// Jut call GetValue&lt;string;&gt;("MyConfig.xml","txt-value")
        /// </para>
        /// <para>
        /// GetValue&lt;MyClass&gt;("MyConfig.xml","obj-value")
        /// </para>
        /// <para>
        /// public class MyClass {
        /// </para>
        /// public string TxtValue {get;set;}
        /// <para>
        /// </para>
        /// <para>
        /// }
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ConfigPath"></param>
        /// <param name="TagName"></param>
        /// <example>
        /// Config file name is 'MyConfig.xml' (this function just run with cml file format) with content like that
        /// </example>
        /// <returns></returns>

        public static T GetValue<T>(string ConfigPath,string TagName,bool IsDynamic=false)
        {
            if (typeof(T).FullName.IndexOf("System.Collections.Generic.List")==0)
            {
                throw new Exception(string.Format("{0} is not Array", typeof(T).FullName));
            }
            if (ConfigPath == null)
            {
                throw new ArgumentNullException("Path");
            }
            if (TagName == null)
            {
                throw new ArgumentNullException("TagName");
            }
            var key = (typeof(T).FullName+"/"+ ConfigPath + "/" + TagName).ToLower();
            var filePath = string.Join(Path.DirectorySeparatorChar, ScriprLoader.BaseAppDirectory, ConfigPath);
            if (CacheXDoc[filePath] == null)
            {
                lock (objCacheValues)
                {
                    if (CacheXDoc[filePath] == null)
                    {
                        CacheXDoc[filePath] = XDocument.Load(filePath);
                    }
                }
            }
            if (CacheValues[key] == null)
            {
                lock (objCacheValues)
                {
                    if (CacheValues[key] == null)
                    {
                        if (IsDynamic)
                        {
                            var watcher = new FileSystemWatcher();
                            //var Watcher = new FileSystemWatcher();
                            watcher.Path = Path.GetDirectoryName(filePath);
                            watcher.Filter = Path.GetFileName(filePath);
                            watcher.EnableRaisingEvents = true;
                            watcher.NotifyFilter = NotifyFilters.LastWrite |
                            NotifyFilters.CreationTime;
                            watcher.Changed += (s,e)=> {
                                CacheValues.Remove(key);
                            };

                            //Watcher[filePath] = watcher;
                        }
                        XDocument doc = CacheXDoc[filePath] as XDocument;
                        var tagNames = TagName.Split('/');
                        var ele = doc.Root.Elements().FirstOrDefault(p => string.Equals(p.Name.LocalName, tagNames[0], StringComparison.OrdinalIgnoreCase));
                        for(var i = 1; i < tagNames.Length; i++)
                        {
                            ele = ele.Elements().FirstOrDefault(p => string.Equals(p.Name.LocalName, tagNames[i], StringComparison.OrdinalIgnoreCase));
                        }
                       
                        if (ele == null)
                        {
                            throw new Exception(string.Format("'{0}' was not found in '{1}'", TagName, filePath));
                        }
                        
                        if (CheckIfPrimitiveType(typeof(T)))
                        {
                            var val = ele.Attributes().FirstOrDefault(p => string.Equals(p.Name.LocalName, "value", StringComparison.OrdinalIgnoreCase));
                            if (val == null)
                            {
                                throw new Exception(string.Format("'{0}' attribute at '{1}' was not found in '{2}'", "value", TagName, filePath));
                            }
                            CacheValues[key] = Convert.ChangeType(val.Value, typeof(T));
                        }
                        else
                        {
                            CacheValues[key] = ParseToObject(typeof(T), ele, TagName, filePath);
                        }

                    }
                }
            }
            
            return (T)CacheValues[key];
        }

        

        private static object ParseToObject(Type ObjType, XElement root,string RootTag,string configPath)
        {
            if (ObjType.IsArray|| ObjType.FullName.IndexOf("System.Collections.Generic.List") == 0)
            {
                
                Type uType = null;
                
                if (ObjType.GenericTypeArguments.Length > 0)
                {
                    uType = ObjType.GenericTypeArguments[0];
                }
                else
                {
                    uType = ObjType.GetElementType();
                }
                var eles = root.Elements().ToList();
                var lst = Array.CreateInstance(uType, eles.Count);
                //var retList = new List<object>();


                foreach (var ele in eles)
                {
                    var index = eles.IndexOf(ele);
                    var item = ParseToObject(uType, ele, RootTag + "[" + index + "]", configPath);
                    lst.SetValue(item, index);
                  
                }
                return lst;
            }
            var ret = ObjType.Assembly.CreateInstance(ObjType.FullName);
            var properties = ObjType.GetProperties();
            foreach (var p in properties)
            {
                string propertName = ConvertToXmlConvention(p.Name);

                var ele = root.Elements().FirstOrDefault(x => x.Name.LocalName == propertName);
                if (ele != null)
                {
                    if (CheckIfPrimitiveType(p.PropertyType))
                    {
                        var pType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                        var val = Convert.ChangeType(ele.Attributes().FirstOrDefault(x => x.Name.LocalName == "value").Value, pType);
                        p.SetValue(ret, val);
                    }
                    else
                    {
                        var val = ParseToObject(p.PropertyType, ele, RootTag + "/" + propertName, configPath);
                        p.SetValue(ret, val);
                    }
                }
                else
                {
                    throw new Exception(string.Format("'{0}' was not found at '{1}' in '{2}'", propertName, RootTag, configPath));
                }
            }
            return ret;
        }
        //[System.Diagnostics.DebuggerStepThrough]
        private static string ConvertToXmlConvention(string name)
        {
            var ret = "";
            //var isPreUper = false;
            var upperCount = 0;
            var c = 0;
            for(var i = 0; i < name.Length; i++)
            {
                if (Char.IsUpper(name[i]))
                {
                    if (c > 1)
                    {
                        return name;
                    }
                    if (upperCount == 0)
                    {
                        upperCount++;
                        ret += "-" + name.Substring(i, 1).ToLower();
                    }
                    else
                    {
                        ret += name.Substring(i, 1).ToLower();
                        upperCount++;
                    }
                    c++;
                }
                else
                {
                    c = 0;
                    if (upperCount > 1)
                    {
                        ret += "-" + name.Substring(i, 1).ToLower();
                        upperCount = 0;
                    }
                    else
                    {
                        if (upperCount - 1 >= 0)
                        {
                            upperCount--;
                        }
                        ret += name.Substring(i, 1).ToLower();
                    }
                }
            }
            ret = ret.TrimStart('-');
            return ret;
        }

        private static bool CheckIfPrimitiveType(Type declaringType)
        {
            declaringType = Nullable.GetUnderlyingType(declaringType) ?? declaringType;
            if (declaringType.IsPrimitive)
            {
                return true;
            }
            else
            {
                if (declaringType == typeof(string))
                {
                    return true;
                }
                if (declaringType == typeof(DateTime))
                {
                    return true;
                }
                if (declaringType == typeof(object))
                {
                    return true;
                }
                if (declaringType == typeof(Decimal))
                {
                    return true;
                }
                else if (declaringType.IsClass)
                {
                    return false;
                }
            }
            return false;
        }
    }
}
