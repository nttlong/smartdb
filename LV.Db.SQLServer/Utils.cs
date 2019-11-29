using LV.Db.Common;

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace LV.Db.SQLServer
{
    public class Utils
    {
       
        internal static bool IsAnonymousType(Type type)
        {
            if (type.IsGenericType)
            {
                var d = type.GetGenericTypeDefinition();
                if (d.IsClass && d.IsSealed && d.Attributes.HasFlag(TypeAttributes.NotPublic))
                {
                    var attributes = d.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
                    if (attributes != null && attributes.Length > 0)
                    {
                        //WOW! We have an anonymous type!!!
                        return true;
                    }
                }
            }
            return false;

        }
    }
}
