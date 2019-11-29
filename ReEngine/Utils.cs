using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ReEngine
{
    public static class Utils
    {
        /// <summary>
        /// Private field sever for lock and cache
        /// </summary>
        private static readonly object objCheckIfPrimitiveTypeCache = new object();
        private static Hashtable CheckIfPrimitiveTypeCache = new Hashtable();
        /// <summary>
        /// Chech type of value is primitive types
        /// <para>
        /// The method has optimize by cache in Hashtable data
        /// </para>
        /// </summary>
        /// <param name="declaringType"></param>
        /// <returns></returns>
        public static bool IsPrimitiveType(Type declaringType)
        {
            //Check cache 
            if (CheckIfPrimitiveTypeCache[declaringType.FullName] == null)
            {
                //if it was not in cache check type and create cache the return from cache
                lock (objCheckIfPrimitiveTypeCache)
                {
                    if (CheckIfPrimitiveTypeCache[declaringType.FullName] == null)
                    {
                        CheckIfPrimitiveTypeCache = new Hashtable();
                        declaringType = Nullable.GetUnderlyingType(declaringType) ?? declaringType;
                        if (declaringType.IsPrimitive)
                        {
                            CheckIfPrimitiveTypeCache[declaringType.FullName] = true;
                            return (bool)CheckIfPrimitiveTypeCache[declaringType.FullName];
                        }
                        else
                        {
                            if (declaringType == typeof(string))
                            {
                                CheckIfPrimitiveTypeCache[declaringType.FullName] = true;
                                return (bool)CheckIfPrimitiveTypeCache[declaringType.FullName];
                            }
                            if (declaringType == typeof(DateTime))
                            {
                                CheckIfPrimitiveTypeCache[declaringType.FullName] = true;
                                return (bool)CheckIfPrimitiveTypeCache[declaringType.FullName];
                            }
                            if (declaringType == typeof(object))
                            {
                                CheckIfPrimitiveTypeCache[declaringType.FullName] = true;
                                return (bool)CheckIfPrimitiveTypeCache[declaringType.FullName];
                            }
                            if (declaringType == typeof(Decimal))
                            {
                                CheckIfPrimitiveTypeCache[declaringType.FullName] = true;
                                return (bool)CheckIfPrimitiveTypeCache[declaringType.FullName];
                            }
                            else if (declaringType.IsClass)
                            {
                                CheckIfPrimitiveTypeCache[declaringType.FullName] = true;
                                return (bool)CheckIfPrimitiveTypeCache[declaringType.FullName];
                            }
                        }
                        CheckIfPrimitiveTypeCache[declaringType.FullName] = false;
                        return (bool)CheckIfPrimitiveTypeCache[declaringType.FullName];

                    }
                }
            }
            //if it was in cache the return without consider
            return (bool)(CheckIfPrimitiveTypeCache[declaringType.FullName]);
        }
    }
}
