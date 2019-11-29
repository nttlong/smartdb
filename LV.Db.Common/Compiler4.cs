using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace LV.Db.Common
{
    public class Compiler
    {
        [System.Diagnostics.DebuggerStepThrough]
        internal static string GetLogicalExpression(ICompiler Cmp, Expression expr, List<FieldMapping> fieldsMapping, List<MapAlais> Mapping, ref List<object> Params)
        {
            if (expr is BinaryExpression)
            {
                return CompilerLogical.BinaryExpr(Cmp, (BinaryExpression)expr, fieldsMapping, Mapping, ref Params);
            }
            if (expr is MemberExpression)
            {
                var ret= CompilerLogical.MemberExpr(Cmp, (MemberExpression)expr, fieldsMapping, Mapping, ref Params);
                if (utils.CheckIsBoolField(expr))
                {
                    Params.Add(new ParamConst
                    {
                        Type=typeof(bool),
                        Value=true
                    });
                    return ret + "={" + (Params.Count - 1) + "}";
                }
                
                return ret;
                
            }
            if (expr is ConstantExpression)
            {

                var val = utils.Resolve(expr);
                Params.Add(val);
                if (val == null)
                {
                    return "Null";
                }
                return "{" + (Params.Count - 1) + "}";
            }
            if (expr is MethodCallExpression)
            {
                return CompilerLogical.MethodCallExpr(Cmp, (MethodCallExpression)expr, fieldsMapping, Mapping, ref Params);
            }
            if (expr is UnaryExpression)
            {
                return CompilerLogical.UnaryExpr(Cmp, (UnaryExpression)expr, fieldsMapping, Mapping, ref Params);
            }
            if (expr is InvocationExpression)
            {
                return CompilerLogical.InvocationExpr(Cmp, (InvocationExpression)expr, fieldsMapping, Mapping, ref Params);
            }
            if (expr is ParameterExpression)
            {

                var ret = Compiler.GetField(Cmp, expr, fieldsMapping, Mapping, ref Params);
                if (ret == "*")
                {
                    return ret;
                }
                if (ret.Substring(ret.Length - 2, 2) != ".*")
                {
                    return ret + ".*";
                }
                else
                {
                    return ret;
                }
            }
            if (expr is LambdaExpression)
            {
                return Compiler.GetLogicalExpression(Cmp, ((LambdaExpression)expr).Body, fieldsMapping, Mapping, ref Params);
            }
            if (expr is NewExpression)
            {
                return Compiler.GetNewExpression(Cmp, ((NewExpression)expr), fieldsMapping, Mapping, ref Params);
            }
            throw new NotImplementedException();
        }

        private static string GetNewExpression(ICompiler cmp, NewExpression expr, List<FieldMapping> fieldsMapping, List<MapAlais> mapping, ref List<object> Params)
        {
            var val = utils.Resolve(expr);
            Params.Add(val);
            return "{" + Params.Count + "}";
        }



        internal static string BuildCallFunction(ICompiler Cmp, MethodCallExpression expr, List<FieldMapping> fieldsMapping, List<MapAlais> mapping, ref List<object> Params)
        {
            var functionSchema = utils.Resolve(expr.Arguments[0]).Value as string;
            var functionName = utils.Resolve(expr.Arguments[1]).Value as string;
            var exprParam = expr.Arguments[2] as NewArrayExpression;
            var fields = new List<string>();
            foreach (var x in exprParam.Expressions)
            {
                var fx = GetField(Cmp, x, fieldsMapping, mapping, ref Params);
                fields.Add(fx);
            }
            return Cmp.GetQName(functionSchema, functionName) + "(" + string.Join(",", fields.ToArray()) + ")";
        }
        [System.Diagnostics.DebuggerStepThrough]
        internal static string BuildLogicalFlow(ICompiler Cmp, MethodCallExpression expr, List<FieldMapping> fieldsMapping, List<MapAlais> mapping, ref List<object> Params)
        {
            string swichExpr = null;
            var brances = expr.Arguments[1] as NewArrayExpression;
            var defaultExpr = Compiler.GetLogicalExpression(Cmp, brances.Expressions[brances.Expressions.Count - 1], fieldsMapping, mapping, ref Params);
            var hasSwicth = true;
            if (expr.Arguments[0] is ConstantExpression)
            {
                var val = utils.Resolve(expr.Arguments[0]);
                if (val == null)
                {
                    hasSwicth = false;
                }

            }
            if (hasSwicth)
            {
                swichExpr = Compiler.GetLogicalExpression(Cmp, expr.Arguments[0], fieldsMapping, mapping, ref Params);
            }
            var lstBranhcs = new List<string>();
            for (var i = 0; i < brances.Expressions.Count - 1; i++)
            {
                if (brances.Expressions[i] is MethodCallExpression)
                {
                    var mc = brances.Expressions[i] as MethodCallExpression;
                    var whenExpr = Compiler.GetLogicalExpression(Cmp, mc.Arguments[0], fieldsMapping, mapping, ref Params);
                    var thenExp = Compiler.GetLogicalExpression(Cmp, mc.Arguments[1], fieldsMapping, mapping, ref Params);
                    lstBranhcs.Add("When " + whenExpr + " Then " + thenExp);

                }

            }
            if (swichExpr == null)
            {
                return "Case " + string.Join(" ", lstBranhcs.ToArray()) + " Else " + defaultExpr + " End";
            }
            else
            {
                return "Case " + swichExpr + " " + string.Join(" ", lstBranhcs.ToArray()) + " Else " + defaultExpr + " End";
            }

        }
        //[System.Diagnostics.DebuggerStepThrough]
        internal static string GetTableName(Expression expr, List<MapAlais> mapping, MemberInfo member)
        {
            if (expr is ParameterExpression)
            {
                var mapItem = mapping.FirstOrDefault(p => p.ParamExpr == expr);

                if (mapItem == null)
                {
                    mapItem = mapping.FirstOrDefault(p => member != null && p.Member == member);
                    if (mapItem == null)
                    {
                        mapItem = utils.FindAliasByMember(mapping, member);
                    }
                    if (mapItem == null)
                    {
                        mapItem = utils.FindAliasByExpression(mapping, expr);
                    }

                }
                if (mapItem == null)
                {
                    if ((member != null) && utils.CheckIfPrimitiveType(((PropertyInfo)member).PropertyType))
                    {
                        return null;
                    }
                    return "*";
                }
                if (!string.IsNullOrEmpty(mapItem.TableName))
                {
                    if (!string.IsNullOrEmpty(mapItem.Schema))
                    {
                        return mapItem.Schema + "././" + mapItem.TableName;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(mapItem.AliasName))
                        {
                            return mapItem.AliasName;
                        }
                        return mapItem.TableName;
                    }
                }
                if (mapItem.Children != null && mapItem.Children.Count > 0)
                {
                    List<string> tbls = utils.GetTables(mapItem, member);
                    if (tbls.Count > 0)
                    {
                        return string.Join(",", tbls);
                    }


                }
                return mapItem.AliasName;
            }
            if (expr is MemberExpression)
            {
                var mb = expr as MemberExpression;
                if (mb.Expression is ParameterExpression)
                {
                    var mapItem = utils.FindAliasByExpression(mapping, mb.Expression);
                    if (mapItem == null)
                    {
                        mapItem = utils.FindAliasByMember(mapping, mb.Member);
                    }
                    if (mapItem == null)
                    {
                        mapItem = utils.FindAliasByPropertyType(mapping, ((PropertyInfo)mb.Member).PropertyType);
                    }
                    if (mapItem.Children != null)
                    {
                        var v = mapItem.Children.FirstOrDefault(p => p.Member == mb.Member);
                        if (v != null)
                        {
                            return v.AliasName;
                        }
                    }
                    return mapItem.AliasName;
                }
                else
                {
                    var mapItem = mapping.FirstOrDefault(p => p.Member == mb.Member);
                    if (mapItem != null)
                    {
                        if (mapItem.Children != null)
                        {
                            var v = mapItem.Children.FirstOrDefault(p => p.Member == mb.Member);

                            return v.AliasName;
                        }
                        return mapItem.AliasName;
                    }
                    return GetTableName(mb.Expression, mapping, member);
                }

            }
            if (expr is ConstantExpression)
            {
                return null;
            }
            throw new NotImplementedException();
        }

        internal static string BuildInOperator(ICompiler cmp, MethodCallExpression expr, List<FieldMapping> fieldsMapping, List<MapAlais> mapping, ref List<object> Params)
        {
            var field = Compiler.GetLogicalExpression(cmp, expr.Arguments[0], fieldsMapping, mapping, ref Params);
            var ret = field + " In ";
            var Rights = new List<object>();
            if (expr.Arguments[1] is NewArrayExpression)
            {
                var nx = expr.Arguments[1] as NewArrayExpression;

                foreach (var x in nx.Expressions)
                {
                    var pCount = Params.Count;
                    var FX = Compiler.GetLogicalExpression(cmp, x, fieldsMapping, mapping, ref Params);
                    if (Params.Count > pCount)
                    {
                        if (Params[pCount] is BaseQuerySet)
                        {

                            var subQuery = ((BaseQuerySet)Params[pCount]);
                            Params.RemoveAt(pCount);
                            var sql = subQuery.ToSQLString();

                            sql = utils.RepairParameters(sql, Params, subQuery.Params);
                            Params.AddRange(subQuery.Params);
                            return ret + "(" + sql + ")";
                        }
                        else
                        {
                            return ret + " " + FX;
                        }

                    }
                    else
                    {
                        Rights.Add(FX);
                    }
                }
            }
            throw new NotImplementedException();
        }

        internal static List<QueryFieldInfo> RawCompile(ICompiler Cmp, Expression expr, ref List<object> Params)
        {
            List<QueryMemberInfo> nx = null;
            if (expr is NewExpression)
            {
                nx = ((NewExpression)expr).Arguments.Select(p => new QueryMemberInfo()
                {
                    Expression = p,
                    Member = ((NewExpression)expr).Members[((NewExpression)expr).Arguments.IndexOf(p)]
                }).ToList();
            }
            if (expr is MemberInitExpression)
            {
                nx = ((MemberInitExpression)expr).Bindings.Select(p => new QueryMemberInfo()
                {
                    Expression = ((MemberAssignment)p).Expression,
                    Member = p.Member
                }).ToList();
            }
            var ret = Compiler.GetFields(Cmp, nx, new List<FieldMapping>(), new List<MapAlais>(), new List<ExtraSelectAll>(), ref Params);
            return ret;
        }
        [System.Diagnostics.DebuggerStepThrough]
        internal static List<QueryFieldInfo> GetFields(ICompiler Cmp, List<QueryMemberInfo> expr, List<FieldMapping> fieldsMapping, List<MapAlais> mapAlias, List<ExtraSelectAll> ExtraFields, ref List<object> Params)
        {
            var ret = new List<QueryFieldInfo>();
            foreach (var arg in expr)
            {
                var extraField = ExtraFields.FirstOrDefault(p => p.Expr == arg.Expression);
                if (extraField == null)
                {
                    var x = GetField(Cmp, arg.Expression, fieldsMapping, mapAlias, ref Params);

                    if (arg.Expression is MemberExpression)
                    {
                        var mapItem = utils.FindAliasByExpression(mapAlias, arg.Expression);
                        if (mapItem != null)
                        {
                            if (x.Substring(x.Length - 2, 2) == ".*" && mapItem.TableName != null)
                            {
                                x = x.Replace(".*", "." + Cmp.GetQName("", ((MemberExpression)arg.Expression).Member.Name));
                            }
                        }
                        ret.Add(new QueryFieldInfo()
                        {
                            Field = x,
                            Alias = arg.Member.Name
                        });
                    }
                    else if (arg.Expression is MethodCallExpression)
                    {
                        ret.Add(new QueryFieldInfo()
                        {
                            Field = x,
                            Alias = arg.Member.Name,
                            FnCall = ((MethodCallExpression)arg.Expression).Method.Name
                        });


                    }
                    else if (arg.Expression is UnaryExpression)
                    {
                        ret.Add(new QueryFieldInfo()
                        {
                            Field = x,
                            Alias = (arg.Member != null) ? arg.Member.Name : null
                        });
                    }
                    else
                    {
                        if (x == "*")
                        {
                            ret.Add(new QueryFieldInfo() { Field = x });
                        }
                        else if (x.Substring(x.Length - 2, 2) != ".*")
                        {
                            if (x.IndexOf(",") == -1)
                            {
                                ret.Add(new QueryFieldInfo()
                                {
                                    Field = x,
                                    Alias = (arg.Member != null) ? arg.Member.Name : null
                                });
                            }
                            else
                            {
                                foreach (var f in x.Split(','))
                                {
                                    ret.Add(new QueryFieldInfo()
                                    {
                                        Field = Cmp.GetQName("", f) + ".*"
                                    });
                                }
                            }
                        }
                        else
                        {
                            ret.Add(new QueryFieldInfo() { Field = x });
                        }
                    }
                }
                else
                {
                    ret.AddRange(extraField.Tables.Select(p => new QueryFieldInfo()
                    {
                        Field = Cmp.GetQName("", p) + ".*"
                    }).ToArray());
                }
            }
            return ret;
        }
        [System.Diagnostics.DebuggerStepThrough]
        internal static string GetField(ICompiler Cmp, Expression expr, List<FieldMapping> fieldsMapping, List<MapAlais> mapAlias, ref List<object> Params)
        {
            if (expr is ParameterExpression)
            {
                return GetTxtFieldParameterExpr(Cmp, (ParameterExpression)expr, mapAlias, ref Params);
            }
            if (expr is MemberExpression)
            {
                return GetTxtFieldMemberExpr(Cmp, (MemberExpression)expr, fieldsMapping, mapAlias, ref Params);
            }
            if (expr is BinaryExpression)
            {
                return GetTxtFieldBinaryExpr(Cmp, (BinaryExpression)expr, fieldsMapping, mapAlias, ref Params);
            }
            if (expr is ConstantExpression)
            {
                return GetTxtFieldConstExpr(Cmp, (ConstantExpression)expr, mapAlias, ref Params);
            }
            if (expr is InvocationExpression)
            {
                return GetTxtFieldInvocationExpr(Cmp, (InvocationExpression)expr, mapAlias, ref Params);
            }
            if (expr is MethodCallExpression)
            {
                return CompilerLogical.MethodCallExpr(Cmp, (MethodCallExpression)expr, fieldsMapping, mapAlias, ref Params);
            }
            if (expr is UnaryExpression)
            {
                return CompilerLogical.UnaryExpr(Cmp, (UnaryExpression)expr, fieldsMapping, mapAlias, ref Params);
            }
            throw new NotImplementedException();
        }

        private static string GetTxtFieldInvocationExpr(ICompiler Cmp, InvocationExpression expr, List<MapAlais> mapAlias, ref List<object> Params)
        {
            Params.Add(utils.Resolve(expr));
            return "{" + (Params.Count - 1) + "}";
        }

        internal static string GetTxtFieldBinaryExpr(ICompiler Cmp, BinaryExpression expr, List<FieldMapping> fieldsMapping, List<MapAlais> mapAlias, ref List<object> Params)
        {
            var pLen = Params.Count;
            var op = utils.GetOp(expr.NodeType);
            var left = GetTxtField(Cmp, expr.Left, fieldsMapping, mapAlias, ref Params);
            if (left is SubQuery)
            {
                left = "(" + ((SubQuery)left).Sql + ")";
            }
            var right = GetTxtField(Cmp, expr.Right, fieldsMapping, mapAlias, ref Params);
            if (right is SubQuery)
            {
                right = "(" + ((SubQuery)right).Sql + ")";
            }
            if (Params.Count == pLen + 1)
            {
                if (op == "=" && (((ParamConst)Params[pLen]).Value == null))
                {
                    Params.RemoveAt(pLen);
                    return left + " is null";
                }
                if (op == "!=" && (((ParamConst)Params[pLen]).Value == null))
                {
                    Params.RemoveAt(pLen);
                    return left + " is not null";
                }
            }
            return left + " " + op + right;
        }

        internal static object GetTxtField(ICompiler Cmp, Expression expr, List<FieldMapping> fieldsMapping, List<MapAlais> mapAlias, ref List<object> Params)
        {
            if (expr is BinaryExpression) return GetTxtFieldBinaryExpr(Cmp, (BinaryExpression)expr, fieldsMapping, mapAlias, ref Params);
            if (expr is MemberExpression) return GetTxtFieldMemberExpr(Cmp, (MemberExpression)expr, fieldsMapping, mapAlias, ref Params);
            if (expr is ConstantExpression) return GetTxtFieldConstExpr(Cmp, (ConstantExpression)expr, mapAlias, ref Params);
            if (expr is MethodCallExpression)
            {
                var cx = expr as MethodCallExpression;
                if (cx.Method.Name == "FirstOrDefault")
                {
                    var val = utils.Resolve(cx.Arguments[0]);
                    if (val.Value is BaseQuerySet)
                    {
                        ((BaseQuerySet)val.Value).selectTop = 1;
                        var retValue = ((BaseQuerySet)val.Value).ToSQLString();
                        retValue = utils.RepairParameters(retValue, Params, ((BaseQuerySet)val.Value).Params);
                        Params.AddRange(((BaseQuerySet)val.Value).Params);
                        return new SubQuery()
                        {
                            Sql = retValue
                        };
                    }
                }
                else if (cx.Method.DeclaringType == typeof(SqlFuncs))
                {
                    var ret = CompilerLogical.MethodCallExpr(Cmp, cx, fieldsMapping, mapAlias, ref Params);
                    return ret;
                }
                else
                {
                    var pr = utils.Resolve(expr);
                    Params.Add(pr);
                    return "{" + (Params.Count-1) + "}";
                }


            }
            throw new NotImplementedException();
        }
        internal static string GetTxtFieldConstExpr(ICompiler Cmp, ConstantExpression expr, List<MapAlais> mapAlias, ref List<object> Params)
        {

            var val = utils.Resolve(expr);
            Params.Add(val);
            if (val == null)
            {
                return "Null";
            }
            return "{" + (Params.Count - 1) + "}";

        }
        [System.Diagnostics.DebuggerStepThrough]
        internal static string GetTxtFieldMemberExpr(ICompiler Cmp, MemberExpression expr, List<FieldMapping> FieldsMapping, List<MapAlais> mapAlias, ref List<object> Params)
        {
            var isUseAlias = false;
            MapAlais map = null;
            if (utils.IsConstantExpr(expr))
            {
                Params.Add(utils.Resolve(expr));
                return "{" + (Params.Count - 1) + "}";
            }
            if (!utils.CheckIfPrimitiveType(((PropertyInfo)expr.Member).PropertyType))
            {
                map = mapAlias.FirstOrDefault(p => p.ParamExpr == expr.Expression);
            }
            else
            {
                if (expr.Expression is MemberExpression)
                {
                    map = utils.FindAliasByMember(mapAlias, ((MemberExpression)expr.Expression).Member);
                    if (map == null)
                    {
                        map = utils.FindAliasByExpression(mapAlias, ((MemberExpression)expr.Expression).Expression);
                    }
                }
                else
                {
                    map = mapAlias.FirstOrDefault(p => p.ParamExpr == (ParameterExpression)expr.Expression);
                    if (map == null)
                    {
                        map = mapAlias.FirstOrDefault(p => p.Member == ((MemberExpression)expr).Member);
                    }
                }
            }
            var tblName = "";
            if (map == null)
            {
                map = utils.FindAliasByMember(mapAlias, expr.Member);
                if (map == null)
                {
                    tblName = GetTableName(expr.Expression, mapAlias, expr.Member);
                }
                else
                {
                    tblName = string.IsNullOrEmpty(map.AliasName) ? map.TableName : map.AliasName;
                }
            }
            else
            {
                isUseAlias = !string.IsNullOrEmpty(map.AliasName);
                tblName = string.IsNullOrEmpty(map.AliasName) ? map.TableName : map.AliasName;
                if (tblName == null)
                {
                    var rMap = utils.FindAliasByMember(mapAlias, expr.Member);
                    if (rMap != null)
                    {
                        tblName = rMap.AliasName;
                    }
                    else
                    {
                        if (expr.Expression is MemberExpression)
                        {
                            rMap = utils.FindAliasByPropertyType(mapAlias, ((PropertyInfo)((MemberExpression)expr.Expression).Member).PropertyType);
                            if (rMap != null)
                            {
                                tblName = rMap.AliasName;
                            }
                            else
                            {
                                throw new Exception(string.Format("TableName was not found in 'mapAlias' at {0}", mapAlias.IndexOf(map)));

                            }

                        }
                        else
                        {
                            rMap = utils.FindAliasByExpression(mapAlias, expr.Expression);
                            if (rMap != null)
                            {
                                tblName = rMap.AliasName;
                            }
                            else
                            {
                                throw new Exception(string.Format("TableName was not found in 'mapAlias' at {0}", mapAlias.IndexOf(map)));

                            }
                        }

                    }
                }
            }
            if (string.IsNullOrEmpty(tblName))
            {
                throw new Exception(string.Format("Table of field '{0}' was not found", expr.Member.Name));
            }
            if (tblName.IndexOf(",") == -1)
            {
                if (utils.CheckIfPrimitiveType(((PropertyInfo)expr.Member).PropertyType))
                {
                    if (map != null)
                    {
                        var fm = FieldsMapping.FirstOrDefault(p => p.Member == expr.Member);
                        if (!isUseAlias)
                        {
                            return Cmp.GetFieldName(map.Schema, tblName, ((fm != null) ? fm.FieldName : expr.Member.Name));
                        }
                        else
                        {
                            return Cmp.GetFieldName(null, tblName, ((fm != null) ? fm.FieldName : expr.Member.Name));
                        }
                    }
                    return Cmp.GetFieldName("", tblName, expr.Member.Name);
                }
                else
                {
                    var fm = FieldsMapping.FirstOrDefault(p => p.Member == expr.Member);
                    if (map != null)
                    {

                        return Cmp.GetFieldName(map.Schema, tblName, ((fm != null) ? fm.FieldName : "*")).Replace(@".""*""", ".*");
                    }
                    return Cmp.GetFieldName("", tblName, ((fm != null) ? fm.FieldName : expr.Member.Name));
                }
            }
            else
            {
                return tblName;
            }
        }


        //[System.Diagnostics.DebuggerStepThrough]
        internal static string GetTxtFieldParameterExpr(ICompiler Cmp, ParameterExpression exr, List<MapAlais> mapAlias, ref List<object> Params)
        {
            var tblName = GetTableName(exr, mapAlias, null);
            if (tblName == "*")
            {
                return tblName;
            }
            if (tblName.IndexOf(",") == -1 && tblName.IndexOf(".") == -1)
            {
                return Cmp.GetQName("", tblName) + ".*";
            }
            else if (tblName.IndexOf("././") > -1)
            {
                var indexOfSplit = tblName.IndexOf("././");
                var strTableName = tblName.Substring(0, indexOfSplit);
                var strFieldName = tblName.Substring(indexOfSplit + "././".Length, tblName.Length - (indexOfSplit + "././".Length));
                return Cmp.GetQName(strTableName, strFieldName) + ".*";
            }
            else
            {
                return tblName;
            }
        }
    }

}
