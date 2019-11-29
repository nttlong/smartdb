using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace LV.Db.Common
{
    public class CompilerLogical
    {
        [System.Diagnostics.DebuggerStepThrough]
        internal static string BinaryExpr(ICompiler Cmp, BinaryExpression expr,List<FieldMapping> fieldsMapping, List<MapAlais> Mapping, ref List<object> Params)
        {

            var op = utils.GetOp(expr.NodeType);
            var left = Compiler.GetLogicalExpression(Cmp,expr.Left, fieldsMapping, Mapping, ref Params);
            if (utils.CheckIsBoolField(expr.Left))
            {
                if (op == "="  && left.IndexOf("={")>-1 && left.IndexOf("}")>-1)
                {
                    left = left.Split("={")[0];
                    Params.RemoveAt(Params.Count-1);
                }
            }
            var pLen = Params.Count;
            var right = Compiler.GetLogicalExpression(Cmp, expr.Right, fieldsMapping, Mapping, ref Params);
            if(Params.Count==pLen+1 && ((ParamConst)Params[pLen]).Value==null && op == "=")
            {
                Params.RemoveAt(pLen);
                return "(" + left + " is null)";
            }
            if (Params.Count == pLen + 1 && ((ParamConst)Params[pLen]).Value == null && op == "!=")
            {
                Params.RemoveAt(pLen);
                return "(" + left + " is not null)";
            }
            var isLeftParentheses = false;
            var isRightParentheses = false;
            if (expr.Left is BinaryExpression)
            {
                if (((((BinaryExpression)expr.Left).Left is BinaryExpression) ||
                    (((BinaryExpression)expr.Left).Right is BinaryExpression)) && ((((BinaryExpression)expr.Left).NodeType != expr.NodeType)))
                {
                    isLeftParentheses = true;
                }
            }
            if (expr.Right is BinaryExpression)
            {
                if (((((BinaryExpression)expr.Right).Left is BinaryExpression) ||
                    (((BinaryExpression)expr.Right).Right is BinaryExpression)) && ((((BinaryExpression)expr.Right).NodeType != expr.NodeType)))
                {
                    isRightParentheses = true;
                }
            }
            if (expr.Left is BinaryExpression && expr.Right is BinaryExpression)
            {
                if (((BinaryExpression)expr.Right).NodeType != ((BinaryExpression)expr.Left).NodeType)
                {
                    if (isLeftParentheses || isRightParentheses)
                    {
                        isLeftParentheses = true;
                        isRightParentheses = true;
                    }
                }
            }
            return (string)(isLeftParentheses ? "(" : "") + left + (string)(isLeftParentheses ? ")" : "") + " " + op + " " +
                 (string)(isRightParentheses ? "(" : "") + right + (string)(isRightParentheses ? ")" : "");
           

        }
        [System.Diagnostics.DebuggerStepThrough]
        internal static string MemberExpr(ICompiler Cmp, MemberExpression mx,List<FieldMapping> fieldsMapping, List<MapAlais> Mapping, ref List<object> Params)
        {
           
            if(utils.IsConstantExpr(mx))
            {
                Params.Add(utils.Resolve(mx));
                return "{" + (Params.Count - 1) + "}";
            }
            if(Mapping.Count==1 && Mapping[0].TableName != null && Mapping[0].Children==null)
            {
                return Cmp.GetFieldName(Mapping[0].Schema, Mapping[0].TableName, mx.Member.Name);
            }
            MapAlais tbl = null;
            string tblName = "";
            if (mx.Expression is MemberExpression)
            {
                tbl =  utils.FindAliasByMember(Mapping, ((MemberExpression)mx.Expression).Member);
            }
            if (tbl != null)
            {
                tblName = tbl.AliasName;
            }
            else
            {
                tblName = Compiler.GetTableName(mx.Expression, Mapping, mx.Member);
                if (tblName == null)
                {
                    var m = utils.FindAliasByMemberExpression(Mapping,mx);
                    if (m != null)
                    {
                        tblName = m.AliasName ?? m.TableName;
                    }
                }
                if (tblName.IndexOf("././") > -1)
                {
                    var indexOfSplit = tblName.IndexOf("././");
                    var strTableName = tblName.Substring(0, indexOfSplit);
                    var strFieldName = tblName.Substring(indexOfSplit+ "././".Length,tblName.Length-(indexOfSplit + "././".Length));
                    return Cmp.GetFieldName(strTableName, strFieldName, mx.Member.Name);
                }
                var fm = fieldsMapping.FirstOrDefault(p => p.Member == mx.Member);
                return Cmp.GetFieldName("", tblName, (fm==null)?mx.Member.Name:fm.FieldName);
            }
            if (tblName.IndexOf(".") > -1)
            {
                if (utils.CheckIfPrimitiveType(((PropertyInfo)mx.Member).PropertyType)){
                    return Cmp.GetFieldName("", tblName, mx.Member.Name);
                }
                return tblName;
            }
            if (tblName.IndexOf(",") > -1)
            {
                tblName = tblName.Split(',')[0];
            }
            return Cmp.GetFieldName("", tblName, mx.Member.Name);
        }

        internal static string InvocationExpr(ICompiler Cmp, InvocationExpression expr, List<FieldMapping> fieldsMapping, List<MapAlais> mapping, ref List<object> Params)
        {
            Params.Add(utils.Resolve(expr));
            return "{" + (Params.Count - 1) + "}";
        }
        
        internal static string UnaryExpr(ICompiler Cmp, UnaryExpression expr,List<FieldMapping> fieldsMapping, List<MapAlais> mapping, ref List<object> Params)
        {
            var op = utils.GetOp(expr.NodeType);
            
            var ret= Compiler.GetLogicalExpression(Cmp,expr.Operand,fieldsMapping, mapping, ref Params);
            if (op != null)
            {
                return op + "(" + ret + ")";
            }
            else
            {
                return ret;
            }
        }
        [System.Diagnostics.DebuggerStepThrough]
        internal static string MethodCallExpr(ICompiler Cmp, MethodCallExpression expr, List<FieldMapping> fieldsMapping, List<MapAlais> mapping, ref List<object> Params)
        {
            Regex regex = new Regex(@"\{\d+\}");
            if (mapping.Count == 0)
            {
                throw new Exception("mapping is Empty");
            }
            if (expr.Method.Name == "FirstOrDefault")
            {
                if(expr.Arguments[0] is MethodCallExpression)
                {
                    var mc1 = expr.Arguments[0] as MethodCallExpression;
                    var subQuery = utils.Resolve(expr.Arguments[0]);
                    if (subQuery.Value is BaseQuerySet)
                    {
                        var sqlQr = subQuery.Value as BaseQuerySet;
                        sqlQr.selectTop = 1;
                        var sql = sqlQr.ToSQLString();

                        sql = utils.RepairParameters(sql, Params, sqlQr.Params);
                        Params.AddRange(sqlQr.Params);
                        return "(" + sql + ")";
                    }
                }
            }
            if (expr.Method.DeclaringType == typeof(SqlFuncs))
            {
                if (expr.Method.Name == "Case")
                {
                    return Compiler.BuildLogicalFlow(Cmp,expr,fieldsMapping, mapping, ref Params);
                }
                if (expr.Method.Name == "Call")
                {
                    return Compiler.BuildCallFunction(Cmp,expr, fieldsMapping, mapping, ref Params);
                }
                if (expr.Method.Name == "In")
                {
                    return Compiler.BuildInOperator(Cmp, expr, fieldsMapping, mapping, ref Params);
                }
                var ParamInfo = new List<SqlFunctionParamInfo>();
                var args = expr.Arguments;
                if(expr.Arguments.Count==1 && expr.Arguments[0] is NewArrayExpression)
                {
                    args = ((NewArrayExpression)expr.Arguments[0]).Expressions;
                }
                foreach (var arg in args)
                {
                    var paramValueIndex = Params.Count;
                    var val = Compiler.GetField(Cmp,arg,fieldsMapping, mapping, ref Params);
                    if (utils.CheckIsBoolField(arg)&&expr.Method.Name=="When")
                    {
                        val = val + "={" + Params.Count + "}";
                        Params.Add(new ParamConst
                        {
                            Type=typeof(bool),
                            Value=true
                        });
                    }
                    if(val is string && regex.IsMatch(val.ToString()))
                    {
                        val = val.Replace("{", @"\{").Replace("}", @"\}");
                    }
                    var name = "agf" + args.IndexOf(arg);
                    if (!(expr.Arguments[0] is NewArrayExpression))
                    {
                        name = expr.Method.GetParameters()[args.IndexOf(arg)].Name;
                    }
                        
                    var paramValue = (Params.Count > 0 && paramValueIndex < Params.Count) ? Params[paramValueIndex] : null;
                    
                    ParamInfo.Add(new SqlFunctionParamInfo()
                    {
                        Name = name,
                        ParamValue = paramValue,
                        Value = val
                    });
                }
                var ret = Cmp.GetSqlFunction(expr.Method.Name, ParamInfo.ToArray());
                
                if (regex.IsMatch(ret))
                {
                    ret= string.Format(ret, ParamInfo.Select(p => p.Value).ToArray());
                }
                ret = ret.Replace(@"\{", "{").Replace(@"\}", "}");
                return ret;
            }
            else
            {
                Params.Add(utils.Resolve(expr));
                return "{" + (Params.Count - 1) + "}";
            }
            throw new NotImplementedException();
        }
    }

    internal class CallSqlFunction
    {
        public string Field { get; internal set; }
        public string FunctionName { get; internal set; }
    }
}
