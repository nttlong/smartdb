using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LV.Db.Common
{
    internal class QProvider : IQueryProvider
    {
        private object querySet;
        
        public QProvider(object querySet)
        {
            this.querySet = querySet;
        }


        //[System.Diagnostics.DebuggerStepThrough]
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var t = typeof(TElement);
            var mc = expression as MethodCallExpression;
            BaseQuerySet qr1 = null;
            BaseQuerySet qr2 = null;
            LambdaExpression selector = null;
            LambdaExpression joinClause = null;
            var isLeftJoin = false;
            if (mc.Method.Name == "Skip")
            {
                var qr = utils.Resolve(mc.Arguments[0]).Value as QuerySet<TElement>;
                var val = utils.Resolve(mc.Arguments[1]).Value;
                var retQr = qr.Clone<TElement>();
                retQr.skipNumber = (int)val;
                return retQr;
            }
            if (mc.Method.Name == "Take")
            {
                var qr = utils.Resolve(mc.Arguments[0]).Value as QuerySet<TElement>;
                var val = utils.Resolve(mc.Arguments[1]).Value;
                var retQr = qr.Clone<TElement>();
                retQr.selectTop = (int)val;
                if (retQr.IsSubQueryOnNextPhase())
                {
                    retQr.SetDataSource("("+qr.ToSQLString()+") as "+qr.compiler.GetQName("sql"));
                    retQr.unionSQLs.Clear();
                    retQr.fields.Clear();
                    retQr.where = null;
                    retQr.groupbyList.Clear();
                    retQr.having = null;
                    retQr.mapAlias = typeof(TElement).GetProperties()
                        .Select(p => new MapAlais {
                            AliasName="sql",
                            Member=p,
                            ParamExpr=Expression.Parameter(typeof(TElement),"p")
                        }).ToList();
                    retQr.isSubQuery = true;
                    retQr.table_name = "sql";
                }
                return retQr;
            }
            if (mc.Method.Name == "OrderByDescending")
            {
                var qr = utils.Resolve(mc.Arguments[0]).Value as BaseQuerySet;
                if (qr.IsSubQueryOnNextPhase())
                {
                    var retQr = new QuerySet<TElement>(true);
                    retQr.dbContext = qr.dbContext;
                    retQr.compiler = qr.compiler;
                    retQr.table_name = "sql";
                    retQr.SetDataSource("(" + qr.ToSQLString() + ") " + retQr.compiler.GetQName("sql"));
                    retQr.Params.AddRange(qr.Params);
                    retQr.mapAlias = typeof(TElement).GetProperties()
                        .Select(p => new MapAlais
                        {
                            AliasName = "sql",
                            Member = p,
                            ParamExpr = Expression.Parameter(p.PropertyType, "p")

                        }).ToList();
                    retQr.SortBy(retQr, mc.Arguments[1], "desc");
                    
                    return retQr;
                }
                var retOrderBy = new QuerySet<TElement>(true);
                qr.SortBy(retOrderBy as BaseQuerySet, mc.Arguments[1], "desc");
                return retOrderBy;
            }
            if (mc.Method.Name == "Distinct")
            {
                var qr = utils.Resolve(mc.Arguments[0]).Value as BaseQuerySet;
                if (qr.IsSubQueryOnNextPhase())
                {
                    var retQr = new QuerySet<TElement>(true);
                    retQr.dbContext = qr.dbContext;
                    retQr.compiler = qr.compiler;
                    retQr.table_name = "sql";
                    retQr.SetDataSource("("+qr.ToSQLString()+") "+retQr.compiler.GetQName("sql"));
                    retQr.Params.AddRange(qr.Params);
                    retQr.isSelectDistinct = true;
                    retQr.mapAlias = typeof(TElement).GetProperties()
                        .Select(p => new MapAlais {
                            AliasName="sql",
                            Member=p,
                            ParamExpr=Expression.Parameter(p.PropertyType,"p")

                        }).ToList();
                    return retQr;
                }
                else
                {
                    var retQr = new QuerySet<TElement>(true);
                    qr.CopyTo(retQr, retQr.elemetType);
                    retQr.isSelectDistinct = true;
                    return retQr;
                }
                
            }
            if (mc.Arguments.Count==1 && mc.Arguments[0] is ConstantExpression)
            {
                ///That means select
                var qr =utils.Resolve(mc.Arguments[0]).Value as IQueryable<TElement>;
                return qr;
            }
            if (mc.Arguments.Count == 3 && 
                mc.Arguments[1] is UnaryExpression &&
                ((UnaryExpression)mc.Arguments[1]).Operand is LambdaExpression &&
                ((LambdaExpression)((UnaryExpression)mc.Arguments[1]).Operand).Body is MemberExpression)
            {
                if (mc.Method.Name == "SelectMany")
                {
                    qr1 = utils.Resolve(mc.Arguments[0]).Value as BaseQuerySet;
                    if (mc.Arguments[1] is UnaryExpression)
                    {
                        var unx = mc.Arguments[1] as UnaryExpression;
                        var ldx = unx.Operand as LambdaExpression;
                        var rmb = ldx.Body as MemberExpression;
                        qr2 = utils.Resolve(rmb).Value as BaseQuerySet;
                    }
                    else if (mc.Arguments[1] is ConstantExpression)
                    {
                        qr2 = utils.Resolve(mc.Arguments[1]).Value as BaseQuerySet;
                    }
                    var ux = mc.Arguments[2] as UnaryExpression;
                    selector = ((LambdaExpression)ux.Operand);
                    var retCrosQr = QueryBuilder.BuilJoinFromBase<TElement>(qr1 as BaseQuerySet, qr2 as BaseQuerySet, null, selector, "");
                    return retCrosQr;
                }
                if (mc.Method.Name == "GroupBy")
                {
                    if (mc.Arguments[0] is ConstantExpression)
                    {
                       
                        qr1 = utils.Resolve(mc.Arguments[0]).Value as BaseQuerySet;
                        

                    }
                    if(mc.Arguments[1] is UnaryExpression)
                    {
                        var groupExpr = (mc.Arguments[1] as UnaryExpression).Operand;
                    }
                    if (mc.Arguments[2] is UnaryExpression)
                    {
                        selector = (mc.Arguments[2] as UnaryExpression).Operand as LambdaExpression;
                    }
                    var retQr = new QuerySet<TElement>(true);
                    qr1.CopyTo(retQr, typeof(TElement));
                    retQr.SelectByExpression(retQr,selector);
                    //new System.Linq.Expressions.Expression.UnaryExpressionProxy(mc.Arguments[1]).Operand
                }
            }
            if (mc.Arguments.Count == 5)
            {
                if (mc.Method.Name == "Join")
                {
                    
                    
                    qr1 = utils.Resolve(mc.Arguments[0]).Value as BaseQuerySet;
                    qr2 = utils.Resolve(mc.Arguments[1]).Value as BaseQuerySet;
                    var fieldMappings = qr1.FieldsMapping.Union(qr2.FieldsMapping).ToList();
                    var joinTimes = (qr1.joinTimes + 1) * 10 + qr2.joinTimes;
                    var ltName = "l" + joinTimes.ToString();
                    var rtName = "r" + joinTimes.ToString();
                    var leftKey = mc.Arguments[2];
                    var rightKey = mc.Arguments[3];

                    var leftKeys = new List<string>();
                    var rightKeys = new List<string>();
                    var joinParams = new List<object>();
                    var extarAlias = new List<MapAlais>();
                    if (leftKey is UnaryExpression)
                    {
                        var ux = leftKey as UnaryExpression;
                        var bExpr = ((LambdaExpression)ux.Operand).Body;

                        var mAlias = new List<MapAlais>();

                        mAlias.Add(new MapAlais()
                        {
                            AliasName = ltName,
                            ParamExpr = ((LambdaExpression)ux.Operand).Parameters[0],
                            Children = qr1.mapAlias,
                            Member=((MemberExpression)((LambdaExpression)ux.Operand).Body).Member
                        });
                        if (bExpr is NewExpression)
                        {
                            var nx = bExpr as NewExpression;
                            var F = Compiler.GetFields(qr1.compiler,nx.Arguments.Select(p => new QueryMemberInfo()
                            {
                                Expression = p,
                                Member = nx.Members[nx.Arguments.IndexOf(p)]
                            }).ToList(), fieldMappings, mAlias, new List<ExtraSelectAll>(), ref joinParams);
                            leftKeys.AddRange(F.Select(p => p.Field).ToArray());


                        }
                        else
                        {


                            var F = Compiler.GetField(qr1.compiler,bExpr, fieldMappings, mAlias, ref joinParams);
                            leftKeys.Add(F);
                        }
                        extarAlias.AddRange(mAlias);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    if (rightKey is UnaryExpression)
                    {
                        var ux = rightKey as UnaryExpression;
                        var bExpr = ((LambdaExpression)ux.Operand).Body;

                        var mAlias = new List<MapAlais>();
                        mAlias.Add(new MapAlais()
                        {
                            AliasName = rtName,
                            ParamExpr = ((LambdaExpression)ux.Operand).Parameters[0],
                            Children = qr2.mapAlias,
                            Member = ((MemberExpression)((LambdaExpression)ux.Operand).Body).Member
                        });
                        if (bExpr is NewExpression)
                        {
                            var nx = bExpr as NewExpression;
                            var F = Compiler.GetFields(qr1.compiler,nx.Arguments.Select(p => new QueryMemberInfo()
                            {
                                Expression = p,
                                Member = nx.Members[nx.Arguments.IndexOf(p)]
                            }).ToList(), fieldMappings, mAlias, new List<ExtraSelectAll>(), ref joinParams);
                            rightKeys.AddRange(F.Select(p => p.Field).ToArray());


                        }
                        else
                        {

                            var F = Compiler.GetField(qr1.compiler,bExpr, fieldMappings, mAlias, ref joinParams);
                            rightKeys.Add(F);
                        }
                        extarAlias.AddRange(mAlias);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    var retQr = QueryBuilder.BuilJoinFromBase<TElement>(qr1, qr2, joinParams, leftKeys, rightKeys, mc.Arguments[4], extarAlias, isLeftJoin,true);
                    if(((LambdaExpression)((UnaryExpression)mc.Arguments[4]).Operand).Body is MemberInitExpression)
                    {
                        retQr.fields = retQr.fields.Where(p => p.Field.IndexOf(".*") == -1).ToList();
                    }
                    return retQr;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            if (mc.Method.Name == "OrderBy")
            {
                var qr = utils.Resolve(mc.Arguments[0]).Value as BaseQuerySet;
                var retOrderBy = new QuerySet<TElement>(true);
                qr.SortBy(retOrderBy as BaseQuerySet, mc.Arguments[1],"asc");
                return retOrderBy;
            }
            if (mc.Method.Name == "ThenByDescending")
            {
                var qr = utils.Resolve(mc.Arguments[0]).Value as BaseQuerySet;
                var retOrderBy = new QuerySet<TElement>(true);
                qr.SortBy(retOrderBy as BaseQuerySet, mc.Arguments[1], "desc");
                return retOrderBy;
            }
            if (mc.Method.Name == "Select")
            {
                var qr =utils.Resolve(mc.Arguments[0]).Value as BaseQuerySet;
                var retSelect = new QuerySet<TElement>(true);
                qr.SelectByExpression(retSelect as BaseQuerySet,mc.Arguments[1]);
                return retSelect;
            }
            if (mc.Method.Name == "SelectMany")
            {
                var m = ((MethodCallExpression)((LambdaExpression)((UnaryExpression)mc.Arguments[1]).Operand).Body).Method;
                
                if (m.Name== "DefaultIfEmpty")
                {
                    qr1 = utils.Resolve(mc.Arguments[0]).Value as BaseQuerySet;
                    qr2 = utils.Resolve(((MethodCallExpression)((MethodCallExpression)((LambdaExpression)((UnaryExpression)mc.Arguments[1]).Operand).Body).Arguments[0]).Arguments[0]).Value as BaseQuerySet; 
                    var joinExpression = ((MethodCallExpression)((MethodCallExpression)((LambdaExpression)((UnaryExpression)mc.Arguments[1]).Operand).Body).Arguments[0]).Arguments[1];
                    var retQr = QueryBuilder.BuilJoinFromBase<TElement>(qr1, qr2, (LambdaExpression)((UnaryExpression)joinExpression).Operand, (LambdaExpression)((UnaryExpression)mc.Arguments[2]).Operand, "left");
                    return retQr;
                }
                if (m.Name == "Where")
                {
                    qr1 = utils.Resolve(mc.Arguments[0]).Value as BaseQuerySet;
                    qr2 = utils.Resolve(((MethodCallExpression)((LambdaExpression)((UnaryExpression)mc.Arguments[1]).Operand).Body).Arguments[0]).Value as BaseQuerySet;
                    var joinExpression = ((MethodCallExpression)((LambdaExpression)((UnaryExpression)mc.Arguments[1]).Operand).Body).Arguments[1];
                    var retQr = QueryBuilder.BuilJoinFromBase<TElement>(qr1, qr2, (LambdaExpression)((UnaryExpression)joinExpression).Operand, (LambdaExpression)((UnaryExpression)mc.Arguments[2]).Operand,"inner");
                    return retQr;
                }
                throw new NotImplementedException();
            }
            if (mc.Method.Name == "Where")
            {
                var qr = utils.Resolve(mc.Arguments[0]).Value as QuerySet<TElement>;
                var retQr = qr.Clone<TElement>();
                var retQrParams = retQr.Params;
                var ux = mc.Arguments[1] as UnaryExpression;
                var ldx = ux.Operand as LambdaExpression;
                retQr.mapAlias.Add(new MapAlais()
                {
                    ParamExpr = ldx.Parameters[0],
                    Schema = retQr.Schema,
                    TableName = retQr.table_name,
                    Children = retQr.mapAlias
                });
                var fx= Expression.Lambda<Func<TElement, bool>>(((LambdaExpression)((UnaryExpression)mc.Arguments[1]).Operand).Body, ((LambdaExpression)((UnaryExpression)mc.Arguments[1]).Operand).Parameters[0]);
                var isWillRunAsSubQuery = false;
                var strCon= qr.MakeLogical<TElement>(retQr, fx, true,ref isWillRunAsSubQuery);
                if (isWillRunAsSubQuery||string.IsNullOrEmpty(retQr.where))
                {
                    retQr.where = strCon;
                }
                else
                {
                    retQr.where = "("+ retQr.where+") and ("+strCon+")";
                }
                retQr.Params = retQrParams;
                
                return retQr;
            }
            if (mc.Method.Name == "Union")
            {
                var qrFirst = Expression.Lambda(mc.Arguments[0]).Compile().DynamicInvoke() as QuerySet<TElement>;
                var qrSecond = Expression.Lambda(mc.Arguments[1]).Compile().DynamicInvoke() as QuerySet<TElement>;
                return qrFirst.Union(qrSecond);

            }
            foreach (var x in mc.Arguments)
            {
                if(x is ConstantExpression)
                {
                    qr1 =utils.Resolve(x).Value as BaseQuerySet;
                }
                else if(x is UnaryExpression)
                {
                    var ux = x as UnaryExpression;
                    var P = ((LambdaExpression)ux.Operand).Parameters[0];
                    if (((LambdaExpression)ux.Operand).Body is MethodCallExpression)
                    {
                        var mc1 = ((LambdaExpression)ux.Operand).Body as MethodCallExpression;
                        var args = mc1.Arguments.Count;
                        if (args ==0)
                        {
                            qr2 = utils.Resolve(mc1).Value as BaseQuerySet;
                        }
                        if (args == 1)
                        {
                            if(mc1.Arguments[0] is MethodCallExpression)
                            {
                                var mc2 = mc1.Arguments[0] as MethodCallExpression;
                                isLeftJoin = true;
                                qr2 = utils.Resolve(mc2.Arguments[0]).Value as BaseQuerySet;
                                joinClause = ((UnaryExpression)mc2.Arguments[1]).Operand as LambdaExpression;
                            }
                        }
                        if (args == 2)
                        {
                            if(mc1.Method.ReflectedType == typeof(SqlFuncs))
                            {

                            }
                            else
                            {
                                qr2 = utils.Resolve(mc1.Arguments[0]).Value as BaseQuerySet;
                                joinClause = ((UnaryExpression)mc1.Arguments[1]).Operand as LambdaExpression;
                            }
                            
                        }
                        
                    }
                    else if (((LambdaExpression)ux.Operand).Body is NewExpression)
                    {
                        selector = ((LambdaExpression)ux.Operand);
                    }
                    else
                    {
                        if (((BaseQuerySet)qr1).elemetType == typeof(TElement))
                        {
                            var ret1 = ((QuerySet<TElement>)qr1).Clone<TElement>();
                            var Params2 = ret1.Params;
                            ret1.mapAlias.Add(new MapAlais()
                            {
                                ParamExpr = ((LambdaExpression)ux.Operand).Parameters[0],
                                TableName = ret1.table_name,
                                Schema = ret1.Schema,
                                AliasName = ret1.Schema

                            });

                            var strCond = Compiler.GetLogicalExpression(ret1.compiler,((LambdaExpression)ux.Operand).Body,ret1.FieldsMapping, ret1.mapAlias, ref Params2);
                            if (!string.IsNullOrEmpty(qr1.where))
                            {
                                ret1.where = "("+ qr1.where+") and ("+strCond+")";
                            }
                            else
                            {
                                ret1.where = strCond;
                            }
                            ret1.Params = Params2;
                            return ret1;
                        }
                        
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }

                    //var qr2 = Expression.Lambda(P).Compile(true);

                }
                else
                {

                }
            }
            var Params1 = ((BaseQuerySet)qr1).Params.ToList();
                Params1.AddRange(((BaseQuerySet)qr2).Params);
            var tmpAlais = ((BaseQuerySet)qr1).mapAlias.ToList();
            tmpAlais.AddRange(((BaseQuerySet)qr2).mapAlias);
            List<ParameterExpression> Params = utils.GetAllParamsExpr(joinClause).Where(p=>p!=joinClause.Parameters[0]).ToList();
            if (Params.Count == 1)
            {
                ((BaseQuerySet)qr2).mapAlias.Add(new MapAlais()
                {
                    TableName = ((BaseQuerySet)qr2).table_name,
                    Schema = ((BaseQuerySet)qr2).Schema,
                    ParamExpr = Params[0]

                });
            }
            else
            {
                throw new NotImplementedException();
            }
            //var strCond1 = Compiler.GetLogicalExpression(joinClause, tmpAlais, ref Params1);
            var ret = QueryBuilder.BuilJoinFromBase<TElement>(qr1 as BaseQuerySet, qr2 as BaseQuerySet, joinClause, selector, isLeftJoin ? "left": "inner");
            return ret;
            
        }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }
       // [System.Diagnostics.DebuggerStepThrough]
        public TResult Execute<TResult>(Expression Expr)
        {
            try
            {
                if (Expr is MethodCallExpression)
                {
                    var cx = Expr as MethodCallExpression;
                    if (cx.Method.Name == "FirstOrDefault")
                    {
                        var qr = utils.Resolve(cx.Arguments[0]).Value as QuerySet<TResult>;
                        try
                        {
                            
                            if (cx.Arguments.Count() == 1)
                            {
                                return qr.AsQuerySet().ToList().FirstOrDefault();
                            }
                            else
                            {
                                var oldParams = qr.Params.ToList();
                                var Params = qr.Params;
                                //new System.Linq.Expressions.Expression.LambdaExpressionProxy(new System.Linq.Expressions.Expression.UnaryExpressionProxy(cx.Arguments[1]).Operand).Parameters
                                var ux = cx.Arguments[1] as UnaryExpression;
                                var ldx = ux.Operand as LambdaExpression;
                                var mapAlias = new List<MapAlais>();
                                if (qr.fields.Count(p => p.Field != "*" && p.Field.IndexOf(".*") == -1) > 0)
                                {
                                    var mbxs = utils.GetAllMemberExpression(cx.Arguments[1]);
                                    mapAlias = mbxs.Select(p => new MapAlais
                                    {
                                        TableName = "sql",
                                        Member = p.Member,
                                        ParamExpr = p.Expression as ParameterExpression
                                    }).ToList();
                                    var qrTmp = qr.Clone<TResult>();
                                    qrTmp.SetDataSource("(" + qr.ToSQLString() + ") " + qr.compiler.GetQName("sql"));
                                    qrTmp.table_name = "sql";
                                    var strWhere1 = Compiler.GetLogicalExpression(qr.compiler, cx.Arguments[1], qr.FieldsMapping, mapAlias, ref Params);
                                    qrTmp.Params = Params;
                                    qrTmp.where = strWhere1;
                                    qrTmp.fields.Clear();
                                    var ret1 = qrTmp.AsQuerySet().ToList().FirstOrDefault();
                                    return ret1;
                                }
                                else
                                {
                                    mapAlias.Add(new MapAlais()
                                    {
                                        Children = qr.mapAlias,
                                        ParamExpr = ldx.Parameters[0],
                                        Schema = qr.Schema,
                                        TableName = qr.table_name
                                    });
                                }

                                var strWhere = Compiler.GetLogicalExpression(qr.compiler, cx.Arguments[1], qr.FieldsMapping, mapAlias, ref Params);
                                var tmpWhere = qr.where;
                                if (string.IsNullOrEmpty(qr.where))
                                {
                                    qr.where = strWhere;
                                }
                                else
                                {
                                    qr.where = "(" + qr.where + ") and (" + strWhere + ")";

                                }
                                var ret = qr.AsQuerySet().ToList().FirstOrDefault();
                                qr.Params = oldParams;
                                qr.where = tmpWhere;
                                return ret;
                            }
                        }
                        catch (Exception ex)
                        {

                            throw (new Exceptions.ExcutationError(ex.Message+"/r/n Sql:"+qr.ToString())
                            {
                                Sql=qr.ToString(),
                                SqlWithParams=qr.ToSQLString(),
                                Params=qr.Params
                            });
                        }

                    }
                    if (cx.Method.Name == "Max" ||
                        cx.Method.Name == "Min" ||
                        cx.Method.Name == "Sum" ||
                        cx.Method.Name == "Average" ||
                        cx.Method.Name == "Count")
                    {


                        var qr = utils.Resolve(cx.Arguments[0]).Value as BaseQuerySet;
                        try
                        {
                            if (cx.Arguments.Count() == 2)
                            {
                                var mb = ((LambdaExpression)((UnaryExpression)cx.Arguments[1]).Operand).Body as MemberExpression;
                                ParameterExpression parameter = Expression.Parameter
                                (
                                    mb.Member.DeclaringType,
                                    "m"
                                );
                                MemberExpression member = Expression.MakeMemberAccess
                                (
                                    parameter,
                                    mb.Member
                                );
                                var cMb = Expression.Convert(member, typeof(object));
                                var mbObj = Expression.Call(null, typeof(SqlFuncs).GetMethods().FirstOrDefault(p => p.Name == cx.Method.Name && p.IsGenericMethod == false), cMb);


                                var fx = Expression.Lambda(mbObj, parameter);
                                var qrRet = new QuerySet<TResult>(false);
                                qr.CopyTo(qrRet, qr.elemetType);
                                ((BaseQuerySet)qrRet).SelectByExpression(qr, fx);
                                return qrRet.Item();


                            }
                            else if(cx.Arguments.Count() == 1){
                                var ret = qr.dbContext.ExecToList<int>(qr, "select count(*) from (" + qr.ToSQLString() + ") " + qr.compiler.GetQName("sql"),
                                    qr.Params.Cast<ParamConst>().ToArray() );
                                object obj= ret.FirstOrDefault();
                                return (TResult)obj;
                            }
                        }
                        catch (Exception ex)
                        {

                            throw (new Exceptions.ExcutationError(ex.Message + "/r/n Sql:" + qr.ToString())
                            {
                                Sql = qr.ToString(),
                                SqlWithParams = qr.ToSQLString(),
                                Params = qr.Params
                            });
                        }
                    }
                }
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {

                throw(ex);
            }
        }
    }
}