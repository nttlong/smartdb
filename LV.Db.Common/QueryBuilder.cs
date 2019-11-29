using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LV.Db.Common
{
    /// <summary>
    /// Queryabl instance generator
    /// </summary>
    internal class QueryBuilder
    {


        internal static QuerySet<T> Join<T1, T2, T>(QuerySet<T1> qr1, QuerySet<T2> qr2, Expression<Func<T1, T2, bool>> JoinClause, Expression<Func<T1, T2, T>> selecttor)
        {
            if (JoinClause.Body is ConstantExpression)
            {
                var ret = utils.Resolve(JoinClause.Body);
                if (ret.Value is bool && (bool)ret.Value == true)
                {
                    return BuilJoin(qr1, qr2, JoinClause, selecttor, "");
                }
            }
            return BuilJoin(qr1, qr2, JoinClause, selecttor, "inner");

        }

        internal static QuerySet<T> MakeUnion<T, T1>(QuerySet<T> qr1, QuerySet<T1> qr2, bool IsUnionAll)
        {
            var ret = qr1.Clone<T>();
            ret.where = string.Empty;
            ret.groupbyList.Clear();
            ret.having = string.Empty;
            ret.sortList.Clear();
            var sql1 = qr1.ToSQLString();
            var sql2 = qr2.ToSQLString();
            var index = 0;
            foreach (var p in qr2.Params)
            {
                var oldIndex = index;
                var newIndex = qr1.Params.Count + index;
                sql2 = sql2.Replace("{" + oldIndex + "}", "{$$##" + newIndex + "##$$}");
                ret.Params.Add(p);
                index++;
            }
            sql2 = sql2.Replace("{$$##", "{").Replace("##$$}", "}");
            if (ret.unionSQLs.Count == 0)
            {
                ret.unionSQLs.Add(new UnionSQLInfo()
                {
                    Sql = sql1
                });
                ret.unionSQLs.Add(new UnionSQLInfo()
                {
                    Sql = sql2,
                    IsUnionAll = IsUnionAll
                });
            }
            else
            {
                //ret.unionSQLs.LastOrDefault().IsUnionAll = true;
                ret.unionSQLs.Add(new UnionSQLInfo()
                {
                    Sql = sql2,
                    IsUnionAll = IsUnionAll
                });
            }
            if ((!qr2.IsSubQueryOnNextPhaseWithSelectedFields()) &&
                (!qr1.IsSubQueryOnNextPhaseWithSelectedFields()))
            {
                ret.table_name = "u" + ret.unionSQLs.Count;
            }

            return ret;

        }



        internal static QuerySet<T> UnionAll<T, T1>(QuerySet<T> qr1, QuerySet<T1> qr2)
        {
            return MakeUnion(qr1, qr2, true);

        }
        public static QuerySet<T> Union<T, T1>(QuerySet<T> qr1, QuerySet<T1> qr2)
        {
            return MakeUnion(qr1, qr2, false);

        }

        public static QuerySet<T> LeftJoin<T1, T2, T>(QuerySet<T1> qr1, QuerySet<T2> qr2, Expression<Func<T1, T2, bool>> JoinClause, Expression<Func<T1, T2, T>> selecttor)
        {
            return BuilJoin(qr1, qr2, JoinClause, selecttor, "left");

        }

        public static QuerySet<T> FromNothing<T>(ICompiler Provider, Expression<Func<object, T>> Selector)
        {
            var ret = new QuerySet<T>(Provider);
            var Params = new List<object>();
            ret.fields = Compiler.RawCompile(Provider, Selector.Body, ref Params);

            //SelectorCompiler.GetFields(Selector.Body, new string[] { }, new string[] { }, Selector.Parameters.Cast<object>().ToList(), ref Params);
            ret.isFromNothing = true;
            ret.Params.AddRange(Params);
            ret.isReadOnly = true;
            return ret;
        }
        internal static string GetTableName<T>()
        {
#if NET_CORE
            var TableAttr = typeof(T).CustomAttributes.Where(p => p.AttributeType == typeof(QueryDataTableAttribute)).FirstOrDefault();
#else
            var TableAttr = typeof(T).GetCustomAttributes(false).Where(p => p is QueryDataTableAttribute).FirstOrDefault();
#endif

            if (TableAttr is null)
            {
                throw (new Exception(string.Format("It looks like you forgot set DataTable for class {0}", typeof(T).FullName)));
            }
#if NET_CORE
            return new QuerySet<T>(schema, TableAttr.ConstructorArguments[0].Value.ToString());
#else
            return ((QueryDataTableAttribute)TableAttr).TableName;
#endif
        }
        [System.Diagnostics.DebuggerStepThrough]
        public static QuerySet<T> FromEntity<T>(ICompiler Provider, string schema)
        {
#if NET_CORE
            var TableAttr = typeof(T).CustomAttributes.Where(p => p.AttributeType == typeof(QueryDataTableAttribute)).FirstOrDefault();
#else
            var TableAttr = typeof(T).GetCustomAttributes(false).Where(p => p is QueryDataTableAttribute).FirstOrDefault();
#endif

            if (TableAttr is null)
            {
                throw (new Exception(string.Format("It looks like you forgot set DataTable for class {0}", typeof(T).FullName)));
            }
#if NET_CORE
            return new QuerySet<T>(schema, TableAttr.ConstructorArguments[0].Value.ToString());
#else
            return new QuerySet<T>(Provider, schema, ((QueryDataTableAttribute)TableAttr).TableName);
#endif
        }
       // [System.Diagnostics.DebuggerStepThrough]
        internal static QuerySet<T> FromEntity<T>(ICompiler Provider)
        {
            return FromEntity<T>(Provider, "");
        }
        internal static QuerySet<T> FromEntity<T>(ICompiler Provider, bool inogreCheck)
        {
            return new QuerySet<T>(Provider, "", "");
        }
        public static QuerySet<T> RightJoin<T1, T2, T>(QuerySet<T1> qr1, QuerySet<T2> qr2, Expression<Func<T1, T2, bool>> JoinClause, Expression<Func<T1, T2, T>> selecttor)
        {
            return BuilJoin(qr1, qr2, JoinClause, selecttor, "right");

        }



        internal static QuerySet<T> From<T>(ICompiler Provider, string schema, string tableName)
        {
            return new QuerySet<T>(Provider, schema, tableName);
        }

        private static QuerySet<T> BuilJoin<T1, T2, T>(QuerySet<T1> qr1, QuerySet<T2> qr2, Expression<Func<T1, T2, bool>> JoinClause, Expression<Func<T1, T2, T>> selector, string JoinType)
        {
            return BuilJoinFromBase<T>(qr1, qr2, JoinClause, selector, JoinType);
        }
        //[System.Diagnostics.DebuggerStepThrough]
        internal static QuerySet<T> BuilJoinFromBase<T>(BaseQuerySet qr1, BaseQuerySet qr2, List<object> joinParams, List<string> leftKeys, List<string> rightKeys, Expression Selector, List<MapAlais> ExtraAlaias, bool isLeftJoin, bool ByLinq = false)
        {

            var Cmp = qr1.compiler;
            var JoinType = (isLeftJoin) ? "left" : "inner";
            var ret = new QuerySet<T>(true);
            var tempMapAlias = new List<MapAlais>();
            tempMapAlias.AddRange(qr1.mapAlias);
            tempMapAlias.AddRange(qr2.mapAlias);
            ret.joinTimes = (qr1.joinTimes + 1) * 10 + qr2.joinTimes;
            var ltName = "l" + ret.joinTimes.ToString();
            var rtName = "r" + ret.joinTimes.ToString();
            if (!string.IsNullOrEmpty(qr1.table_name))
            {
                ret.tableAlias.Add(new TableAliasInfo()
                {

                    Schema = qr1.Schema,
                    TableName = qr1.table_name,
                    Alias = ltName
                });
            }
            ret.PropertiesMapping.AddRange(qr1.PropertiesMapping);
            ret.PropertiesMapping.AddRange(qr2.PropertiesMapping);
            ret.FieldsMapping = qr1.FieldsMapping.Union(qr2.FieldsMapping).ToList();

            ret.tableAlias.AddRange(qr1.tableAlias);
            if (!string.IsNullOrEmpty(qr2.table_name))
            {
                ret.tableAlias.Add(new TableAliasInfo()
                {

                    Schema = qr2.Schema,
                    TableName = qr2.table_name,
                    Alias = rtName
                });
            }
            ret.tableAlias.AddRange(qr2.tableAlias);



            //var nx = selector.Body as NewExpression;

            var Params = new List<object>();
            Params.AddRange(qr1.Params);

            var strJoin = string.Join(" and ", leftKeys.Select(p => p + "=" + rightKeys[leftKeys.IndexOf(p)]).ToArray());
            strJoin = utils.RepairParameters(strJoin, Params, joinParams);
            Params.AddRange(joinParams);







            var leftSource = "";
            var rightSource = "";

            if (qr1.joinTimes == 0 && qr2.joinTimes == 0)
            {
                tempMapAlias.AddRange(createMapAlais(qr1, ltName));
                leftSource = (!qr1.IsSubQueryOnNextPhaseWithSelectedFields()) ? utils.GetSource(qr1, ltName) : ("(" + qr1.ToSQLString() + ")");
                rightSource = (!qr2.IsSubQueryOnNextPhaseWithSelectedFields()) ? utils.GetSource(qr2, rtName) : ("(" + qr2.ToSQLString() + ")");
                rightSource = utils.RepairParameters(rightSource, Params, qr2.Params);
                Params.AddRange(qr2.Params);
                ret.JoinObject = new QueryJoinObject()
                {
                    L = leftSource + " as " + Cmp.GetQName("", ltName.ToString()),
                    R = rightSource + " as " + Cmp.GetQName("", rtName.ToString()),
                    J = JoinType + " join ",
                    C = "on " + strJoin
                };
                string JoinExpr = utils.BuildJoinExpr(ret.JoinObject);
                ret.datasource = leftSource + " as " + Cmp.GetQName("", ltName.ToString()) +
                            " " + JoinType + " join " + rightSource + " as " + Cmp.GetQName("", rtName.ToString()) + " on " + strJoin;
            }
            if (qr1.joinTimes > 0 && qr2.joinTimes == 0)
            {
                tempMapAlias.AddRange(createMapAlais(qr1, ltName));
                leftSource = (!qr1.IsSubQueryOnNextPhaseWithSelectedFields()) ? utils.GetSource(qr1, ltName) : ("(" + qr1.ToSQLString() + ")");
                if (qr1.IsSubQueryOnNextPhase())
                {
                    leftSource = "(" + leftSource + ") as " + qr1.compiler.GetQName(ltName);
                }
                if (qr2.IsSubQueryOnNextPhase())
                {
                    rightSource = "(" + qr2.ToSQLString() + ") as " + qr1.compiler.GetQName(rtName);
                }
                else
                {
                    rightSource = (!qr2.IsSubQueryOnNextPhaseWithSelectedFields()) ? utils.GetSource(qr2, rtName) : ("(" + qr2.ToSQLString() + ")");
                }
                rightSource = utils.RepairParameters(rightSource, Params, qr2.Params);
                Params.AddRange(qr2.Params);
                
                ret.JoinObject = new QueryJoinObject()
                {
                    L = qr1.JoinObject,
                    R = rightSource + " as " + Cmp.GetQName("", rtName.ToString()),
                    J = JoinType + " join ",
                    C = "on " + strJoin
                };
                if (qr2.IsSubQueryOnNextPhase())
                {
                    ret.datasource = leftSource +
                                " " + JoinType + " join " + rightSource + " on " + strJoin;
                }
                else
                {
                    ret.datasource = leftSource +
                                " " + JoinType + " join " + rightSource + " as " + Cmp.GetQName("", rtName.ToString()) + " on " + strJoin;
                }
            }
            if (qr1.joinTimes == 0 && qr2.joinTimes > 0)
            {

                leftSource = (!qr1.IsSubQueryOnNextPhaseWithSelectedFields()) ? utils.GetSource(qr1, ltName) : ("(" + qr1.ToSQLString() + ")");
                rightSource = "(" + qr2.ToSQLString() + ")";
                rightSource = utils.RepairParameters(rightSource, Params, qr2.Params);
                Params.AddRange(qr2.Params);

                ret.JoinObject = new QueryJoinObject()
                {
                    L = leftSource + " as " + Cmp.GetQName("", ltName.ToString()),
                    R = qr2.JoinObject,
                    J = JoinType + " join ",
                    C = "on " + strJoin
                };


                ret.datasource = leftSource + " as " + Cmp.GetQName("", ltName) +
                            " " + JoinType + " join " + rightSource + " as " + Cmp.GetQName("", rtName) + " on " + strJoin;
            }
            if (qr1.joinTimes > 0 && qr2.joinTimes > 0)
            {


                ret.JoinObject = new QueryJoinObject()
                {
                    L = qr1.JoinObject,
                    R = qr2.JoinObject,
                    J = JoinType + " join ",
                    C = "on " + strJoin
                };
                leftSource = (!qr1.IsSubQueryOnNextPhaseWithSelectedFields()) ? utils.GetSource(qr1, ltName) : ("(" + qr1.ToSQLString() + ")");
                rightSource = "(" + qr2.ToSQLString() + ")";
                rightSource = utils.RepairParameters(rightSource, Params, qr2.Params);
                Params.AddRange(qr2.Params);
                ret.datasource = leftSource + " as " + Cmp.GetQName("", ltName) +
                            " " + JoinType + " join " + rightSource + " as " + Cmp.GetQName("", rtName) + " on " + strJoin;
            }



            //var refTables = new List<object>();
            var refParameters = new List<object>();

            if (qr1.fieldParams != null)
            {
                refParameters.AddRange(qr1.fieldParams);
            }
            if (qr2.fieldParams != null)
            {
                refParameters.AddRange(qr2.fieldParams);
            }
            var retFields = new List<QueryFieldInfo>();

            var exprWithExtraFields = new List<ExtraSelectAll>();
            var fi = Selector as UnaryExpression;
            var cx = fi.Operand as LambdaExpression;
            List<QueryMemberInfo> fieldMemberList = null;
            if (cx.Body is MemberInitExpression)
            {
                var mx = cx.Body as MemberInitExpression;
                foreach (MemberAssignment x in mx.Bindings)
                {
                    var argIndex = mx.Bindings.IndexOf(x);
                    MemberExpression tmb = utils.FindMember(x.Expression);
                    var mb = x.Member;
                    PropertyMapping PP = null;
                    if (tmb != null)
                    {
                        PP = ret.PropertiesMapping.FirstOrDefault(p => p.Property == tmb.Member);
                    }
                    var checkP = ret.PropertiesMapping.FirstOrDefault(p => p.Property == mb);
                    if (checkP == null && ret.PropertiesMapping.Count > 0)
                    {

                    }
                    var P = x.Expression;

                    if (PP != null)
                    {
                        tempMapAlias.Add(new MapAlais()
                        {
                            AliasName = PP.AliasName,
                            Member = mb
                        });
                    }
                    if (P is MemberExpression)
                    {
                        P = ((MemberExpression)P).Expression;
                    }
                    if (P == cx.Parameters[0])
                    {
                        tempMapAlias.AddRange(P.Type.GetProperties().Select(p => new MapAlais()
                        {
                            Member = p,
                            AliasName = ltName
                        }));
                        tempMapAlias.Add(new MapAlais()
                        {
                            AliasName = (PP != null) ? PP.OriginAliasName : ltName,
                            Member = (tmb != null) ? tmb.Member : mb,
                            ParamExpr = cx.Parameters[0],
                            Children = qr1.mapAlias,

                        });
                        ret.PropertiesMapping.Add(new PropertyMapping()
                        {
                            AliasName = (PP != null) ? PP.OriginAliasName : ltName,
                            OriginAliasName = (PP == null) ? ltName : PP.OriginAliasName,
                            TableName = (PP == null) ? qr1.table_name : PP.TableName,
                            Schema = (PP == null) ? qr1.Schema : PP.Schema,
                            Property = typeof(T).GetProperty(mb.Name)
                        });
                    }
                    if (P == cx.Parameters[1])
                    {
                        tempMapAlias.AddRange(P.Type.GetProperties().Select(p => new MapAlais()
                        {
                            Member = p,
                            AliasName = rtName
                        }));
                        if (qr2.joinTimes == 0)
                        {
                            tempMapAlias.Add(new MapAlais()
                            {
                                AliasName = rtName,
                                Member = (tmb != null) ? tmb.Member : mb,
                                ParamExpr = cx.Parameters[1],
                                Children = qr2.mapAlias,

                            });
                        }
                        else
                        {
                            tempMapAlias.Add(new MapAlais()
                            {
                                AliasName = rtName,
                                ParamExpr = cx.Parameters[1],
                            });
                        }
                        ret.PropertiesMapping.Add(new PropertyMapping()
                        {
                            AliasName = rtName,
                            OriginAliasName = (PP == null) ? rtName : PP.OriginAliasName,
                            TableName = (PP == null) ? qr2.table_name : PP.TableName,
                            Schema = (PP == null) ? qr2.Schema : PP.Schema,
                            Property = typeof(T).GetProperty(mb.Name)
                        });
                    }
                }

            }


            if (cx.Body is NewExpression)
            {
                var mx = cx.Body as NewExpression;
                foreach (var x in mx.Arguments)
                {
                    var argIndex = mx.Arguments.IndexOf(x);

                    MemberExpression tmb = utils.FindMember(x);
                    var mb = mx.Members[argIndex];
                    PropertyMapping PP = null;
                    if (tmb != null)
                    {
                        PP = ret.PropertiesMapping.FirstOrDefault(p => p.Property == tmb.Member);


                    }

                    var P = x;
                    if (x is MemberExpression)
                    {

                        P = utils.FindParameterExpression((MemberExpression)x);
                    }
                    if (PP != null)
                    {
                        tempMapAlias.Add(new MapAlais()
                        {
                            AliasName = PP.AliasName,
                            Member = mb
                        });
                    }
                    if (P == cx.Parameters[0])
                    {

                        if (qr1.joinTimes > 0)
                        {

                            var propertyOfQ1 = qr1.elemetType.GetProperty(mb.Name);
                            if (propertyOfQ1 == null)
                            {

                                var extraFields = utils.GetSelectAll(qr1.elemetType, qr1.PropertiesMapping);
                                if (!ByLinq)
                                {
                                    exprWithExtraFields.Add(new ExtraSelectAll()
                                    {
                                        Expr = x,
                                        Tables = extraFields
                                    });
                                }
                            }
                        }
                        tempMapAlias.Add(new MapAlais()
                        {
                            AliasName = (PP != null) ? PP.OriginAliasName : ltName,
                            Member = (tmb != null) ? tmb.Member : mb,
                            ParamExpr = (PP == null) ? cx.Parameters[0] : null,
                            Children = qr1.mapAlias,

                        });
                        tempMapAlias.AddRange(cx.Parameters[0].Type.GetProperties().Select(p => new MapAlais
                        {
                            AliasName = (PP != null) ? PP.OriginAliasName : ltName,
                            Member = p,
                            ParamExpr = (PP == null) ? cx.Parameters[0] : null,
                        }));

                        if ((!ByLinq) && (qr2.joinTimes == 0))
                        {

                            ret.PropertiesMapping.Add(new PropertyMapping()
                            {
                                AliasName = (PP != null) ? PP.OriginAliasName : ltName,
                                OriginAliasName = (PP == null) ? ltName : PP.OriginAliasName,
                                TableName = (PP == null) ? qr1.table_name : PP.TableName,
                                Schema = (PP == null) ? qr1.Schema : PP.Schema,
                                Property = typeof(T).GetProperty(mb.Name)
                            });
                        }
                    }
                    if (P == cx.Parameters[1])
                    {
                        if (qr2.joinTimes > 0)
                        {
                            var propertyOfQ1 = qr2.elemetType.GetProperty(mb.Name);
                            if (propertyOfQ1 == null)
                            {

                                if (!ByLinq)
                                {
                                    exprWithExtraFields.Add(new ExtraSelectAll()
                                    {
                                        Expr = x,
                                        Tables = (new string[] { rtName }).ToList()
                                    });
                                }
                                ret.isReadOnly = true;
                            }
                        }
                        if (qr2.joinTimes == 0)
                        {
                            tempMapAlias.Add(new MapAlais()
                            {
                                AliasName = rtName,
                                Member = mb,
                                ParamExpr = cx.Parameters[1],
                                Children = qr2.mapAlias,

                            });
                            tempMapAlias.AddRange(cx.Parameters[1].Type.GetProperties().Select(p => new MapAlais
                            {
                                AliasName = rtName,
                                Member = p,
                                ParamExpr = cx.Parameters[1],
                            }));
                        }
                        else
                        {
                            tempMapAlias.Add(new MapAlais()
                            {
                                AliasName = rtName,
                                ParamExpr = cx.Parameters[1],
                                Member = mb
                            });
                        }
                        ret.PropertiesMapping.Add(new PropertyMapping()
                        {
                            AliasName = rtName,
                            OriginAliasName = (PP == null) ? rtName : PP.OriginAliasName,
                            TableName = (PP == null) ? qr2.table_name : PP.TableName,
                            Schema = (PP == null) ? qr2.Schema : PP.Schema,
                            Property = typeof(T).GetProperty(mb.Name)
                        });
                    }
                    if(x is MethodCallExpression)
                    {
                        var mbxs = utils.GetAllMemberExpression(x);
                        foreach(var m in mbxs)
                        {
                            if (m.Expression == cx.Parameters[0])
                            {
                                tempMapAlias.Add(new MapAlais
                                {
                                    AliasName=ltName,
                                    Schema=qr1.Schema,
                                    Member=m.Member
                                });
                            }
                            if (m.Expression == cx.Parameters[1])
                            {
                                tempMapAlias.Add(new MapAlais
                                {
                                    AliasName = rtName,
                                    Schema = qr2.Schema,
                                    Member = m.Member
                                });
                            }
                        }
                    }


                }

            }
            if (cx.Body is NewExpression)
            {
                fieldMemberList = ((NewExpression)cx.Body).Arguments.Select(p => new QueryMemberInfo()
                {
                    Expression = p,
                    Member = ((NewExpression)cx.Body).Members[((NewExpression)cx.Body).Arguments.IndexOf(p)]
                }).ToList();
            }
            if (cx.Body is MemberInitExpression)
            {
                var ldx = cx as LambdaExpression;
                fieldMemberList = ((MemberInitExpression)cx.Body).Bindings.Cast<MemberAssignment>().Select(p => new QueryMemberInfo()
                {
                    Expression = p.Expression,
                    Member = p.Member
                }).ToList();
                var nx = cx.Body as MemberInitExpression;
                foreach (MemberAssignment a in nx.Bindings)
                {
                    if (a.Expression is MemberExpression)
                    {
                        var mb = a.Expression as MemberExpression;
                        var x = ret.PropertiesMapping.FirstOrDefault(p => p.Property == mb.Member);
                        var y = ret.PropertiesMapping.FirstOrDefault(p => p.Property == a.Member);
                        if (mb.Expression == ldx.Parameters[0])
                        {
                            tempMapAlias.Add(new MapAlais()
                            {
                                AliasName = ltName,
                                Member = a.Member,
                                Schema = qr1.Schema,
                                TableName = qr1.table_name,
                                ParamExpr = mb.Expression as ParameterExpression

                            });
                            tempMapAlias.Add(new MapAlais()
                            {
                                AliasName = ltName,
                                Member = mb.Member,
                                Schema = qr1.Schema,
                                TableName = qr1.table_name,
                                ParamExpr = mb.Expression as ParameterExpression

                            });

                        }
                        if (mb.Expression == ldx.Parameters[1])
                        {
                            tempMapAlias.Add(new MapAlais()
                            {
                                AliasName = rtName,
                                Member = a.Member,
                                Schema = qr2.Schema,
                                TableName = qr2.table_name,
                                ParamExpr = mb.Expression as ParameterExpression
                            });
                            tempMapAlias.Add(new MapAlais()
                            {
                                AliasName = rtName,
                                Member = mb.Member,
                                Schema = qr2.Schema,
                                TableName = qr2.table_name,
                                ParamExpr = mb.Expression as ParameterExpression
                            });
                        }
                        if (x != null)
                        {
                            tempMapAlias.Add(new MapAlais()
                            {
                                AliasName = x.AliasName,
                                Member = a.Member,
                                Schema = x.Schema,
                                TableName = x.TableName

                            });
                            tempMapAlias.Add(new MapAlais()
                            {
                                AliasName = x.AliasName,
                                Member = mb.Member,
                                Schema = x.Schema,
                                TableName = x.TableName

                            });
                        }
                        if (y != null && x != null)
                        {
                            if (x.TableName != null)
                            {
                                y.TableName = x.TableName;
                                y.Schema = x.Schema;
                            }
                        }
                        else
                        {
                            if (x != null)
                            {
                                ret.PropertiesMapping.Add(new PropertyMapping
                                {
                                    AliasName = x.AliasName,
                                    OriginAliasName = x.OriginAliasName,
                                    Property = a.Member as PropertyInfo,
                                    Schema = x.Schema,
                                    TableName = x.TableName
                                });
                            }
                        }
                        if (ldx.Parameters[0].Type.GetProperties().Count() == 2)
                        {
                            foreach (var pt in ldx.Parameters[0].Type.GetProperties())
                            {
                                tempMapAlias.AddRange(pt.PropertyType.GetProperties().Select(p => new MapAlais()
                                {
                                    AliasName = ltName,
                                    Member = p
                                }));
                            }
                        }
                    }

                }
            }
            if (cx.Body is ParameterExpression)
            {
                tempMapAlias.Add(new MapAlais
                {
                    AliasName = rtName,
                    Children = qr1.mapAlias,
                    ParamExpr = cx.Parameters[0],
                    Schema = qr1.Schema,
                    TableName = qr1.table_name
                });
                tempMapAlias.Add(new MapAlais
                {
                    AliasName = rtName,
                    Children = qr2.mapAlias,
                    ParamExpr = cx.Parameters[1],
                    Schema = qr2.Schema,
                    TableName = qr2.table_name
                });
                if (cx.Parameters[0] == cx.Body)
                {
                    exprWithExtraFields.Add(new ExtraSelectAll()
                    {
                        Expr = cx.Parameters[0],
                        Tables = (new string[] { ltName }).ToList()
                    });
                }
                if (cx.Parameters[1] == cx.Body)
                {
                    exprWithExtraFields.Add(new ExtraSelectAll()
                    {
                        Expr = cx.Parameters[1],
                        Tables = (new string[] { rtName }).ToList()
                    });

                }
                fieldMemberList = new List<QueryMemberInfo>();
                fieldMemberList.Add(new QueryMemberInfo()
                {
                    Expression = cx.Body as ParameterExpression
                });
                tempMapAlias.AddRange(ExtraAlaias);
            }
            if (cx.Body is MemberExpression)
            {
                var mb = cx.Body as MemberExpression;
                fieldMemberList = new List<QueryMemberInfo>();
                var mp = utils.FindAliasByMember(qr1.mapAlias, mb.Member);
                if (mp != null)
                {
                    fieldMemberList.Add(new QueryMemberInfo()
                    {
                        Expression = mp.ParamExpr,
                        Member = mp.Member
                    });
                }
                else
                {
                    mp = utils.FindAliasByMember(qr2.mapAlias, mb.Member);
                    fieldMemberList.Add(new QueryMemberInfo()
                    {
                        Expression = mp.ParamExpr,
                        Member = mp.Member
                    });
                }
            }
            retFields = Compiler.GetFields(qr1.compiler, fieldMemberList, ret.FieldsMapping, tempMapAlias, exprWithExtraFields, ref Params);
            ret.mapAlias = tempMapAlias;
            ret.fields = retFields;
            ret.fieldParams = new List<object>();
            ret.fieldParams.AddRange(refParameters);
            ret.Params = Params;
            ret.dbContext = qr1.dbContext;
            ret.strJoinClause.Add(strJoin);
            ret.compiler = qr1.compiler;
            if (((LambdaExpression)((UnaryExpression)Selector).Operand).Body is NewExpression)
            {
                var mx1 = ((NewExpression)((LambdaExpression)((UnaryExpression)Selector).Operand).Body);
                var ldx = ((LambdaExpression)((UnaryExpression)Selector).Operand);
                foreach (var a in mx1.Arguments)
                {
                    var mb = mx1.Members[mx1.Arguments.IndexOf(a)];
                    var mb1 = mb;
                    var PP = a as ParameterExpression;
                    if (a is MemberExpression)
                    {
                        PP = utils.FindParameterExpression((MemberExpression)a);
                        mb = ((MemberExpression)a).Member;
                    }
                    //List<PropertyInfo> lstPros = utils.GetAllProperties(mb as PropertyInfo);
                    //var checkProsListCount = ret.PropertiesMapping.Join(lstPros, p => p.Property, q => q, (p, q) => p).Count();
                    var pp = ret.PropertiesMapping.FirstOrDefault(p => p.Property == mb);
                    //checkProsListCount = 0;
                    if (PP == ldx.Parameters[0])
                    {
                        ret.PropertiesMapping.Add(new PropertyMapping()
                        {
                            AliasName = (pp != null) ? (pp.OriginAliasName ?? pp.AliasName) : ltName,
                            OriginAliasName = (pp != null) ? pp.OriginAliasName : ltName,
                            Property = mb1 as PropertyInfo,
                            Schema = qr1.Schema,
                            TableName = qr1.table_name
                        });
                    }
                    if (PP == ldx.Parameters[1])
                    {
                        ret.PropertiesMapping.Add(new PropertyMapping()
                        {
                            AliasName = (pp != null) ? (pp.OriginAliasName ?? pp.AliasName) : rtName,
                            OriginAliasName = (pp != null) ? pp.OriginAliasName : rtName,
                            Property = mb1 as PropertyInfo,
                            Schema = qr2.Schema,
                            TableName = qr2.table_name
                        });
                    }






                }
            }
            return ret;
        }

        private static List<MapAlais> createMapAlais(BaseQuerySet qr, string ltName)
        {
            var ret = qr.elemetType.GetProperties().Where(p => !qr.mapAlias.Any(x => x.Member == p)).Select(p => new MapAlais()
            {
                AliasName = ltName,
                Member = p
            }).ToList();
            return ret;
        }
        
        internal static QuerySet<T> BuilJoinFromBase<T>(BaseQuerySet qr1, BaseQuerySet qr2, LambdaExpression JoinClause, LambdaExpression selector, string JoinType)
        {
            var tempMapAlias = new List<MapAlais>();
            var Cmp = qr1.compiler;
            var ret = new QuerySet<T>(true);
            ret.compiler = Cmp;
            ret.FieldsMapping = qr1.FieldsMapping.Union(qr2.FieldsMapping).ToList();
            ret.joinTimes = (qr1.joinTimes + 1) * 10 + qr2.joinTimes;
            var ltName = "l" + ret.joinTimes.ToString();
            var rtName = "r" + ret.joinTimes.ToString();
            if (!string.IsNullOrEmpty(qr1.table_name))
            {
                ret.tableAlias.Add(new TableAliasInfo()
                {

                    Schema = qr1.Schema,
                    TableName = qr1.table_name,
                    Alias = ltName
                });
            }
            ret.PropertiesMapping.AddRange(qr1.PropertiesMapping);
            ret.PropertiesMapping.AddRange(qr2.PropertiesMapping);

            ret.tableAlias.AddRange(qr1.tableAlias);
            if (!string.IsNullOrEmpty(qr2.table_name))
            {
                ret.tableAlias.Add(new TableAliasInfo()
                {

                    Schema = qr2.Schema,
                    TableName = qr2.table_name,
                    Alias = rtName
                });
            }
            ret.tableAlias.AddRange(qr2.tableAlias);

            var isCrossJoin = false;
            if (JoinClause != null && JoinClause.Body is ConstantExpression)
            {
                var retBool = utils.Resolve(JoinClause.Body);
                if (retBool.Value is bool && (bool)retBool.Value == true)
                {
                    isCrossJoin = true;
                }
            }
            if (JoinClause == null)
            {
                isCrossJoin = true;
            }
            //var nx = selector.Body as NewExpression;

            var Params = new List<object>();
            Params.AddRange(qr1.Params);
            Params.AddRange(qr2.Params);
            var joinParams = new List<object>();
            var strJoin = "";


            Params.AddRange(joinParams);
            var leftSource = "";
            var rightSource = "";
            var mbs = utils.GetAllMemberExpression(JoinClause);

            if (qr1.joinTimes == 0 && qr2.joinTimes == 0)
            {
                tempMapAlias.AddRange(createMapAlais(qr1, ltName));
                if (!isCrossJoin)
                {
                    if (JoinClause.Parameters.Count == 2)
                    {
                        strJoin = Compiler.GetLogicalExpression(qr1.compiler, JoinClause.Body, ret.FieldsMapping, (new MapAlais[] {
                            new MapAlais() {
                                AliasName=ltName,
                                ParamExpr=JoinClause.Parameters[0],
                                Children=qr1.IsSubQueryOnNextPhaseWithSelectedFields()?null:qr1.mapAlias
                            },
                            new MapAlais() {
                                AliasName=rtName,
                                ParamExpr=JoinClause.Parameters[1],
                                Children=qr2.IsSubQueryOnNextPhaseWithSelectedFields()?null:qr2.mapAlias
                            }
                        }).ToList(), ref Params);
                    }
                    else
                    {

                       // var tmp = qr1.mapAlias.ToList();
                       var tmp = qr1.mapAlias.Select(p => new MapAlais
                        {
                            AliasName=p.AliasName,
                            Member=p.Member,
                            ParamExpr=p.ParamExpr
                            
                        }).ToList();
                        tmp.AddRange(createMapAlais(qr1, ltName));
                        tmp.AddRange(qr2.mapAlias);
                        mbs.ForEach(p =>
                        {
                            var m = utils.FindAliasByMemberExpression(tmp, p);
                            if (m == null)
                            {
                                if (p.Expression == JoinClause.Parameters[0])
                                {
                                    tmp.Add(new MapAlais
                                    {
                                        AliasName = rtName,
                                        Member = p.Member,
                                        ParamExpr = JoinClause.Parameters[0],
                                        //Schema = qr2.Schema,
                                        //TableName = qr2.table_name
                                    });
                                }
                                else
                                {
                                    tmp.Add(new MapAlais
                                    {
                                        AliasName = ltName,
                                        Member = p.Member,
                                        ParamExpr = p.Expression as ParameterExpression,
                                        Schema = qr2.Schema,
                                        TableName = qr2.table_name
                                    });
                                }


                            }
                            ret.mapAlias.AddRange(tmp);
                        });
                        //tmp.AddRange(createMapAlais(qr, ltName));
                        tmp.Add(new MapAlais()
                        {
                            AliasName = rtName,
                            ParamExpr = JoinClause.Parameters[0]
                        });

                        strJoin = Compiler.GetLogicalExpression(qr1.compiler, JoinClause.Body, ret.FieldsMapping, tmp, ref Params);
                    }
                }
                leftSource = (!qr1.IsSubQueryOnNextPhaseWithSelectedFields()) ? utils.GetSource(qr1, ltName) : ("(" + qr1.ToSQLString() + ")");
                rightSource = (!qr2.IsSubQueryOnNextPhaseWithSelectedFields()) ? utils.GetSource(qr2, rtName) : ("(" + qr2.ToSQLString() + ")");
                rightSource = utils.RepairParameters(rightSource, qr1.Params, qr2.Params);

                ret.JoinObject = new QueryJoinObject()
                {
                    L = leftSource + " as " + Cmp.GetQName("", ltName.ToString()),
                    R = rightSource + " as " + Cmp.GetQName("", rtName.ToString()),
                    J = JoinType + " join ",
                    C = "on " + strJoin
                };
                if (!isCrossJoin)
                {
                    string JoinExpr = utils.BuildJoinExpr(ret.JoinObject);
                    ret.datasource = leftSource + " as " + Cmp.GetQName("", ltName.ToString()) +
                                " " + JoinType + " join " + rightSource + " as " + Cmp.GetQName("", rtName.ToString()) + " on " + strJoin;
                }
                else
                {
                    ret.datasource = leftSource + " as " + Cmp.GetQName("", ltName.ToString()) + "," +
                                     rightSource + " as " + Cmp.GetQName("", rtName.ToString());
                }
            }
            if (qr1.joinTimes > 0 && qr2.joinTimes == 0)
            {
                if (!isCrossJoin)
                {
                    if (JoinClause.Parameters.Count == 2)
                    {
                        strJoin = Compiler.GetLogicalExpression(qr1.compiler, JoinClause.Body, ret.FieldsMapping, (new MapAlais[] {
                            new MapAlais() {
                                AliasName=ltName,
                                ParamExpr=JoinClause.Parameters[0],
                                Children=qr1.IsSubQueryOnNextPhaseWithSelectedFields()?null:qr1.mapAlias
                            },
                            new MapAlais() {
                                AliasName=rtName,
                                ParamExpr=JoinClause.Parameters[1],
                                Children=qr2.IsSubQueryOnNextPhaseWithSelectedFields()?null:qr2.mapAlias
                            }
                        }).ToList(), ref Params);
                    }
                    else
                    {
                        var mapAliasCompiler = new List<MapAlais>();
                        if (!qr1.IsSubQueryOnNextPhase())
                        {
                            mapAliasCompiler.AddRange(qr1.mapAlias.Select(p=>new MapAlais {
                                AliasName=p.AliasName?? ltName,
                                Member=p.Member,
                                ParamExpr=p.ParamExpr
                            }));
                        }
                        var mbxs = utils.GetAllMemberExpression(JoinClause);
                        foreach (var x in mbxs)
                        {
                            if (x.Expression == JoinClause.Parameters[0])
                            {
                                mapAliasCompiler.Add(new MapAlais
                                {
                                    AliasName = rtName,
                                    //Children = (qr2.mapAlias.Count > 0) ? qr2.mapAlias : null,
                                    Member = x.Member,
                                    ParamExpr = x.Expression as ParameterExpression,
                                    //Schema = qr2.Schema,
                                    //TableName = qr2.table_name
                                });
                            }
                            else
                            {
                                var mp = utils.FindAliasByMember(qr1.mapAlias, x.Member);
                                if (qr1.IsSubQueryOnNextPhase())
                                {
                                    mp = null;
                                }
                                mapAliasCompiler.Add(new MapAlais
                                {
                                    AliasName = (mp!=null)?(mp.AliasName??ltName):ltName,
                                    Member = x.Member,
                                    ParamExpr = x.Expression as ParameterExpression,
                                    //Schema = (mp != null) ? mp.Schema : qr1.Schema,
                                    //TableName = (mp != null) ? mp.TableName : qr1.table_name
                                });
                            }

                        }

                        strJoin = Compiler.GetLogicalExpression(qr1.compiler, JoinClause.Body, ret.FieldsMapping, mapAliasCompiler, ref Params);
                    }
                }
                leftSource = (!qr1.IsSubQueryOnNextPhaseWithSelectedFields()) ? utils.GetSource(qr1, ltName) : ("(" + qr1.ToSQLString() + ") as " + Cmp.GetQName("", ltName));
                rightSource = (!qr2.IsSubQueryOnNextPhaseWithSelectedFields()) ? utils.GetSource(qr2, rtName) : ("(" + qr2.ToSQLString() + ") as " + Cmp.GetQName("", rtName));
                rightSource = utils.RepairParameters(rightSource, qr1.Params, qr2.Params);
                ret.JoinObject = new QueryJoinObject()
                {
                    L = qr1.JoinObject,
                    R = rightSource + " as " + Cmp.GetQName("", rtName.ToString()),
                    J = JoinType + " join ",
                    C = "on " + strJoin
                };
                if (!isCrossJoin)
                {
                    ret.datasource = leftSource +
                                " " + JoinType + " join " + rightSource + " as " + Cmp.GetQName("", rtName.ToString()) + " on " + strJoin;
                }
                else
                {
                    ret.datasource = leftSource + "as " + Cmp.GetQName("", ltName.ToString()) +
                                " , " + rightSource + " as " + Cmp.GetQName("", rtName.ToString());
                }
            }
            if (qr1.joinTimes == 0 && qr2.joinTimes > 0)
            {
                if (!isCrossJoin)
                {
                    if (JoinClause.Parameters.Count == 2)
                    {
                        strJoin = Compiler.GetLogicalExpression(qr1.compiler, JoinClause.Body, ret.FieldsMapping, (new MapAlais[] {
                            new MapAlais() {
                                AliasName=ltName,
                                ParamExpr=JoinClause.Parameters[0],
                                Children=qr1.IsSubQueryOnNextPhaseWithSelectedFields()?null:qr1.mapAlias
                            },
                            new MapAlais() {
                                AliasName=rtName,
                                ParamExpr=JoinClause.Parameters[1]
                            }
                        }).ToList(), ref Params);
                    }
                    else
                    {
                        var mbxs = utils.GetAllMemberExpression(JoinClause);
                        var mps = new List<MapAlais>();
                        mps.Add(new MapAlais()
                        {
                            AliasName = ltName,
                            ParamExpr = JoinClause.Parameters[0],
                            Children = (qr1.mapAlias.Count > 0) ? qr1.mapAlias : null
                        });
                        mbxs.ForEach(mbx =>
                        {
                            if (mbx.Expression == JoinClause.Parameters[0])
                            {
                                var mp = utils.FindAliasByMember(qr2.mapAlias, mbx.Member);
                                mps.Add(new MapAlais
                                {
                                    AliasName = rtName,
                                    Member = mbx.Member,
                                    ParamExpr = JoinClause.Parameters[0],
                                    Schema = qr2.Schema,
                                    TableName = qr2.table_name
                                });
                            }
                            else
                            {
                                var mp = utils.FindAliasByMember(qr1.mapAlias, mbx.Member);
                                mps.Add(new MapAlais
                                {
                                    AliasName = (mp != null) ? mp.AliasName : rtName,
                                    Member = mbx.Member,
                                    ParamExpr = mbx.Expression as ParameterExpression,
                                    Schema = (mp != null) ? mp.Schema : qr2.Schema,
                                    TableName = (mp != null) ? mp.TableName : qr2.table_name
                                });
                            }
                        });
                        strJoin = Compiler.GetLogicalExpression(qr1.compiler, JoinClause.Body, ret.FieldsMapping, mps, ref Params);
                    }
                }
                leftSource = (!qr1.IsSubQueryOnNextPhaseWithSelectedFields()) ? utils.GetSource(qr1, ltName) : ("(" + qr1.ToSQLString() + ") as " + Cmp.GetQName("", ltName));
                rightSource = "(" + qr2.ToSQLString() + ") as " + Cmp.GetQName("", rtName);
                rightSource = utils.RepairParameters(rightSource, qr1.Params, qr2.Params);

                ret.JoinObject = new QueryJoinObject()
                {
                    L = leftSource + " as " + Cmp.GetQName("", ltName.ToString()),
                    R = qr2.JoinObject,
                    J = JoinType + " join ",
                    C = "on " + strJoin
                };


                if (!isCrossJoin)
                {
                    ret.datasource = leftSource + " as " + Cmp.GetQName("", ltName.ToString()) +
                                    " " + JoinType + " join " +
                                    rightSource +
                                    " on " + strJoin;
                }
                else
                {
                    ret.datasource = leftSource + " as " + Cmp.GetQName("", ltName.ToString()) +
                                    " , " +
                                    rightSource;
                }
            }
            if (qr1.joinTimes > 0 && qr2.joinTimes > 0)
            {

                if (!isCrossJoin)
                {
                    if (JoinClause.Parameters.Count == 2)
                    {
                        ////var tmpAlias = qr1.mapAlias.Union(new MapAlais[] { new MapAlais() {
                        ////    AliasName=rtName,
                        ////    ParamExpr=JoinClause.Parameters[1]
                        ////}}).ToList();
                        strJoin = Compiler.GetLogicalExpression(qr1.compiler, JoinClause.Body, ret.FieldsMapping, (new MapAlais[] {
                            new MapAlais() {
                                AliasName=ltName,
                                ParamExpr=JoinClause.Parameters[0],
                                Children=qr1.IsSubQueryOnNextPhaseWithSelectedFields()?null:qr1.mapAlias
                            },
                            new MapAlais() {
                                AliasName=rtName,
                                ParamExpr=JoinClause.Parameters[1]
                            }
                        }).ToList(), ref Params);
                    }
                    else
                    {
                        strJoin = Compiler.GetLogicalExpression(qr1.compiler, JoinClause.Body, ret.FieldsMapping, qr1.mapAlias.Union(qr2.mapAlias).ToList(), ref Params);
                    }

                }
                ret.JoinObject = new QueryJoinObject()
                {
                    L = qr1.JoinObject,
                    R = qr2.JoinObject,
                    J = JoinType + " join ",
                    C = "on " + strJoin
                };
                leftSource = (!qr1.IsSubQueryOnNextPhaseWithSelectedFields()) ? utils.GetSource(qr1, ltName) : ("(" + qr1.ToSQLString() + ") as " + Cmp.GetQName("", ltName));
                rightSource = "(" + qr2.ToSQLString() + ") as " + Cmp.GetQName("", rtName);
                rightSource = utils.RepairParameters(rightSource, qr1.Params, qr2.Params);
                if (!isCrossJoin)
                {
                    ret.datasource = leftSource +
                            " " + JoinType + " join " + rightSource + " on " + strJoin;
                }
                else
                {
                    ret.datasource = leftSource +
                            " " + JoinType + " , " + rightSource;
                }
            }



            //var refTables = new List<object>();
            var refParameters = new List<object>();

            if (qr1.fieldParams != null)
            {
                refParameters.AddRange(qr1.fieldParams);
            }
            if (qr2.fieldParams != null)
            {
                refParameters.AddRange(qr2.fieldParams);
            }
            var retFields = new List<QueryFieldInfo>();

            var exprWithExtraFields = new List<ExtraSelectAll>();
            //ret.fields = new List<QueryFieldInfo>();
            if (qr2.joinTimes > 0)
            {
                tempMapAlias = new List<MapAlais>();
                tempMapAlias.AddRange(qr1.mapAlias);
            }
            if (qr1.IsSubQueryOnNextPhase())
            {
                tempMapAlias.AddRange(qr1.elemetType.GetProperties().Select(p => new MapAlais
                {
                    AliasName = ltName,
                    Member = selector.Parameters[0].Type.GetProperty(p.Name),
                    ParamExpr = selector.Parameters[0]


                }));
            }
            if (qr2.IsSubQueryOnNextPhase())
            {
                tempMapAlias.AddRange(qr2.elemetType.GetProperties().Select(p => new MapAlais
                {
                    AliasName = rtName,
                    Member = selector.Parameters[1].Type.GetProperty(p.Name),
                    ParamExpr = selector.Parameters[1]


                }));
            }
            var isSubQuery = false;
            var newMapAlais = new List<MapAlais>();
            if (selector.Body is MemberInitExpression)
            {
                var mx = selector.Body as MemberInitExpression;
                foreach (MemberAssignment x in mx.Bindings)
                {
                    var argIndex = mx.Bindings.IndexOf(x);
                    if (x.Expression is MethodCallExpression)
                    {
                        isSubQuery = true;
                        var mbxs = utils.GetAllMemberExpression(x.Expression);
                        foreach (var mbx in mbxs)
                        {
                            if (mbx.Expression == selector.Parameters[0])
                            {
                                var mp = utils.FindAliasByMember(qr1.mapAlias, mbx.Member);
                                var mapAlias = new MapAlais
                                {
                                    AliasName = (mp != null) ? mp.AliasName : ltName,
                                    Member = mbx.Member,
                                    ParamExpr = selector.Parameters[0],
                                    //Schema = (mp != null) ? mp.Schema : qr1.Schema,
                                    //TableName = (mp != null) ? mp.TableName : qr1.table_name
                                };

                                tempMapAlias.Add(mapAlias);
                            }
                            if (mbx.Expression == selector.Parameters[1])
                            {
                                var mp = utils.FindAliasByMember(qr2.mapAlias, mbx.Member);
                                var mapAlias = new MapAlais
                                {
                                    AliasName = (mp != null) ? mp.AliasName : rtName,
                                    Member = mbx.Member,
                                    ParamExpr = selector.Parameters[1],
                                    //Schema = (mp != null) ? mp.Schema : qr2.Schema,
                                    //TableName = (mp != null) ? mp.TableName : qr2.table_name
                                };

                                tempMapAlias.Add(mapAlias);
                            }
                        }
                    }
                    else
                    {
                        MemberExpression tmb = utils.FindMember(x.Expression);
                        var mb = x.Member;

                        PropertyMapping PP = null;
                        if (tmb != null)
                        {
                            PP = ret.PropertiesMapping.FirstOrDefault(p => p.Property == tmb.Member);
                        }
                        var checkP = ret.PropertiesMapping.FirstOrDefault(p => p.Property == mb);
                        if (checkP == null && ret.PropertiesMapping.Count > 0)
                        {

                        }
                        var P = x.Expression;
                        if (x.Expression is MemberExpression)
                        {

                            P = utils.FindParameterExpression((MemberExpression)x.Expression);
                        }
                        //if (PP != null)
                        //{
                        //    //tempMapAlias.Add(new MapAlais()
                        //    //{
                        //    //    AliasName = PP.AliasName,
                        //    //    Member = mb
                        //    //});
                        //}
                        if (P == selector.Parameters[0])
                        {
                            var mp = utils.FindAliasByMember(qr1.mapAlias, mb);
                            var mapAlais = new MapAlais()
                            {
                                AliasName = (PP != null) ? PP.OriginAliasName : ltName,
                                Member = mb,
                                ParamExpr = selector.Parameters[0],
                                Children = qr1.mapAlias,

                            };
                            tempMapAlias.Add(mapAlais);
                            if (tmb != null)
                            {
                                mapAlais = new MapAlais()
                                {
                                    AliasName = (PP != null) ? PP.OriginAliasName : ltName,
                                    Member = tmb.Member,
                                    ParamExpr = selector.Parameters[0],
                                    Children = qr1.mapAlias,

                                };
                                tempMapAlias.Add(mapAlais);
                            }
                            ret.PropertiesMapping.Add(new PropertyMapping()
                            {
                                AliasName = (PP != null) ? PP.OriginAliasName : ltName,
                                OriginAliasName = (PP == null) ? ltName : PP.OriginAliasName,
                                TableName = (PP == null) ? qr1.table_name : PP.TableName,
                                Schema = (PP == null) ? qr1.Schema : PP.Schema,
                                Property = typeof(T).GetProperty(mb.Name)
                            });
                        }
                        if (P == selector.Parameters[1])
                        {
                            var mp = utils.FindAliasByMember(qr2.mapAlias, mb);
                            var mapAlais = new MapAlais()
                            {
                                AliasName = (PP != null) ? PP.OriginAliasName : rtName,
                                Member = mb,
                                ParamExpr = selector.Parameters[0],
                                Children = qr1.mapAlias,

                            };
                            tempMapAlias.Add(mapAlais);
                            if (qr2.joinTimes > 0)
                            {
                                tempMapAlias.RemoveAll(p => (p.ParamExpr != null && p.ParamExpr == selector.Parameters[1]) && ((p.Children == null) || (p.Children.Count == 0)));
                                var propertyOfQ1 = qr2.elemetType.GetProperty(mb.Name);
                                if (propertyOfQ1 == null)
                                {
                                    if (!utils.CheckIfPrimitiveType(((PropertyInfo)mb).PropertyType))
                                    {
                                        exprWithExtraFields.Add(new ExtraSelectAll()
                                        {
                                            Expr = x.Expression,
                                            Tables = (new string[] { rtName }).ToList()
                                        });
                                    }
                                    else
                                    {
                                        mapAlais = new MapAlais()
                                        {
                                            AliasName = rtName,
                                            Member = mb,
                                            ParamExpr = selector.Parameters[1]

                                        };
                                        tempMapAlias.Add(mapAlais);
                                    }


                                    ret.isReadOnly = true;
                                }

                            }
                            if (qr2.joinTimes == 0)
                            {
                                if (tmb != null)
                                {
                                    var mapAlias = new MapAlais()
                                    {
                                        AliasName = rtName,
                                        Member = tmb.Member,
                                        ParamExpr = selector.Parameters[1],
                                        Children = qr2.mapAlias,

                                    };
                                    tempMapAlias.Add(mapAlias);
                                }
                            }
                            else
                            {
                                mapAlais = new MapAlais()
                                {
                                    AliasName = rtName,
                                    ParamExpr = selector.Parameters[1],
                                };
                                tempMapAlias.Add(mapAlais);

                            }
                            ret.PropertiesMapping.Add(new PropertyMapping()
                            {
                                AliasName = rtName,
                                OriginAliasName = (PP == null) ? rtName : PP.OriginAliasName,
                                TableName = (PP == null) ? qr2.table_name : PP.TableName,
                                Schema = (PP == null) ? qr2.Schema : PP.Schema,
                                Property = typeof(T).GetProperty(mb.Name)
                            });
                        }
                    }
                }

            }


            if (selector.Body is NewExpression)
            {

                var mx = selector.Body as NewExpression;
                
                foreach (var x in mx.Arguments)
                {
                    var argIndex = mx.Arguments.IndexOf(x);

                    MemberExpression tmb = utils.FindMember(x);
                    var mb = mx.Members[argIndex];
                    PropertyMapping PP = null;
                    if (tmb != null)
                    {
                        PP = ret.PropertiesMapping.FirstOrDefault(p => p.Property == tmb.Member);
                    }
                    var P = x;
                    if (x is MemberExpression)
                    {

                        P = utils.FindParameterExpression((MemberExpression)x);
                    }
                    else if(x is MethodCallExpression)
                    {
                        isSubQuery = true;
                        var mbxs = utils.GetAllMemberExpression(((MethodCallExpression)x));
                        foreach (var mbx in mbxs)
                        {
                            if (mbx.Expression == selector.Parameters[0])
                            {
                                var mp = utils.FindAliasByMember(qr1.mapAlias, mbx.Member);
                                var mapAlias = new MapAlais
                                {
                                    AliasName = (mp != null) ? mp.AliasName : ltName,
                                    Member = mbx.Member,
                                    ParamExpr = selector.Parameters[0],
                                    //Schema = (mp != null) ? mp.Schema : qr1.Schema,
                                    //TableName = (mp != null) ? mp.TableName : qr1.table_name
                                };

                                tempMapAlias.Add(mapAlias);
                            }
                            if (mbx.Expression == selector.Parameters[1])
                            {
                                var mp = utils.FindAliasByMember(qr2.mapAlias, mbx.Member);
                                var mapAlias = new MapAlais
                                {
                                    AliasName = (mp != null) ? mp.AliasName : rtName,
                                    Member = mbx.Member,
                                    ParamExpr = selector.Parameters[1],
                                    //Schema = (mp != null) ? mp.Schema : qr2.Schema,
                                    //TableName = (mp != null) ? mp.TableName : qr2.table_name
                                };

                                tempMapAlias.Add(mapAlias);
                            }
                        }
                    }
                    else
                    {
                        var nmbs = utils.GetAllMemberExpression(x);
                        foreach (var fx in nmbs)
                        {
                            var px = utils.FindParameterExpression(fx);
                            if (px == selector.Parameters[0])
                            {
                                var pmInfo = utils.FindAliasByMember(qr1.mapAlias, fx.Member);

                                tempMapAlias.Add(new MapAlais
                                {
                                    AliasName = (pmInfo != null) ? pmInfo.AliasName : ltName,
                                    Member = fx.Member,
                                    ParamExpr = selector.Parameters[0],
                                    Schema = (pmInfo != null) ? pmInfo.Schema : qr1.Schema,
                                    TableName = (pmInfo != null) ? pmInfo.TableName : qr1.table_name
                                });
                            }
                            else if (px == selector.Parameters[1])
                            {
                                if (qr2.joinTimes == 0)
                                {
                                    var pmInfo = utils.FindAliasByMember(qr2.mapAlias, fx.Member);
                                    tempMapAlias.Add(new MapAlais
                                    {
                                        AliasName = (pmInfo != null) ? pmInfo.AliasName : rtName,
                                        Member = fx.Member,
                                        ParamExpr = selector.Parameters[1],
                                        Schema = (pmInfo != null) ? pmInfo.Schema : qr2.Schema,
                                        TableName = (pmInfo != null) ? pmInfo.TableName : qr2.table_name
                                    });
                                }
                                else
                                {

                                }
                            }
                            else if(fx.Expression is ConstantExpression)
                            {
                                
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }
                    }
                    if (PP != null)
                    {
                        tempMapAlias.Add(new MapAlais()
                        {
                            AliasName = PP.AliasName,
                            Member = mb
                        });
                    }
                    if (P == selector.Parameters[0])
                    {
                        tempMapAlias.Add(new MapAlais()
                        {
                            AliasName = (PP != null) ? PP.OriginAliasName : ltName,
                            Member = mb,
                            ParamExpr = (PP == null) ? selector.Parameters[0] : null,
                            Children = qr1.IsSubQueryOnNextPhaseWithSelectedFields() ? null : qr1.mapAlias,

                        });
                        if (tmb != null)
                        {
                            tempMapAlias.Add(new MapAlais()
                            {
                                AliasName = (PP != null) ? PP.OriginAliasName : ltName,
                                Member = tmb.Member,
                                ParamExpr = (PP == null) ? selector.Parameters[0] : null,
                                Children = qr1.IsSubQueryOnNextPhaseWithSelectedFields() ? null : qr1.mapAlias,

                            });
                        }
                        ret.PropertiesMapping.Add(new PropertyMapping()
                        {
                            AliasName = (PP != null) ? PP.OriginAliasName : ltName,
                            OriginAliasName = (PP == null) ? ltName : PP.OriginAliasName,
                            TableName = (PP == null) ? qr1.table_name : PP.TableName,
                            Schema = (PP == null) ? qr1.Schema : PP.Schema,
                            Property = typeof(T).GetProperty(mb.Name)
                        });
                    }
                    if (P == selector.Parameters[1])
                    {
                        if (qr2.joinTimes > 0)
                        {

                            tempMapAlias.Add(new MapAlais()
                            {
                                AliasName = rtName,
                                ParamExpr = selector.Parameters[1]
                            });
                            var propertyOfQ1 = qr2.elemetType.GetProperty(mb.Name);
                            if (propertyOfQ1 == null)
                            {
                                if (!utils.CheckIfPrimitiveType(((PropertyInfo)mb).PropertyType))
                                {
                                    exprWithExtraFields.Add(new ExtraSelectAll()
                                    {
                                        Expr = x,
                                        Tables = (new string[] { rtName }).ToList()
                                    });
                                }
                                else
                                {
                                    tempMapAlias.Add(new MapAlais()
                                    {
                                        AliasName = rtName,
                                        Member = mb,
                                        ParamExpr = selector.Parameters[1]

                                    });
                                }


                                ret.isReadOnly = true;
                            }

                        }
                        if (qr2.joinTimes == 0)
                        {
                            tempMapAlias.Add(new MapAlais()
                            {
                                AliasName = rtName,
                                Member = mb,
                                ParamExpr = selector.Parameters[1],
                                Children = qr2.mapAlias,

                            });
                        }
                        else
                        {
                            tempMapAlias.Add(new MapAlais()
                            {
                                AliasName = rtName,
                                ParamExpr = selector.Parameters[1],
                                Member = mb
                            });
                        }
                        ret.PropertiesMapping.Add(new PropertyMapping()
                        {
                            AliasName = rtName,
                            OriginAliasName = (PP == null) ? rtName : PP.OriginAliasName,
                            TableName = (PP == null) ? qr2.table_name : PP.TableName,
                            Schema = (PP == null) ? qr2.Schema : PP.Schema,
                            Property = typeof(T).GetProperty(mb.Name)
                        });
                    }


                }

            }

            List<QueryMemberInfo> fieldMemberList = null;
            if (selector.Body is NewExpression)
            {
                fieldMemberList = ((NewExpression)selector.Body).Arguments.Select(p => new QueryMemberInfo()
                {
                    Expression = p,
                    Member = ((NewExpression)selector.Body).Members[((NewExpression)selector.Body).Arguments.IndexOf(p)]
                }).ToList();
            }
            if (selector.Body is MemberInitExpression)
            {
                fieldMemberList = ((MemberInitExpression)selector.Body).Bindings.Cast<MemberAssignment>().Select(p => new QueryMemberInfo()
                {
                    Expression = p.Expression,
                    Member = p.Member
                }).ToList();
            }
            if(selector.Body is ParameterExpression)
            {
                var p = selector.Body as ParameterExpression;
                fieldMemberList = new List<QueryMemberInfo>();
                if (selector.Parameters[0] == selector.Body)
                {
                    fieldMemberList.Add(new QueryMemberInfo
                    {
                        Expression = ((ParameterExpression)selector.Body),

                    });
                    ret.FieldsMapping = qr1.FieldsMapping;
                }
                else
                {
                    fieldMemberList.Add(new QueryMemberInfo
                    {
                        Expression = ((ParameterExpression)selector.Body),


                    });
                    ret.FieldsMapping = qr2.FieldsMapping;
                }
                
            }
            retFields = Compiler.GetFields(qr1.compiler, fieldMemberList, ret.FieldsMapping, tempMapAlias, exprWithExtraFields, ref Params);
            retFields = retFields.GroupBy(p => p.Field + " as " + ((p.Alias == null) ? "" : p.Alias.ToString())).Select(p => new QueryFieldInfo()
            {
                Alias = p.FirstOrDefault().Alias,
                Field = p.FirstOrDefault().Field
            }).ToList();
            ret.mapAlias = tempMapAlias;
            ret.fields = retFields;
            ret.fieldParams = new List<object>();
            ret.fieldParams.AddRange(refParameters);
            ret.Params = Params;
            ret.dbContext = qr1.dbContext;
            ret.strJoinClause.Add(strJoin);


            foreach (var fx in fieldMemberList)
            {
                var mb = fx.Member;
                var mb1 = mb;
                var PP = fx.Expression as ParameterExpression;
                var a = fx.Expression;
                if (a is MemberExpression)
                {
                    PP = utils.FindParameterExpression((MemberExpression)a);
                    mb = ((MemberExpression)a).Member;
                }

                var pp = ret.PropertiesMapping.FirstOrDefault(p => p.Property == mb);
                if (PP == selector.Parameters[0])
                {
                    ret.PropertiesMapping.Add(new PropertyMapping()
                    {
                        AliasName = (pp != null) ? (pp.OriginAliasName ?? pp.AliasName) : ltName,
                        OriginAliasName = (pp != null) ? pp.OriginAliasName : ltName,
                        Property = mb1 as PropertyInfo,
                        Schema = qr1.Schema,
                        TableName = qr1.table_name
                    });
                }
                if (PP == selector.Parameters[1])
                {
                    ret.PropertiesMapping.Add(new PropertyMapping()
                    {
                        AliasName = (pp != null) ? (pp.OriginAliasName ?? pp.AliasName) : rtName,
                        OriginAliasName = (pp != null) ? pp.OriginAliasName : rtName,
                        Property = mb1 as PropertyInfo,
                        Schema = qr2.Schema,
                        TableName = qr2.table_name
                    });
                }






            }
            //if (selector.Body is MemberInitExpression)
            //{
            //    ret.exprSelection = selector;
            //}
            ret.latestSelector = selector;
            
            return ret;
        }
        public static QuerySet<T> CrossJoin<T1, T2, T>(ICompiler Provider, QuerySet<T1> qr1, QuerySet<T2> qr2, Expression<Func<T1, T2, T>> resultSelector)
        {
            return BuilCrossJoin<T1, T2, T>(Provider, qr1, qr2, resultSelector);
        }

        private static QuerySet<T> BuilCrossJoin<T1, T2, T>(ICompiler Provider, QuerySet<T1> qr1, QuerySet<T2> qr2, Expression<Func<T1, T2, T>> resultSelector)
        {
            var Cmp = qr1.compiler;
            var Params = new List<object>();

            var retQr = new QuerySet<T>(Provider);
            retQr.joinTimes++;
            var table1 = "T" + retQr.joinTimes.ToString();
            //retQr.sequenceJoin++;
            var table2 = "T" + retQr.joinTimes.ToString();
            var source1 = Cmp.CreateAliasTable(Cmp.GetQName(qr1.Schema, qr1.GetTableName()), Cmp.GetQName("", table1));
            var source2 = Cmp.CreateAliasTable(Cmp.GetQName(qr1.Schema, qr2.GetTableName()), Cmp.GetQName("", table2));
            if (qr1.GetFields().Count > 0 || (!string.IsNullOrEmpty(((QuerySet<T1>)qr1).where)))
            {
                source1 = Cmp.CreateAliasTable("(" + qr1.ToSQLString() + ")", Cmp.GetQName("", table1));
            }
            if (qr2.GetFields().Count > 0 || (!string.IsNullOrEmpty(((QuerySet<T2>)qr2).where)))
            {
                source2 = Cmp.CreateAliasTable("(" + qr2.ToSQLString() + ")", Cmp.GetQName("", table2));
            }

            for (var i = 0; i < qr2.GetParams().Count; i++)
            {
                var nIndex = qr1.GetParams().Count + Params.Count + i;
                source2 = source2.Replace("{" + i.ToString() + "}", "{$##" + nIndex.ToString() + "##$}");
            }
            string FromSource = source1 + "," + source2;
            FromSource = FromSource.Replace("{$##", "{").Replace("##$}", "}");
            retQr.SetDataSource(FromSource);
            retQr.Params.AddRange(qr1.GetParams());
            retQr.Params.AddRange(Params);
            retQr.Params.AddRange(qr2.GetParams());
            return retQr;
        }
        /// <summary>
        /// Make Query set from class and tablename
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <returns></returns>
        internal static QuerySet<T> From<T>(ICompiler Provider, string TableName)
        {
            if (Globals.DbType == DbTypes.None)
            {
                var t = typeof(Settings).FullName;
                throw new Exception(string.Format(@"It looks like you forgot call {0}.SetConnectionString", t));
            }
            return new QuerySet<T>(Provider, TableName);
        }


    }
}
