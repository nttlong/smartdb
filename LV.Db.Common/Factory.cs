using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LV.Db.Common
{
    public class Factory
    {
        public string DBProviderType { get; private set; }
        public string ConnectionString { get; private set; }

        internal static QuerySet<T1> CreateQuery<T1>(ICompiler Provider, string table_name, Expression<Func<T1>> Expr)
        {
            var ret = new QuerySet<T1>(Provider,table_name);
            ret.MapFields(Expr);
            return ret;

        }
        
        public static T Field<T>(string FieldName)
        {
            return default(T);
        }

        
    }
}
