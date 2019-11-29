using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LV.Db.Common
{
    public abstract class BaseQuerySet
    {
        internal LambdaExpression latestSelector;
        internal ICompiler compiler { get; set; }
        internal string function_Schema { get; set; }
        internal string function_call { get; set; }
        internal List<FieldMapping> FieldsMapping = new List<FieldMapping>();
        internal List<QueryFieldInfo> fields { get; set; }
        /// <summary>
        /// Sometime the query parser generate parameter instead of constant
        /// Example:
        /// p=>new {p.firtsname+" "+p.lastname} 
        /// will generate a struct looks like:
        /// 'select firstname+{0}+lastname' with {0} is position of parameter in this list
        /// </summary>
        public List<object> Params { get; set; }

        internal QueryJoinObject JoinObject { get; set; }
        public Type elemetType { get; internal set; }
        public Factory factory { get; internal set; }

        internal int joinTimes;
        internal string table_name;
        internal string where = null;
        internal string Schema = null;


        internal List<PropertyMapping> PropertiesMapping = new List<PropertyMapping>();
        internal List<TableAliasInfo> tableAlias = new List<TableAliasInfo>();
        internal List<MapAlais> mapAlias = new List<MapAlais>();
        internal string having;
        internal int subQueryCount;

        //internal string table_alias;
        internal string datasource
        {
            get
            {
                return _dataSource;
            }
            set
            {
                if (value != null)
                {
                    _dataSource = value;
                }
                else
                {
                    _dataSource = value;
                }
            }
        }
        //internal int sequenceJoin;
        internal List<SortingInfo> sortList = new List<SortingInfo>();
        internal List<QueryFieldInfo> groupbyList = new List<QueryFieldInfo>();


        //internal List<object> parametersExpr;
        //internal object[] table_names;
        internal List<object> fieldParams;
        internal bool isFromNothing;

        internal int selectTop;
        internal int skipNumber;
        internal DBContext dbContext;
        internal bool isSubQuery
        {
            get
            {
                return _isSubQr;
            }
            set
            {
                if (value)
                {
                    this.Schema = null;
                }
                _isSubQr = value;
            }
        }

        internal List<UnionSQLInfo> unionSQLs = new List<UnionSQLInfo>();
        internal bool hasExpressionInSelector;
        internal List<string> strJoinClause = new List<string>();

        internal bool isReadOnly;
        internal bool isSelectDistinct;
        private string _dataSource;
        private bool isQueryDataSource;
        private bool _isSubQr;

        internal bool IsSubQueryOnNextPhase()
        {
            return (!string.IsNullOrEmpty(this.where))
                || this.groupbyList.Count > 0
                || this.selectTop > 0
                || this.skipNumber > 0
                || this.isSubQuery
                || (this.unionSQLs != null) && (this.unionSQLs.Count > 0)
                || this.isSelectDistinct;
        }
        internal bool IsSubQueryOnNextPhaseWithSelectedFields()
        {
            return (!string.IsNullOrEmpty(this.where))
                || this.groupbyList.Count > 0
                || this.selectTop > 0
                || this.skipNumber > 0
                || this.isSubQuery
                || (this.unionSQLs != null) && (this.unionSQLs.Count > 0)
                || this.isSelectDistinct
                || this.fields.Where(p => p.Field.IndexOf(".*") == -1).Count() > 0;
        }
        public abstract string ToSQLString();

        internal BaseQuerySet CopyTo(BaseQuerySet ret, Type type)
        {
            ret.compiler = this.compiler;
            ret.function_Schema = this.function_Schema;
            ret.function_call = this.function_call;
            ret.elemetType = type;
            ret.isSelectDistinct = this.isSelectDistinct;
            ret.FieldsMapping.AddRange(this.FieldsMapping);
            ret.isReadOnly = this.isReadOnly;
            ret.hasExpressionInSelector = this.hasExpressionInSelector;
            ret.PropertiesMapping.AddRange(this.PropertiesMapping);
            ret.mapAlias.AddRange(this.mapAlias);
            ret.subQueryCount = this.subQueryCount;
            ret.unionSQLs.AddRange(this.unionSQLs);
            ret.isSubQuery = this.isSubQuery;
            ret.dbContext = this.dbContext;
            ret.Params.AddRange(this.Params);
            ret.datasource = this.datasource;
            ret.table_name = this.table_name;
            ret.fields.AddRange(this.fields);
            ret.isFromNothing = this.isFromNothing;
            ret.Schema = this.Schema;
            ret.strJoinClause.AddRange(this.strJoinClause);
            ret.where = this.where;
            ret.having = this.having;
            ret.sortList.AddRange(this.sortList);
            ret.groupbyList.AddRange(this.groupbyList);
            ret.strJoinClause = this.strJoinClause;
            ret.tableAlias.AddRange(this.tableAlias);
            ret.joinTimes = this.joinTimes;
            if (this.fieldParams != null)
            {
                ret.fieldParams = new List<object>();
                ret.fieldParams.AddRange(this.fieldParams);
            }
            ret.latestSelector = this.latestSelector;
            return ret;
        }
        //[System.Diagnostics.DebuggerStepThrough]
        internal List<QueryFieldInfo> ExtractFieldsList(ICompiler Cmp, BaseQuerySet ret, LambdaExpression expr, ref List<MapAlais> newMapAlias, ref bool hasAliasOrExpr)
        {
            hasAliasOrExpr = utils.CheckIsExpressionHasAliasOrBinaryExpression(expr.Body);


            if (ret.unionSQLs.Count > 0)
            {
                ret.datasource = ret.ToSQLString();
                ret.table_name = "u" + ret.unionSQLs.Count;
                ret.unionSQLs.Clear();
                ret.isSubQuery = true;
            }
            var Params = this.Params;

            if (hasAliasOrExpr)
            {

                newMapAlias.Add(new MapAlais()
                {
                    Children = ret.mapAlias,
                    ParamExpr = expr.Parameters[0],
                    TableName = ret.table_name,
                    Schema = ret.Schema
                });

            }
            if (ret.table_name != null)
            {
                newMapAlias.Add(new MapAlais()
                {
                    TableName = ret.table_name,
                    Schema = ret.Schema,
                    ParamExpr = expr.Parameters[0]
                });
            }
            else if (newMapAlias.Count == 0)
            {
                newMapAlias.AddRange(ret.mapAlias);
            }
            List<QueryMemberInfo> nx = null;
            if (expr.Body is NewExpression)
            {
                nx = ((NewExpression)expr.Body).Arguments.Select(p => new QueryMemberInfo()
                {
                    Expression = p,
                    Member = ((NewExpression)expr.Body).Members[((NewExpression)expr.Body).Arguments.IndexOf(p)]
                }).ToList();
                newMapAlias.AddRange(ret.mapAlias);
            }
            if (expr.Body is MemberInitExpression)
            {
                nx = ((MemberInitExpression)expr.Body).Bindings.Select(p => new QueryMemberInfo()
                {
                    Expression = ((MemberAssignment)p).Expression,
                    Member = p.Member
                }).ToList();
            }
            if (expr.Body is MemberExpression)
            {
                var mb = expr.Body as MemberExpression;
                var F = mb.Member.Name;
                var fm = ret.FieldsMapping.FirstOrDefault(p => p.Member == mb.Member);
                if (fm != null)
                {
                    F = fm.FieldName;
                }

                var pm = ret.PropertiesMapping.FirstOrDefault(p => p.Property == ((MemberExpression)expr.Body).Member);
                if (pm != null)
                {
                    if (utils.CheckIfPrimitiveType(((PropertyInfo)mb.Member).PropertyType))
                    {
                        return (new QueryFieldInfo[] {
                        new QueryFieldInfo()
                        {

                            Field=compiler.GetFieldName(pm.Schema,pm.TableName,F)
                        }

                        }).ToList();
                    }
                    else
                    {
                        return (new QueryFieldInfo[] {
                        new QueryFieldInfo()
                        {

                            Field=compiler.GetQName(pm.Schema,pm.AliasName?? pm.TableName)+".*"
                        }

                        }).ToList();
                    }

                }
                else
                {
                    return (new QueryFieldInfo[] {
                        new QueryFieldInfo()
                        {

                            Field=compiler.GetFieldName(ret.Schema,ret.table_name,F)
                        }

                    }).ToList();
                }

            }
            if (expr.Body is ParameterExpression)
            {
                var map = utils.FindAliasByParameterType(newMapAlias, expr.Body.Type);
                return (new QueryFieldInfo[] {new QueryFieldInfo(){
                    Field=Cmp.GetQName((map.AliasName??map.TableName))+".*"
                }}).ToList();

            }
            if (expr.Body is MethodCallExpression)
            {
                Params = ret.Params;
                var tmpAlias = ret.mapAlias.ToList();
                tmpAlias.Add(new MapAlais()
                {
                    Children = ret.mapAlias,
                    ParamExpr = expr.Parameters[0],
                    Schema = ret.Schema,
                    TableName = ret.table_name
                });

                var field = Compiler.GetField(ret.compiler, expr.Body, ret.FieldsMapping, tmpAlias, ref Params);
                var retList = new List<QueryFieldInfo>();
                retList.Add(new QueryFieldInfo()
                {
                    Field = field
                });
                return retList;
            }
            if (expr.Body is BinaryExpression)
            {
                Params = ret.Params;
                var tmpAlias = ret.mapAlias.ToList();
                tmpAlias.Add(new MapAlais()
                {
                    Children = ret.mapAlias,
                    ParamExpr = expr.Parameters[0],
                    Schema = ret.Schema,
                    TableName = ret.table_name
                });

                var field = Compiler.GetField(ret.compiler, expr.Body, ret.FieldsMapping, tmpAlias, ref Params);
                var retList = new List<QueryFieldInfo>();
                retList.Add(new QueryFieldInfo()
                {
                    Field = field
                });
                return retList;
            }
            var retFields = Compiler.GetFields(ret.compiler, nx, ret.FieldsMapping, newMapAlias, new List<ExtraSelectAll>(), ref Params);
            return retFields;
        }
        //[System.Diagnostics.DebuggerStepThrough]
        internal BaseQuerySet SelectByExpression(BaseQuerySet ret, Expression expr)
        {
            this.CopyTo(ret, ret.elemetType);

            if (ret.latestSelector!=null && ret.latestSelector.Body is NewExpression)
            {

                var mx = ret.latestSelector.Body as NewExpression;
                if (mx.Arguments.Count(p => p is MethodCallExpression) > 0)
                {
                    ret.mapAlias = mx.Members.Select(p => new MapAlais
                    {
                        Member = p,
                        ParamExpr = Expression.Parameter(ret.elemetType),
                        TableName = "sql"

                    }).ToList();
                    ret._dataSource = "("+ret.ToSQLString()+") "+ret.compiler.GetQName("sql");
                    ret.isQueryDataSource = true;
                    ret.fields.Clear();
                    ret.groupbyList.Clear();
                    ret.isReadOnly = true;
                    ret.Schema = null;
                    ret.table_name = "sql";
                    ret.hasExpressionInSelector = false;
                    ret.having = null;
                    ret.isSelectDistinct = false;
                    ret.isSubQuery = false;
                    ret.JoinObject = null;
                    ret.selectTop = -1;
                    ret.skipNumber = -1;
                    ret.sortList.Clear();
                    ret.strJoinClause.Clear();
                    ret.subQueryCount = 0;
                    ret.unionSQLs.Clear();
                    ret.where = null;
                    ret._isSubQr = false;
                    
                }
            }
            var selector = expr as LambdaExpression;
            if (expr is UnaryExpression)
            {
                var ux = expr as UnaryExpression;
                selector = ((LambdaExpression)ux.Operand);
            }
            var hasAliasOrExpr = false;
            var newMapAlias = new List<MapAlais>();

            ret.Params = Params;
            ret.fields.Clear();
            var fs = this.ExtractFieldsList(this.compiler, ret, selector as LambdaExpression, ref newMapAlias, ref hasAliasOrExpr);
            ret.fields.AddRange(fs);
            ret.hasExpressionInSelector = hasAliasOrExpr;
            
            var argFields = ret.fields.Where(p => p.FnCall != null && SqlFuncs.AggregateFuncs.IndexOf("," + p.FnCall + ",",StringComparison.OrdinalIgnoreCase) > -1).ToList();
            if (argFields.Count > 0)
            {
                if (ret.groupbyList==null||ret.groupbyList.Count == 0)
                {

                    var groupByFields = ret.fields.Where(p => p.FnCall == null || SqlFuncs.AggregateFuncs.IndexOf("," + p.FnCall + ",", StringComparison.OrdinalIgnoreCase) == -1).ToList();
                    ret.groupbyList = groupByFields.Select(p => new QueryFieldInfo()
                    {
                        Field = p.Field
                    }).ToList();


                    if (ret.groupbyList.Count > 0)
                    {
                        if (this.IsSubQueryOnNextPhase())
                        {
                            if (!ret.isQueryDataSource)
                            {
                                ret.datasource = "(" + ret.datasource + ") as " + ret.compiler.GetQName(ret.table_name);
                            }
                            else
                            {
                                ret.datasource = "(select "+ ret.compiler.GetQName(ret.table_name) +".* from " + ret.datasource + ") as " + ret.compiler.GetQName(ret.table_name);
                            }
                        }
                        ret.mapAlias = ret.elemetType.GetProperties().Select(p => new MapAlais
                        {
                            AliasName = ret.table_name,
                            Member = p,
                            ParamExpr = selector.Parameters[0]
                        }).ToList();
                        

                    }
                    return ret;
                }
            }
            if (hasAliasOrExpr)
            {
                ret.subQueryCount++;
                //ret.fields.Clear();
            }
            if (hasAliasOrExpr)
            {
                ret.mapAlias = newMapAlias;
            }
            if (selector.Body is MemberInitExpression)
            {
                var nbx = selector.Body as MemberInitExpression;
                foreach (var x in nbx.Bindings)
                {

                    if (x is MemberAssignment)
                    {
                        var mba = x as MemberAssignment;

                        if ((mba.Expression is MemberExpression) &&
                            (((MemberExpression)mba.Expression).Expression is MemberExpression) &&
                            ((MemberExpression)((MemberExpression)mba.Expression).Expression).Expression is MemberExpression)

                        {
                            ParameterExpression px = utils.FindParameterExpression((MemberExpression)mba.Expression);
                            var mb = ((MemberExpression)((MemberExpression)((MemberExpression)mba.Expression)).Expression).Member;
                            var map = utils.FindAliasByExpression(ret.mapAlias, ((MemberExpression)((MemberExpression)mba.Expression)).Expression);
                            if (map != null)
                            {
                                ret.mapAlias.Add(new MapAlais()
                                {
                                    AliasName = map.AliasName,
                                    ParamExpr = px
                                });
                            }
                        }

                    }
                }
            }

            if (selector.Body is NewExpression)
            {
                var nx = selector.Body as NewExpression;
                foreach (var x in nx.Arguments)
                {

                    if (x is MemberExpression && ((MemberExpression)x).Expression is MemberExpression)
                    {
                        var mb = ((MemberExpression)((MemberExpression)x).Expression).Member;
                        var pp = this.PropertiesMapping.FirstOrDefault(p => p.Property == mb);
                        if (pp != null)
                        {
                            ret.mapAlias.Add(new MapAlais()
                            {
                                AliasName = pp.AliasName,
                                Member = nx.Members[nx.Arguments.IndexOf(x)],
                                Schema = pp.Schema,
                                TableName = pp.TableName
                            });
                            ret.PropertiesMapping.Add(new PropertyMapping()
                            {
                                AliasName = pp.AliasName,
                                OriginAliasName = pp.OriginAliasName,
                                Property = nx.Members[nx.Arguments.IndexOf(x)] as PropertyInfo,
                                Schema = pp.Schema,
                                TableName = pp.TableName
                            });
                        }
                    }
                    else if (x is ParameterExpression)
                    {
                        var test = utils.GetAllPropertiesOfType(x.Type);
                    }
                    else
                    {
                        ret.hasExpressionInSelector = true;
                        ret.isReadOnly = true;
                    }
                }
            }
            if (ret.unionSQLs.Count > 0)
            {
                ret._dataSource = "(" + ret._dataSource + ") " + ret.compiler.GetQName(this.table_name);
            }
            return ret;
        }


        internal BaseQuerySet SortBy(BaseQuerySet ret, Expression expr, string SortType)
        {
            this.CopyTo(ret, ret.elemetType);
            if (ret.tableAlias.Count == 0)
            {
                if (expr is UnaryExpression)
                {
                    var ux = expr as UnaryExpression;
                    var ldx = ux.Operand as LambdaExpression;
                    var mbx = ldx.Body as MemberExpression;
                    ret.mapAlias.Add(new MapAlais()
                    {
                        Member = mbx.Member,
                        ParamExpr = ldx.Parameters[0],
                        Schema = ret.Schema,
                        TableName = ret.table_name
                    });

                }
            }
            var mb = (MemberExpression)((LambdaExpression)((UnaryExpression)expr).Operand).Body;

            var Params = ret.Params;
            var mapAlias = new List<MapAlais>();

            var F = Compiler.GetField(compiler, mb, ret.FieldsMapping, ret.mapAlias, ref Params);
            ret.sortList.Add(new SortingInfo()
            {
                FieldName = F,
                SortType = SortType
            });
            return ret;
        }

    }
}