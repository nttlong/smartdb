using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ReEngine
{
    /// <summary>
    /// This stattic class use for load Provider
    /// <para>
    /// Provider including:
    /// </para>
    /// <para>
    /// 1- An inetrface declaring
    /// </para>
    /// <para>
    /// 2- An Implementation declaring
    /// </para>
    /// </summary>
    public static class ProviderLoader
    {
        private static readonly object objCacheLock=new object();
        static Hashtable CacheProvider = new Hashtable();
        /// <summary>
        /// Load provider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ConfigPath"></param>
        /// <param name="TagName"></param>
        /// <returns></returns>
        //[System.Diagnostics.DebuggerStepThrough]
        public static T Get<T>(string ConfigPath,string TagName)
        {
            if (CacheProvider[ConfigPath + "/" + TagName]== null){
                lock (objCacheLock)
                {
                    if (CacheProvider[ConfigPath + "/" + TagName] == null)
                    {
                        var providerPathConfig = Config.GetValue<string>(ConfigPath, TagName);
                        var items = providerPathConfig.Split(',');
                        var ProviderPath = items[1];
                        var ProviderTypeName = items[0];
                        var ProviderAssembly = System.Runtime.Loader.AssemblyLoadContext.Default
                               .LoadFromAssemblyPath(Path.Combine(AppContext.BaseDirectory, ProviderPath + ".dll"));
                        var ProviderType = ProviderAssembly.DefinedTypes.First(p => p.Name == ProviderTypeName);
                        var ret = ProviderAssembly.CreateInstance(ProviderType.FullName);
                        if (ret == null)
                        {
                            throw new Exception(string.Format("Provider '{0}' is null", providerPathConfig));
                        }
                        if (!(ret is T))
                        {
                            throw new Exception(string.Format("Provider Type of '{0}' is not type of {1}", providerPathConfig, typeof(T).FullName));
                        }
                        CacheProvider[ConfigPath + "/" + TagName] = ret;

                    }

                }
            }
            return (T)CacheProvider[ConfigPath + "/" + TagName];

        }
    }
}
