using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LV.Db.Common
{
    internal class SortCompiler
    {
        internal static List<string> GetFields(BaseQuerySet qr, Expression Expr)
        {
            var Params = qr.Params;
            var ret = new List<string>();
            if (Expr is NewExpression)
            {
                var nx=Expr as NewExpression;
                foreach(var x in nx.Arguments)
                {
                    var fields = GetFields(qr,x);
                    ret.AddRange(fields);
                }
                return ret;
            }
            if(Expr is MemberExpression)
            {
                var Field = Compiler.GetField(qr.compiler, Expr, qr.FieldsMapping, qr.mapAlias, ref Params);
                ret.Add(Field);
                return ret;
            }
            if (Expr is UnaryExpression)
            {
                var ux = Expr as UnaryExpression;
                var fields = GetFields(qr,ux.Operand);
                ret.AddRange(fields);
                return ret;
            }
            if(Expr is LambdaExpression)
            {
                var lx = Expr as LambdaExpression;
                var fields = GetFields(qr,lx.Body);
                ret.AddRange(fields);
                return ret;
            }
            throw new NotImplementedException();
        }
    }
}
