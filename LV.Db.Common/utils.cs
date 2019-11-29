using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace LV.Db.Common
{
    internal class utils
    {
        static string[] Operators = new string[]
        {
            "Add"
        };
        static string[] OperatorMapping = new string[]
        {
            "+"
        };

        internal static Hashtable TableInfoCache = new Hashtable();
        private static readonly object objCheckIfPrimitiveTypeCache=new object();
        private static readonly object objLockCacheGetTableInfo=new object();
        private static Hashtable CheckIfPrimitiveTypeCache = new Hashtable();
        private static Hashtable CacheGetTableInfo=new Hashtable();

        internal static bool CheckIsBoolField(Expression expr)
        {
            if(expr is MemberExpression)
            {
                var mb = expr as MemberExpression;
                if(mb.Member is FieldInfo)
                {
                    var f = mb.Member as FieldInfo;
                    var ft = Nullable.GetUnderlyingType(f.FieldType) ?? f.FieldType;
                    return ft == typeof(bool);

                }
                else if (mb.Member is PropertyInfo)
                {
                    var f = mb.Member as PropertyInfo;
                    var ft = Nullable.GetUnderlyingType(f.PropertyType) ?? f.PropertyType;
                    return ft == typeof(bool);

                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            return false;
        }

        /// <summary>
        /// This method Load Assembly from dll_file in directory
        /// </summary>
        /// <param name="directory">Absolute path to directory where contains dll_file</param>
        /// <param name="dll_file">dll_file</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough]
        internal static System.Reflection.Assembly LoadFile(string directory, string dll_file)
        {
            System.Reflection.Assembly ass = null;

#if NET_CORE
            // Requires nuget - System.Runtime.Loader
            ass = System.Runtime.Loader.AssemblyLoadContext.Default
                   .LoadFromAssemblyPath(Path.Combine(directory,dll_file);
#else
            ass = System.Reflection.Assembly.LoadFile(directory + "\\" + dll_file);
#endif 
            // System.Type myType = ass.GetType("Custom.Thing.SampleClass");
            // object myInstance = Activator.CreateInstance(myType);
            return ass;
        }
       [System.Diagnostics.DebuggerStepThrough]
        internal static ParamConst Resolve(Expression expr)
        {
            try
            {
                return new ParamConst
                {
                    Value = Expression.Lambda(expr).Compile().DynamicInvoke(),
                    Type = expr.Type
                };
            }
            catch(MemberAccessException ex)
            {
                throw new CompileException("It looks like you try to resolve invalid expression. " + ex.Message);
            }
            catch(ArgumentException ex)
            {
                throw new CompileException(string.Format("The arguments in '{0}' can not resolve.",expr)  + ex.Message);
            }
            catch(TargetInvocationException ex)
            {
                throw new CompileException(string.Format("The expression '{0}' is relative to an action or a function canot resolve.", expr) + ex.Message);
            }
            catch(Exception ex)
            {
                throw (ex);
            }
        }

        internal static MapAlais FindAliasByExpression(List<MapAlais> mapAlais, Expression expr)
        {
            if (expr is ParameterExpression)
            {
                var ret = mapAlais.FirstOrDefault(p => p.ParamExpr == expr);
                if (ret != null)
                {
                    return ret;
                }
                else
                {
                    return null;
                }
            }
            else if (expr is MemberExpression)
            {
                return FindAliasByExpression(mapAlais, ((MemberExpression)expr).Expression);
            }
            else if(expr is ConstantExpression)
            {
                return null;
            }
            else
            {
                throw new NotImplementedException();
            }
        }



        internal static List<MemberExpression> GetAllMemberExpression(Expression expr)
        {
            var ret = new List<MemberExpression>();
            if (expr is UnaryExpression)
            {
                var ux = expr as UnaryExpression;
                var tmp = GetAllMemberExpression(ux.Operand);
                ret.AddRange(tmp);
            }
            if (expr is MemberExpression)
            {
                ret.Add((MemberExpression)expr);
                ret.AddRange(GetAllMemberExpression(((MemberExpression)expr).Expression));
            }
            else if (expr is MethodCallExpression)
            {
                foreach (var arg in ((MethodCallExpression)expr).Arguments)
                {
                    ret.AddRange(GetAllMemberExpression(arg));
                }
            }
            else if (expr is LambdaExpression)
            {
                ret.AddRange(GetAllMemberExpression(((LambdaExpression)expr).Body));
            }
            else if (expr is BinaryExpression)
            {
                ret.AddRange(GetAllMemberExpression(((BinaryExpression)expr).Left));
                ret.AddRange(GetAllMemberExpression(((BinaryExpression)expr).Right));
            }
            else if (expr is ParameterExpression)
            {
                return ret;
            }
            else if (expr is UnaryExpression)
            {
                ret.AddRange(GetAllMemberExpression(((UnaryExpression)expr).Operand));
            }
            else if (expr is ConstantExpression)
            {
                return ret;
            }
            else if (expr is NewArrayExpression)
            {
                var nx = expr as NewArrayExpression;
                nx.Expressions.ToList().ForEach(p =>
                {
                    ret.AddRange(GetAllMemberExpression(p));
                });
            }
            else if (expr is MemberInitExpression)
            {
                var nx = expr as MemberInitExpression;
                nx.Bindings.Cast<MemberAssignment>().ToList().ForEach(p =>
                {
                    ret.AddRange(GetAllMemberExpression(p.Expression));
                });
            }
            else if (expr is NewExpression)
            {
                var nx = expr as NewExpression;
                nx.Arguments.ToList().ForEach(p =>
                {
                    ret.AddRange(GetAllMemberExpression(p));
                });
            }
            else
            {
                throw new NotImplementedException();
            }
            return ret;
        }
        [System.Diagnostics.DebuggerStepThrough]
        internal static TableInfo GetTableInfo<T>(ICompiler Cmp, string schema, string table_name)
        {
            if (CacheGetTableInfo[typeof(T).FullName] == null)
            {
                lock (objLockCacheGetTableInfo)
                {
                    if (CacheGetTableInfo[typeof(T).FullName] == null)
                    {
                        var ret = new TableInfo();
                        ret.name = table_name;
                        ret.Columns = new List<ColumInfo>();
                        var pkText = "";
                        var pkAttr = typeof(T).GetCustomAttribute<PrimaryKeysAttribute>();
                        if (pkAttr != null)
                        {
                            pkText = "," + pkAttr.Columns + ",";
                        }
                        foreach (var P in typeof(T).GetProperties())
                        {
                            var col = new ColumInfo()
                            {
                                TableName = table_name,
                                Name = P.Name,
                                ColumnSize = Cmp.GetMaxTextLen()
                            };
                            var uType = Nullable.GetUnderlyingType(P.PropertyType);
                            if (P.PropertyType != uType)
                            {
                                col.AllowDBNull = true;
                            }

                            var colAttr = P.GetCustomAttribute<DataTableColumnAttribute>();
                            if (colAttr != null)
                            {
                                if (colAttr.AutoInc)
                                {
                                    col.IsAutoIncrement = true;
                                    col.IsIdentity = true;
                                    col.IsExpression = true;
                                    col.IsReadOnly = true;

                                }
                                col.ColumnSize = (colAttr.Len <= 0) ? Cmp.GetMaxTextLen() : colAttr.Len;

                                if (colAttr.IsRequire)
                                {
                                    col.AllowDBNull = false;
                                }
                            }
                            if (pkText.IndexOf($",{P.Name},") > -1)
                            {
                                col.IsKey = true;
                            }
                            ret.Columns.Add(col);
                        }
                        CacheGetTableInfo[typeof(T).FullName] = ret;
                    }
                }
            }

            return CacheGetTableInfo[typeof(T).FullName] as TableInfo;
        }

        internal static List<ParameterExpression> GetAllParamsExpr(Expression expr)
        {
            var ret = new List<ParameterExpression>();
            if (expr is LambdaExpression)
            {
                ret.AddRange(GetAllParamsExpr(((LambdaExpression)expr).Body));
            }
            else if (expr is BinaryExpression)
            {
                var bx = expr as BinaryExpression;
                ret.AddRange(GetAllParamsExpr(bx.Left));
                ret.AddRange(GetAllParamsExpr(bx.Right));
            }
            else if (expr is MemberExpression)
            {
                var mb = expr as MemberExpression;
                ret.AddRange(GetAllParamsExpr(mb.Expression));
            }
            else if (expr is ParameterExpression)
            {
                ret.Add((ParameterExpression)expr);
            }
            else if (expr is MethodCallExpression)
            {
                var mc = expr as MethodCallExpression;
                foreach (var x in mc.Arguments)
                {
                    ret.AddRange(GetAllParamsExpr(x));
                }
            }
            else if (expr is UnaryExpression)
            {
                var ux = expr as UnaryExpression;
                ret.AddRange(GetAllParamsExpr(ux.Operand));

            }
            else if (expr is NewArrayExpression)
            {
                var nxa = expr as NewArrayExpression;
                foreach (var x in nxa.Expressions)
                {
                    ret.AddRange(GetAllParamsExpr(x));
                }
            }
            else if (expr is ConstantExpression)
            {
                return ret;
            }
            else
            {
                throw new NotImplementedException();
            }
            return ret;
        }

        internal static PropertyMapping FindMapping(List<PropertyMapping> mapping, Expression expr)
        {
            if (expr is MemberExpression)
            {
                var ret = mapping.FirstOrDefault(p => p.Property == ((PropertyInfo)((MemberExpression)expr).Member));
                if (ret != null)
                {
                    return ret;
                }
                else
                {
                    ret = FindMapping(mapping, ((MemberExpression)expr).Expression);
                    return ret;
                }
            }
            return null;
        }
        [System.Diagnostics.DebuggerStepThrough]
        internal static bool IsInherit(Type type, Type BaseType)
        {
            if (type == typeof(Object))
            {
                return false;
            }
            if (type == BaseType)
            {
                return true;
            }
            else
            {
                if (type.BaseType != null)
                {
                    return IsInherit(type.BaseType, BaseType);
                }
                else
                {
                    return false;
                }
            }
        }

        internal static MapAlais FindOriginTableByMember(List<MapAlais> mapAlias, MemberInfo member)
        {
            var ret = mapAlias.FirstOrDefault(p => p.Member == member && !string.IsNullOrEmpty(p.TableName));
            if (ret != null)
            {
                return ret;
            }
            else
            {
                foreach (var x in mapAlias)
                {
                    if (x.Children != null)
                    {
                        ret = FindOriginTableByMember(x.Children, member);
                        if (ret != null)
                        {
                            return ret;
                        }
                    }
                }
                return ret;
            }
        }

        internal static bool IsConstantExpr(MemberExpression mx)
        {
            if (mx.Expression is MemberExpression)
            {
                return IsConstantExpr((MemberExpression)mx.Expression);
            }
            else if (mx.Expression is ConstantExpression)
            {

                return true;
            }
            else if (mx.Expression is ParameterExpression)
            {
                return false;
            }
            else if (mx.Expression == null)
            {
                return true;
            }
            else if(mx.Expression is MethodCallExpression)
            {
                var mc = mx.Expression as MethodCallExpression;
                return mc.Method.ReflectedType != typeof(SqlFuncs);
            }
            else
            {
                throw new NotImplementedException();
            }


        }

        internal static string GetOp(ExpressionType NodeType)
        {
            if (NodeType == ExpressionType.Add)
            {
                return "+";
            }
            if (NodeType == ExpressionType.Multiply)
            {
                return "*";
            }
            if (NodeType == ExpressionType.Equal)
            {
                return "=";
            }
            if (NodeType == ExpressionType.NotEqual)
            {
                return "!=";
            }
            if (NodeType == ExpressionType.AndAlso)
            {
                return "and";
            }
            if (NodeType == ExpressionType.OrElse)
            {
                return "or";
            }
            if (NodeType == ExpressionType.GreaterThan)
            {
                return ">";
            }
            if (NodeType == ExpressionType.GreaterThanOrEqual)
            {
                return ">=";
            }
            if (NodeType == ExpressionType.LessThan)
            {
                return "<";
            }
            if (NodeType == ExpressionType.LessThanOrEqual)
            {
                return "<=";
            }
            if (NodeType == ExpressionType.Not)
            {
                return "Not";
            }
            if (NodeType == ExpressionType.Quote)
            {
                return null;
            }
            if (NodeType == ExpressionType.Convert)
            {
                return null;
            }
            throw new NotImplementedException();
        }

        internal static MapAlais FindAliasByPropertyType(List<MapAlais> mapping, Type propertyType)
        {
            var ret = mapping.Where(p => p.Member != null && p.Member is PropertyInfo && ((PropertyInfo)p.Member).PropertyType == propertyType).FirstOrDefault();
            if (ret != null)
            {
                return ret;
            }
            else
            {
                foreach (var x in mapping)
                {
                    if (x.Children != null)
                    {
                        ret = FindAliasByPropertyType(x.Children, propertyType);
                        if (ret != null)
                        {
                            return ret;
                        }
                    }
                }
                return null;
            }
        }
        internal static MapAlais FindAliasByParameterType(List<MapAlais> mapping, Type propertyType)
        {
            var ret = mapping.Where(p => p.ParamExpr != null && p.ParamExpr.Type == propertyType).FirstOrDefault();
            if (ret != null)
            {
                return ret;
            }
            else
            {
                foreach (var x in mapping)
                {
                    if (x.Children != null)
                    {
                        ret = FindAliasByParameterType(x.Children, propertyType);
                        if (ret != null)
                        {
                            return ret;
                        }
                    }
                }
                return null;
            }
        }
        internal static string RepairParameters(string txt, List<object> params1, List<object> params2)
        {
            var ret = txt;
            var index = 0;
            foreach (var p in params2)
            {
                var oldIndex = index;
                var newIndex = params1.Count + index;
                ret = ret.Replace("{" + oldIndex + "}", "{$##" + newIndex + "##$}");
                index++;
            }
            return ret.Replace("{$##", "{").Replace("##$}", "}");
        }

        internal static string GetSource(BaseQuerySet q1, string Alias)
        {
            var Cmp = q1.compiler;
            if (!string.IsNullOrEmpty(q1.datasource))
            {
                if (q1.unionSQLs.Count>0)
                {

                }
                return q1.datasource;
            }
            if (q1.fields.Count > 0 || (!string.IsNullOrEmpty(q1.where)))
            {
                return "(" + q1.ToSQLString() + ")";
            }
            else
            {
                var ret = Cmp.GetQName(q1.function_Schema ?? q1.Schema, q1.function_call ?? q1.table_name);
                if (!string.IsNullOrEmpty(q1.function_call))
                {
                    ret += " as " + Cmp.GetQName("", Alias);
                    return ret;
                }
                return ret;
            }
            throw new NotImplementedException();
        }

        internal static bool CheckIfPrimitiveType(Type declaringType)
        {
            if (CheckIfPrimitiveTypeCache[declaringType.FullName] == null)
            {
                lock (objCheckIfPrimitiveTypeCache)
                {
                    if (CheckIfPrimitiveTypeCache[declaringType.FullName] == null)
                    {
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
                                CheckIfPrimitiveTypeCache[declaringType.FullName] = false;
                                return (bool)CheckIfPrimitiveTypeCache[declaringType.FullName];
                            }
                        }
                        CheckIfPrimitiveTypeCache[declaringType.FullName] = false;
                        return (bool)CheckIfPrimitiveTypeCache[declaringType.FullName];

                    }
                }
            }
            return (bool)(CheckIfPrimitiveTypeCache[declaringType.FullName]);
        }

        internal static List<PropertyInfo> GetAllPropertiesOfType(Type type)
        {


            var ret = new List<PropertyInfo>();
            var retProPer = type.GetProperties().ToList();
            retProPer.ForEach(p =>
            {
                ret.AddRange(GetAllProperties(p));
            });
            ret.AddRange(retProPer);
            ret = ret.Where(p => !CheckIfPrimitiveType(p.PropertyType)).ToList();
            return ret;
        }

        internal static bool CheckIfAnonymousType(Type type)
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
            return type.FullName.IndexOf("<>f__AnonymousType") == 0;
        }

        internal static bool CheckTypeIsMemberOfType(Type type, Type reflectedType)
        {
            if (type == reflectedType)
            {
                return true;
            }
            else
            {
                foreach (var p in type.GetProperties().Where(x => x.PropertyType != type))
                {
                    var ret = CheckTypeIsMemberOfType(p.PropertyType, reflectedType);
                    if (ret)
                    {
                        return ret;
                    }
                }
            }
            return false;
        }

        internal static List<MapAlais> CreateMemberMapping(Expression expr, ParameterExpression LeftParam, ParameterExpression RightParam, string LeftTableName, string RightTableName, MemberInfo member = null)
        {
            var ret = new List<MapAlais>();
            if (expr is NewExpression)
            {
                var nx = expr as NewExpression;
                var argIndex = 0;
                foreach (var arg in nx.Arguments)
                {
                    if (arg is ParameterExpression)
                    {
                        var tmp = CreateMemberMapping(arg, LeftParam, RightParam, LeftTableName, RightTableName, nx.Members[argIndex]);
                        ret.AddRange(tmp);
                    }
                    else if (arg is MemberExpression)
                    {
                        var tmp = CreateMemberMapping(((MemberExpression)arg).Expression, LeftParam, RightParam, LeftTableName, RightTableName, nx.Members[argIndex]);
                        ret.AddRange(tmp);
                    }
                    else if (arg is BinaryExpression)
                    {
                        var bx = arg as BinaryExpression;
                        var tmp1 = CreateMemberMapping(bx.Left, LeftParam, RightParam, LeftTableName, RightTableName, nx.Members[argIndex]);
                        var tmp2 = CreateMemberMapping(bx.Right, LeftParam, RightParam, LeftTableName, RightTableName, nx.Members[argIndex]);
                        ret.AddRange(tmp1);
                        ret.AddRange(tmp2);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    argIndex++;
                }
            }
            else if (expr is ParameterExpression)
            {
                if (expr == LeftParam)
                {
                    ret.Add(new MapAlais()
                    {
                        AliasName = LeftTableName,
                        ParamExpr = LeftParam
                    });
                }
                else if (expr == RightParam)
                {
                    ret.Add(new MapAlais()
                    {
                        AliasName = RightTableName,
                        ParamExpr = RightParam
                    });
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (expr is BinaryExpression)
            {
                var bx = expr as BinaryExpression;
                var tmp1 = CreateMemberMapping(bx.Left, LeftParam, RightParam, LeftTableName, RightTableName, member);
                var tmp2 = CreateMemberMapping(bx.Right, LeftParam, RightParam, LeftTableName, RightTableName, member);
                ret.AddRange(tmp1);
                ret.AddRange(tmp2);
            }
            else if (expr is MemberExpression)
            {
                var mx = expr as MemberExpression;
                var tmp = CreateMemberMapping(mx.Expression, LeftParam, RightParam, LeftTableName, RightTableName, member);
                ret.AddRange(tmp);
            }
            else if (expr is ConstantExpression)
            {
                return new List<MapAlais>();
            }
            else
            {
                throw new NotImplementedException();
            }
            return ret;
        }
        [System.Diagnostics.DebuggerStepThrough]
        internal static string BuildJoinExpr(QueryJoinObject JO)
        {
            // var L1 = ((QueryJoinObject)JO.R).L + " " + JO.C + " ";
            //var RR = " " + L1 + ((QueryJoinObject)JO.R).J + ((QueryJoinObject)((QueryJoinObject)JO.R).R).L + " " + ((LV.Db.Common.QueryJoinObject)JO.R).C + " " + ((LV.Db.Common.QueryJoinObject)((LV.Db.Common.QueryJoinObject)JO.R).R).J + " " + ((LV.Db.Common.QueryJoinObject)((LV.Db.Common.QueryJoinObject)JO.R).R).R + " ";
            // var FX = JO.L + " " + JO.J + RR + ((LV.Db.Common.QueryJoinObject)((LV.Db.Common.QueryJoinObject)JO.R).R).C;
            if ((JO.L is string) && (JO.R is string))
            {
                return JO.L + " " + JO.J + " " + JO.R + " " + JO.C;
            }
            else if ((JO.L is string) && (JO.R is QueryJoinObject))
            {
                var R = JO.R as QueryJoinObject;
                //(R.L + JO.C + R.J + R.R)
                var fx = JO.L + " " + JO.J + " " + R.L + " " + JO.C + " " + R.J + R.R + " " + R.C;
                var strR = GetExprFromJoinObject(new QueryJoinObject()
                {
                    L = R.L,
                    J = R.J,
                    C = JO.C,
                    R = R.R
                });
                var ret = string.Format("{0} {1} {2} {3}", JO.L, JO.J, strR, R.C);
                return ret;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal static MemberExpression FindMember(Expression x)
        {
            if (x is MemberExpression)
            {
                var ret = FindMember(((MemberExpression)x).Expression);
                if (ret != null)
                {
                    return ret;
                }
                return x as MemberExpression;
            }
            return null;
        }

        private static object GetExprFromJoinObject(QueryJoinObject JO)
        {
            if ((JO.L is string) && (JO.R is QueryJoinObject))
            {
                var R = JO.R as QueryJoinObject;
                //(R.L + JO.C + R.J + R.R)
                var fx = JO.L + " " + JO.J + " " + R.L + " " + JO.C + " " + R.J + R.R + " " + R.C;
                var strR = GetExprFromJoinObject(new QueryJoinObject()
                {
                    L = R.L,
                    J = R.J,
                    C = JO.C,
                    R = R.R
                });
                var ret = string.Format("{0} {1} {2} {3}", JO.L, JO.J, strR, R.C);
                return ret;
            }
            return JO.L + " " + JO.C + " " + JO.J + JO.R;
        }




        internal static bool CheckIsExpressionHasAliasOrBinaryExpression(Expression expr)
        {
            if (expr is NewExpression)
            {
                var nx = expr as NewExpression;
                foreach (var x in nx.Arguments)
                {
                    if (x is MemberExpression)
                    {
                        if (((MemberExpression)x).Member.Name != nx.Members[nx.Arguments.IndexOf(x)].Name)
                        {
                            return true;
                        }
                    }
                    else if (x is BinaryExpression)
                    {
                        return true;
                    }
                }
                return false;
            }
            if (expr is MemberInitExpression)
            {
                var mbi = expr as MemberInitExpression;
                foreach (var x in mbi.NewExpression.Arguments)
                {

                    if (x is MemberExpression)
                    {
                        if (((MemberExpression)x).Member != mbi.NewExpression.Members[mbi.NewExpression.Arguments.IndexOf(x)])
                        {
                            return true;
                        }
                    }
                    else if (x is BinaryExpression)
                    {
                        return true;
                    }
                }
                return false;
            }
            if (expr is MemberExpression)
            {
                return false;
            }
            if (expr is ParameterExpression)
            {
                return false;
            }
            if (expr is MethodCallExpression)
            {
                return true;
            }
            if (expr is BinaryExpression)
            {
                return true;
            }
            throw new NotImplementedException();
        }
        [System.Diagnostics.DebuggerStepThrough]
        internal static ParameterExpression FindParameterExpression(MemberExpression expr)
        {
            if (expr.Expression is ParameterExpression)
            {
                return expr.Expression as ParameterExpression;
            }
            else if (expr.Expression is MemberExpression)
            {
                return FindParameterExpression((MemberExpression)expr.Expression);
            }
            else
            {
                return null;
            }
        }

        internal static MapAlais FindAliasByMemberExpression(List<MapAlais> mapAlias, Expression mb)
        {
            if (mb is MemberExpression)
            {
                var ret = FindAliasByMember(mapAlias, ((MemberExpression)mb).Member);
                if (ret != null)
                {
                    return ret;
                }
                else
                {
                    return FindAliasByMemberExpression(mapAlias, ((MemberExpression)mb).Expression);

                }
            }
            else
            {
                return null;
            }
        }

        // [System.Diagnostics.DebuggerStepThrough]
        internal static MapAlais FindAliasByMember(List<MapAlais> mapAlias, MemberInfo member)
        {
            if (mapAlias == null)
            {
                return null;
            }
            var ret = mapAlias.FirstOrDefault(p => member != null && p.Member == member);
            var lst = mapAlias.Where(p => member != null && p.Member == member).ToList();
            if (ret != null)
            {
                return ret;
            }
            else
            {
                if (mapAlias != null)
                {
                    foreach (var x in mapAlias)
                    {
                        if (x.Children != null)
                        {
                            ret = FindAliasByMember(x.Children.Where(p => p != x).ToList(), member);
                            if (ret != null)
                            {
                                return ret;
                            }
                        }
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
        }

        internal static List<string> GetSelectAll(Type type, List<PropertyMapping> propertiesMapping)
        {
            var extraFields = type.GetProperties().Join(propertiesMapping, p => p, q => q.Property, (p, q) => q)
                                    .Select(p => p.AliasName).ToList();
            foreach (var p in type.GetProperties())
            {
                if (!CheckIfPrimitiveType(p.PropertyType))
                {
                    extraFields.AddRange(GetSelectAll(p.PropertyType, propertiesMapping));
                }
            }
            return extraFields;
        }

        //[System.Diagnostics.DebuggerStepThrough]
        internal static List<string> GetTables(MapAlais mapItem, MemberInfo member)
        {
            if (mapItem.Children != null)
            {
                mapItem.Children = mapItem.Children.Where(p => p != mapItem).ToList();
            }
            if (mapItem.Children == null)
            {
                if (mapItem.AliasName != null)
                {
                    return new List<string>(new string[] { mapItem.AliasName });
                }
                return new List<string>();

            }
            else
            {
                var ret = new List<string>();
                if (member == null)
                {
                    foreach (var x in mapItem.Children.Where(p => p != mapItem).ToList())
                    {
                        ret.AddRange(GetTables(x, member));
                    }
                }
                else
                {
                    var mitem = mapItem.Children.FirstOrDefault(x => x.Member == member);

                    if (mitem != null)
                    {
                        if (mitem.Children == null)
                        {
                            return new List<string>(new string[] { mitem.AliasName });
                        }
                        else
                        {

                        }
                        foreach (var x in mitem.Children)
                        {
                            ret.AddRange(GetTables(x, member));
                        }
                        return ret;
                    }
                    foreach (var x in mapItem.Children)
                    {
                        var r = GetTables(x, member);
                        if (r != null)
                        {
                            ret.AddRange(r);
                        }

                    }
                    ret = ret.GroupBy(p => p).Select(p => p.Key).ToList();
                }
                if (member is PropertyInfo && ret.Count>0)
                {
                    if (utils.CheckIfPrimitiveType(((PropertyInfo)member).PropertyType))
                    {
                        var maxLen = ret.Max(p => p.Length);
                        return ret.Where(p => p.Length == maxLen).ToList();


                    }
                }
                return ret;
            }

        }

        internal static List<PropertyInfo> GetAllProperties(PropertyInfo propertyInfo)
        {
            var ret = new List<PropertyInfo>();
            var retP = propertyInfo.PropertyType.GetProperties().Where(p => !CheckIfPrimitiveType(Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType)).ToList();
            ret.AddRange(retP);
            foreach (var x in retP)
            {
                ret.AddRange(GetAllProperties(x));
            }
            return ret;
        }
    }
}
