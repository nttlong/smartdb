
#if NET_CORE
           using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
#endif 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LV.Db.Common
{
    /// <summary>
    /// The library has been built for accessing across different types of RDBMS engine  with minimum effort such as: MS SQL server, PostgreSQL, MySQL and Oracle.
    /// The crucial function of the library is translated from Dot Net Lambda Expression into SQL which could be directly used in RDBMS engine
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QuerySet<T> : BaseQuerySet, IQueryable<T>, IOrderedQueryable<T>
    {
        

        Type IQueryable.ElementType => typeof(T);

        Expression IQueryable.Expression => Expression.Constant(this);
        [System.Diagnostics.DebuggerStepThrough]
        public DataPage<T2> GetPage<T2>(DataPageInfo<T> PageInfo)
        {
            if (PageInfo.Sort == null || PageInfo.Sort.Length == 0)
            {
                throw new Exception("It looks like you try to get a page of items without sorting");
            }
            var qr = this;
            var qrCount = this.Clone<T>();
            if (PageInfo.Filter != null)
            {
                qr = this.Filter(PageInfo.Filter);
                qrCount = qrCount.Filter(PageInfo.Filter);
            }
            qr = qr.MakeRowNumber(PageInfo.Sort.ToList());
            var ret = qr.GetPageOfItems<T2>(qrCount, PageInfo.PageSize, PageInfo.PageIndex);
            return ret;
        }
        public DataPage<T> GetPage(DataPageInfo<T> PageInfo)
        {
            if (PageInfo.Sort == null || PageInfo.Sort.Length == 0)
            {
                throw new Exception("It looks like you try to get a page of items without sorting");
            }
            var qr = this;
            var qrCount = this.Clone<T>();
            if (PageInfo.Filter != null)
            {
                qr = this.Filter(PageInfo.Filter);
                qrCount = qrCount.Filter(PageInfo.Filter);
            }
            qr = qr.MakeRowNumber(PageInfo.Sort.ToList());
            var ret = qr.GetPageOfItems<T>(qrCount, PageInfo.PageSize, PageInfo.PageIndex);
            return ret;
        }
        [System.Diagnostics.DebuggerStepThrough]
        private DataPage<T2> GetPageOfItems<T2>(QuerySet<T> QrCount, int pageSize, int pageIndex)
        {
            var ret = new DataPage<T2>();
            var retQr = this.Clone<T2>();
            var totalItem = QrCount.Count();
            var Params = this.Params.ToList();
            var sql = this.compiler.GetQName("SQLPager");
            var rowField = this.compiler.GetFieldName("", "SQLPager", "RowNumber");
            retQr.isSubQuery = true;
            retQr.table_name = "SQL";

            retQr.datasource = "(select * from (" + this.ToSQLString() + ") " + this.compiler.GetQName("", "SQLPager") +
                " where " + rowField + ">={" + Params.Count + "} and " + rowField + "<={" + (Params.Count + 1) + "}) as " + this.compiler.GetQName("SQL");
            var start = pageIndex * pageSize;
            var end = (pageIndex + 1) * pageSize;
            Params.Add(new ParamConst { Value = start, Type = typeof(int) });
            Params.Add(new ParamConst { Value = end, Type = typeof(int) });
            retQr.Params = Params;
            ret.TotalItems = totalItem;
            ret.PageSize = pageSize;
            ret.PageIndex = pageIndex;
            ret.TotalPages = (int)(ret.TotalItems / ret.PageSize);
            ret.Items = retQr.ToList();
            if (ret.TotalItems % ret.PageSize > 0)
            {
                ret.TotalPages++;
            }
            return ret;
        }

        public QuerySet<T> MakeRowNumber(List<DataPageInfoSorting> Sort)
        {
            var ret = this.Clone<T>();
            ret.where = "";
            ret.fields.Clear();
            var sortFields = new List<SQLDataPageInfoSorting>();
            foreach (var F in Sort)
            {
                var mp = this.mapAlias.FirstOrDefault(p => p.Member != null && p.Member.Name == F.Field);
                if (mp != null)
                {
                    sortFields.Add(new SQLDataPageInfoSorting()
                    {
                        Field = F.Field,
                        //Schema=mp.Schema,
                        TableName = "SQL",
                        IsDesc = F.IsDesc
                    });
                }
                else
                {
                    sortFields.Add(new SQLDataPageInfoSorting()
                    {
                        Field = F.Field,
                        IsDesc = F.IsDesc,
                        TableName = "SQL"
                    });
                }
            }

            ret.datasource = this.compiler.MakeRowNumber(this.ToSQLString(), sortFields);
            ret.datasource = "(" + ret.datasource + ") as " + this.compiler.GetQName("SQL");
            ret.isSubQuery = true;
            ret.table_name = "SQL";
            return ret;
        }

        IQueryProvider IQueryable.Provider => new QProvider(this);
        /// <summary>
        /// The table name will be map with Database
        /// </summary>

        public QuerySet<T> Union<T1>(QuerySet<T1> qr)
        {
            var ret = this.Clone<T>();
            return QueryBuilder.Union(ret, qr);
        }
        public QuerySet<T> UnionAll<T1>(QuerySet<T1> qr)
        {
            var ret = this.Clone<T>();
            return QueryBuilder.UnionAll(ret, qr);
        }

        public EntityMakeTable<T> MakeTable(string schema, string tableName)
        {
            var ret = new EntityMakeTable<T>(this.Clone<T>(), schema, tableName);
            return ret;
        }

        internal void MapFields(Expression<Func<T>> Expr)
        {
            var nx = Expr.Body as NewExpression;
            foreach (var x in nx.Arguments)
            {
                if (x is MethodCallExpression)
                {
                    var mc = x as MethodCallExpression;
                    if (mc.Method.ReflectedType == typeof(clsExtionsion))
                    {
                        var fieldNameVal = utils.Resolve(mc.Arguments[1]);
                        if (fieldNameVal == null)
                        {
                            throw new Exception(mc.Arguments[1].ToString() + " can not be null");
                        }
                        this.FieldsMapping.Add(new FieldMapping()
                        {
                            FieldName = fieldNameVal.ToString(),
                            Member = nx.Members[nx.Arguments.IndexOf(x)]
                        });
                    }
                    else if (mc.Method.ReflectedType == typeof(Factory))
                    {
                        var fieldNameVal = utils.Resolve(mc.Arguments[0]);
                        if (fieldNameVal == null)
                        {
                            throw new Exception(mc.Arguments[0].ToString() + " can not be null");
                        }
                        this.FieldsMapping.Add(new FieldMapping()
                        {
                            FieldName = fieldNameVal.ToString(),
                            Member = nx.Members[nx.Arguments.IndexOf(x)]
                        });
                    }
                }
            }
        }

        public EntityMakeTable<T> MakeTempTable(string schema, string tableName)
        {
            var ret = new EntityMakeTable<T>(this.Clone<T>(), schema, tableName);
            ret.IsTempTable = true;
            return ret;
        }
        public EntityInsert<T1, T> InsertOn<T1>(Expression<Func<T, T1>> TableSelector)
        {

            var mb = TableSelector.Body as MemberExpression;
            var mapItem = this.PropertiesMapping.FirstOrDefault(p => p.Property == ((PropertyInfo)mb.Member));
            var fx = utils.FindAliasByExpression(this.mapAlias, mb);
            var ret = new EntityInsert<T1, T>(this);
            ret.tableInsert = mapItem.TableName ?? mapItem.AliasName;
            ret.schemaName = mapItem.Schema;
            ret.aliasForInsert = (mapItem.OriginAliasName ?? mapItem.AliasName) ?? mapItem.TableName;
            ret.schemaForInsert = mapItem.Schema;
            return ret;
        }

        public EntityInsert<T1, T> InsertInto<T1>(string SchemaName)
        {
            var ret = this.InsertInto<T1>();
            ret.schemaName = SchemaName;
            return ret;
        }

        /// <summary>
        /// Insert data item
        /// </summary>
        /// <param name="DataItem"></param>
        /// <returns></returns>
        /// <exception cref="LV.Db.Common.DataActionError">DataActionError</exception>
        //[System.Diagnostics.DebuggerStepThrough]
        public T InsertItem(T DataItem)
        {
            var Cmp = this.compiler;
            var fields = new List<string>();
            var fieldsValues = new List<string>();
            var valueParams = new List<ParamConst>();
            var index = 0;
            foreach (PropertyInfo P in typeof(T).GetProperties().ToList())
            {
                var fieldMapping = this.FieldsMapping.FirstOrDefault(p => p.Member == P);
                fields.Add((fieldMapping == null) ? P.Name : fieldMapping.FieldName);
                fieldsValues.Add("{" + index + "}");
#if NET_CORE
                valueParams.Add(new ParamConst {
                    Type=P.PropertyType,
                    Value= P.GetValue(DataItem)
                });
#else
                valueParams.Add(new ParamConst
                {
                    Type = P.PropertyType,
                    Value = P.GetValue(DataItem, new object[] { })
                });
#endif
                index++;
            }
            var sql = "Insert into " + Cmp.GetQName(this.Schema, this.table_name) +
                "(" + string.Join(",", fields.Select(p => Cmp.GetQName("", p)).ToArray()) + ")" +
                " values(" + string.Join(",", fieldsValues.ToArray()) + "); ";
            var tbl = utils.GetTableInfo<T>(this.compiler, this.Schema, this.table_name);
            var strGetIndent = Cmp.CreateSQLGetIdentity(this.Schema, this.table_name, tbl);
            sql += " " + strGetIndent;
            var retData = this.dbContext.ExcuteCommand(sql, valueParams.Cast<ParamConst>().ToArray());
            if (retData.NewID != null)
            {
                foreach (DictionaryEntry item in retData.NewID)
                {
                    var P = typeof(T).GetProperty(item.Key.ToString());
                    if (P != null)
                    {
#if NET_CORE
                        P.SetValue(DataItem,item.Value);
#else
                        P.SetValue(DataItem, item.Value, new object[] { });
#endif
                    }
                }
            }
            return DataItem;
        }
        /// <summary>
        /// Insert data by with specific fields in Expr
        /// <para>
        /// Exception see LV.Db.Common.Exceptions.Error <see cref="Exceptions.Error"/>
        /// </para>
        /// </summary>
        /// <exception cref="Exceptions.Error"></exception>
        /// <param name="Expr"></param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough]
        public T InsertItem(Expression<Func<object, T>> Expr)
        {
            var Cmp = this.compiler;
            var fields = new List<string>();
            var fieldsValues = new List<string>();
            var valueParams = new List<ParamConst>();
            var index = 0;
            var mbx = Expr.Body as MemberInitExpression;
            object data = utils.Resolve(mbx).Value;
            var insertCols = new List<ColumInfo>();
            foreach (var mb in mbx.Bindings)
            {

                fields.Add(mb.Member.Name);
                fieldsValues.Add("{" + index + "}");
                var pValue = new ParamConst
                {

                };
                valueParams.Add(pValue);
#if NET_CORE
                //valueParams.Add(((PropertyInfo) mb.Member).GetValue(data));
                pValue.Value=((PropertyInfo) mb.Member).GetValue(data);
                pValue.Type=((PropertyInfo) mb.Member).PropertyType;
#else

                pValue.Value = ((PropertyInfo)mb.Member).GetValue(data, new object[] { });
                pValue.Type = ((PropertyInfo)mb.Member).PropertyType;
#endif
                
                insertCols.Add(new ColumInfo
                {
                    DataType=pValue.Type,
                    Value=pValue.Value,
                    ColumnSize=(pValue.Type==typeof(string)&&(pValue.Value!=null))?pValue.Value.ToString().Length:-1,
                    Name=mb.Member.Name
                });
                index++;
            }
            /*
             * Find not nullable property type
             * **/
            var tbl = utils.GetTableInfo<T>(this.compiler, this.Schema, this.table_name);
            var missCols = (from col in tbl.Columns.Where(p => (!p.AllowDBNull)&&
                                                                (!p.IsAutoIncrement) && 
                                                                (!p.IsExpression) &&
                                                                (!p.IsReadOnly))
                           from iCol in insertCols.Where(p =>p.Name == col.Name).DefaultIfEmpty()
                           select new
                           {
                               Name= col.Name,
                               DataType=col.DataType,
                               col.TableName,
                               Value=(iCol!=null)?iCol.Value:null
                           }).Where(p=>p.Value==null).ToList();
                
               
            if (missCols.Count > 0)
            {
                var msg = string.Format("The column(s) {0} are missing", string.Join(",", missCols.Select(p => p.Name)));
                throw new Exceptions.Error(msg)
                {
                    ErrorType = Exceptions.ErrorType.MissFields,
                    Fields = missCols.Select(p => p.Name).ToArray(),
                    Detail= new Exceptions.ErrrorDetail
                    {
                        Fields=missCols.Select(p=> new Exceptions.ErrorFieldDetail {
                            Name=p.Name,
                            DataType=p.DataType,
                            TableName=p.TableName
                        }).ToArray()
                    }
                };
            }
            var exeecLenCols = (from c in insertCols
                               from c1 in tbl.Columns.Where(fx => 
                               fx.Name.ToLower()==c.Name.ToLower() && 
                               fx.ColumnSize<c.ColumnSize &&
                               fx.ColumnSize>-1)
                               select new {
                                   InputSize=c.ColumnSize,
                                   RequireSize=c1.ColumnSize,
                                   Name=c1.Name,
                                   DataType=c1.DataType,
                                   InputValue=c.Value,
                                   c1.TableName
                               }).ToList();
            if (exeecLenCols.Count > 0)
            {
                var msg = string.Format("The size of column(s) {0} is exceed", string.Join(",", exeecLenCols.Select(p => p.Name)));
                throw new Exceptions.Error(msg)
                {
                    ErrorType=Exceptions.ErrorType.ValueIsExceed,
                    Fields= exeecLenCols.Select(p=>p.Name).ToArray(),
                    Detail = new Exceptions.ErrrorDetail
                    {
                        Fields = exeecLenCols.Select(p => new Exceptions.ErrorFieldDetail
                        {
                            Name = p.Name,
                            DataType = p.DataType,
                            Value=p.InputValue,
                            Len=p.InputSize,
                            ExpectedLen=p.RequireSize,
                            TableName = p.TableName
                        }).ToArray()
                    }
                };
            }
            var sql = "Insert into " + Cmp.GetQName(this.Schema, this.table_name) +
                "(" + string.Join(",", fields.Select(p => Cmp.GetQName("", p)).ToArray()) + ")" +
                " values(" + string.Join(",", fieldsValues.ToArray()) + "); ";
           
            
            var strGetIndent = Cmp.CreateSQLGetIdentity(this.Schema, this.table_name, tbl);
            sql += " " + strGetIndent;
            try
            {
                var retData = this.dbContext.ExcuteCommand(sql, valueParams.Cast<ParamConst>().ToArray());
                if (retData.NewID != null)
                {
                    foreach (DictionaryEntry item in retData.NewID)
                    {
                        var P = typeof(T).GetProperty(item.Key.ToString());
                        if (P != null)
                        {
#if NET_CORE
                        P.SetValue(data,Convert.ChangeType(item.Value,P.DeclaringType));
#else
                            P.SetValue(data, Convert.ChangeType(item.Value, P.PropertyType), new object[] { });
#endif
                        }
                    }
                }
                return (T)data;
            }
            catch(DataActionError ex)
            {
                throw new Exceptions.Error(ex.Message)
                {
                    ErrorType = Exceptions.ErrorType.DuplicatetData,
                    Fields = ex.Detail.Fields,
                    
                };
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public EntityInsert<T1, T> InsertInto<T1>()
        {

            var ret = new EntityInsert<T1, T>(this);

#if NET_CORE
            
            var TableAttr = typeof(T1).CustomAttributes.Where(p => p.AttributeType == typeof(QueryDataTableAttribute))
                .FirstOrDefault();
#else

            var TableAttr = typeof(T1).GetCustomAttributes(false).Where(p => p is QueryDataTableAttribute)
                .FirstOrDefault();
#endif


            if (TableAttr is null)
            {
                throw (new Exception(string.Format("It looks like you forgot set DataTable for class {0}", typeof(T).FullName)));
            }
#if NET_CORE
            ret.tableInsert = TableAttr.ConstructorArguments[0].Value.ToString();
#else
            ret.tableInsert = ((QueryDataTableAttribute)TableAttr).TableName;
#endif

            ret.querySet.mapAlias.AddRange(this.mapAlias);

            return ret;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public EntityUpdate<T1, T> UpdateOn<T1>(Expression<Func<T, T1>> TableSelector)
        {
            if (this.isReadOnly)
            {
                throw new Exception(@"It looks like you try to update on readonly query  " + Environment.NewLine + this.ToString());
            }
            if (this.joinTimes == 0)
            {
                throw new Exception("It looks like you have tried to select table in a QuerySet with a single table");
            }
            if (!(TableSelector.Body is MemberExpression))
            {
                throw new Exception("It looks like you put wrong param. The input param is some thing looks like 'p=>p.emp'");
            }
            if (utils.CheckIfPrimitiveType(typeof(T1)))
            {
                throw new Exception(string.Format("It looks like {0} is not an Object Type", typeof(T1).FullName));
            }
            var mb = TableSelector.Body as MemberExpression;
            var mapItem = this.PropertiesMapping.FirstOrDefault(p => p.Property == mb.Member);
            var ret = new EntityUpdate<T1, T>(this);
            if (mapItem != null)
            {
                ret.querySet.table_name = mapItem.OriginAliasName;
            }
            else
            {
                throw new Exception(@"Can not find table for updating  " + Environment.NewLine + this.ToString());

            }
            return ret;
        }

        public QuerySet<T> SetTableName(string tableName)
        {
            this.table_name = tableName;
            return this;
        }

        public QuerySet<T> SetSchema(string schema)
        {
            this.Schema = schema;
            return this;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public DeleteEntity<T> DeleteOn<T1>(Expression<Func<T, T1>> TableSelector)
        {
            if (this.joinTimes == 0)
            {
                throw new Exception("It looks like you have tried to select table in a QuerySet with a single table");
            }
            if (!(TableSelector.Body is MemberExpression))
            {
                throw new Exception("It looks like you put wrong param. The input param is some thing looks like 'p=>p.emp'");
            }
            if (utils.CheckIfPrimitiveType(typeof(T1)))
            {
                throw new Exception(string.Format("It looks like {0} is not an Object Type", typeof(T1).FullName));
            }
            var ret = new DeleteEntity<T>(this);
            var mb = TableSelector.Body as MemberExpression;
            var mapItem = this.PropertiesMapping.FirstOrDefault(p => p.Property == mb.Member);
            if (mapItem != null)
            {
                ret.querySet.table_name = mapItem.OriginAliasName;
            }
            else
            {
                throw new Exception("Can not find table for deleteion at sql " + Environment.NewLine + this.ToString());
            }


            return ret;
        }
        public EntityUpdate<T, T> Update(Expression<Func<object, T>> Expr)
        {
            var Cmp = this.compiler;
            var ret = new EntityUpdate<T, T>(this);
            ret.querySet.mapAlias.Add(new MapAlais()
            {
                Schema = ret.querySet.Schema,
                TableName = ret.querySet.table_name,
                ParamExpr = Expr.Parameters[0]
            });
            ret.fields = new List<DMLFieldInfo>();
            var nx = Expr.Body as MemberInitExpression;
            var Params = ret.querySet.Params;

            foreach (MemberAssignment x in nx.Bindings)
            {
                var fieldSource = Compiler.GetField(this.compiler, x.Expression, ret.querySet.FieldsMapping, ret.querySet.mapAlias, ref Params);
                ret.fields.Add(new DMLFieldInfo()
                {
                    ToField = Cmp.GetQName("", x.Member.Name),
                    Source = fieldSource
                });
            }

            return ret;
        }
        public EntityInsert<T, T> Insert(Expression<Func<T, T>> Expr)
        {
            var Cmp = this.compiler;
            var ret = new EntityInsert<T, T>(this);
            ret.querySet.mapAlias.Add(new MapAlais()
            {
                Schema = ret.querySet.Schema,
                TableName = ret.querySet.table_name,
                ParamExpr = Expr.Parameters[0]
            });
            ret.fields = new List<DMLFieldInfo>();
            var nx = Expr.Body as MemberInitExpression;
            var Params = ret.querySet.Params;

            foreach (MemberAssignment x in nx.Bindings)
            {
                if (x.Expression is MethodCallExpression)
                {
                    var mc = x.Expression as MethodCallExpression;
                    if (mc.Method.Name == "FirstOrDefault")
                    {
                        var qrx = mc.Arguments[0];
                        var fx = utils.Resolve(qrx).Value as BaseQuerySet;
                        fx.selectTop = 1;
                        var sql = "(" + utils.RepairParameters(fx.ToSQLString(), ret.querySet.Params, fx.Params) + ")";

                        ret.fields.Add(new DMLFieldInfo()
                        {
                            ToField = Cmp.GetQName("", x.Member.Name),
                            Source = sql
                        });
                        ret.querySet.Params.AddRange(fx.Params);

                    }
                }
                else
                {
                    var fieldSource = Compiler.GetField(this.compiler, x.Expression, ret.querySet.FieldsMapping, ret.querySet.mapAlias, ref Params);
                    ret.fields.Add(new DMLFieldInfo()
                    {
                        ToField = Cmp.GetQName("", x.Member.Name),
                        Source = fieldSource
                    });
                }
            }

            return ret;
        }


        public EntityUpdate<T, T> Set(Expression<Func<T, T>> expr)
        {
            throw new NotImplementedException();
        }
        public EntityUpdate<T, T> Set(Expression<Func<T, object>> field, Expression<Func<T, object>> source)
        {
            var ret = new EntityUpdate<T, T>(this);
            ret.Set(field, source);
            return ret;
        }
        public EntityUpdate<T, T> Set(Expression<Func<T, object>> field, object value)
        {
            var ret = new EntityUpdate<T, T>(this);
            ret.Set(field, value);
            return ret;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public int Count(DBContext db)
        {
            var qr = this.Clone<T>();
            var retQr = qr.Select(p => new { total = SqlFuncs.Count(p) });
            var ret = retQr.ToList(db);
            return ret[0].total;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public int Count(DBContext db, Expression<Func<T, bool>> Filter)
        {
            var qr = this.Clone<T>();
            var qrCount = qr.Where(Filter).Select(p => SqlFuncs.Count(p));
            return qrCount.FirstOrDefault();
        }
        [System.Diagnostics.DebuggerStepThrough]
        public int Count()
        {
            return this.Count(this.dbContext);
        }
        public int Count(Expression<Func<T, bool>> Filter)
        {
            return this.Count(this.dbContext, Filter);
        }


        /// <summary>
        /// List of selected fields. Each item in this list directly can be use in database
        /// Example:
        /// For PostgreSQL:
        ///             --------------------------------------------------------------------------------
        ///             |             Value                                                             |
        ///             --------------------------------------------------------------------------------
        ///             |"employees"."firstname"                                                        |
        ///             ---------------------------------------------------------------------------------
        ///             |"employees"."firstname"+' '+"employees"."lastname" as "fullname"               |
        ///             --------------------------------------------------------------------------------
        /// For MS SQL server:
        ///             --------------------------------------------------------------------------------
        ///             |             Value                                                             |
        ///             --------------------------------------------------------------------------------
        ///             |[employees].[firstname]                                                        |
        ///             ---------------------------------------------------------------------------------
        ///             |[employees].[firstname]+' '+[employees].[lastname] as [fullname]               |
        ///             --------------------------------------------------------------------------------             
        ///             
        /// For MySQl:
        ///             --------------------------------------------------------------------------------
        ///             |             Value                                                             |
        ///             --------------------------------------------------------------------------------
        ///             |`employees`.`firstname`                                                        |
        ///             ---------------------------------------------------------------------------------
        ///             |`employees`.`firstname`+' '+`employees`.`lastname` as `fullname`               |
        ///             --------------------------------------------------------------------------------      
        /// </summary>








        // internal List<MapType> mapTypes=new List<MapType>();

        /// <summary>
        /// after compiler pasre QuerySet.Filter the expression will be stored here
        /// </summary>



        /// <summary>
        /// make new instance QuerySet with table name
        /// </summary>
        /// <param name="table_name"></param>
        internal QuerySet(ICompiler Provider, string table_name)
        {
            this.compiler = Provider;
            this.table_name = table_name;
            this.fields = new List<QueryFieldInfo>();
            this.Params = new List<object>();
            this.elemetType = typeof(T);

        }
        internal QuerySet(ICompiler Provider, string Schema, string FunctionName, params object[] Params)
        {
            this.compiler = Provider;
            this.Params = new List<object>();
            this.Params.AddRange(Params);
            var paramNames = new List<string>();

            foreach (var p in Params)
            {
                paramNames.Add("{" + paramNames.Count + "}");
            }
            this.table_name = FunctionName;
            this.function_Schema = Schema;
            this.function_call = FunctionName + "(" + string.Join(",", paramNames.ToArray()) + ")";

            this.fields = new List<QueryFieldInfo>();

            this.elemetType = typeof(T);

        }
        internal QuerySet(bool IgnoreCheckTable)
        {
            this.fields = new List<QueryFieldInfo>();
            this.Params = new List<object>();
            this.elemetType = typeof(T);
        }
        //[System.Diagnostics.DebuggerStepThrough]
        internal QuerySet(ICompiler Provider)
        {
            this.compiler = Provider;
            this.elemetType = typeof(T);
            if (!utils.CheckIfAnonymousType(typeof(T)))
            {
#if NET_CORE
                var attr = typeof(T).GetCustomAttributes().Where(p => p is QueryDataTableAttribute).Cast<QueryDataTableAttribute>().FirstOrDefault();
#else
                var attr = typeof(T).GetCustomAttributes(false).Where(p => p is QueryDataTableAttribute).Cast<QueryDataTableAttribute>().FirstOrDefault();
#endif
                if (attr == null)
                {
                    throw new Exception(string.Format("It looks like you forgot set 'QueryDataTable' for {0}", typeof(T)));
                }
                this.table_name = attr.TableName;
                this.Schema = attr.Schema;
            }

            this.fields = new List<QueryFieldInfo>();
            this.Params = new List<object>();
        }

        internal QuerySet(ICompiler Provider, string schema, string tableName)
        {
            this.elemetType = typeof(T);
            this.table_name = tableName;
            this.fields = new List<QueryFieldInfo>();
            this.Params = new List<object>();
            this.Schema = schema;
            this.compiler = Provider;
            
            //this.SyncTable<T>(ret.table_name);
            //this.mapTypes = new List<MapType>();

        }

        /// <summary>
        /// Set datasource, the data source maybe a sub query
        /// </summary>
        /// <param name="sql"></param>
        internal void SetDataSource(string sql)
        {
            this.unionSQLs.Clear();
            this.where = null;
            this.fields.Clear();
            this.groupbyList.Clear();
            this.having = null;
            this.datasource = sql;
        }

        public QuerySet<QrJoin<T, T1>> Join<T1>(QuerySet<T1> qr, Expression<Func<T, T1, bool>> JoinExpr)
        {
            return this.Join(qr, JoinExpr, (p, q) => new QrJoin<T, T1>()
            {
                Left = p,
                Right = q
            });
        }
        //[System.Diagnostics.DebuggerStepThrough]
        internal string MakeLogical<T2>(QuerySet<T2> ret, Expression<Func<T, bool>> expr, bool ForFilter,ref bool IsWillRunAsSubQuery)
        {
            if((ret.fields.Count(p=>p.Field!="*" && p.Field.IndexOf(".*") == -1) > 0)
                )
            {
                IsWillRunAsSubQuery = true;
                var mpxs = utils.GetAllMemberExpression(expr);
                var mapForCompiler = new List<MapAlais>();
                var mFields = new List<FieldMapping>();
                
                foreach(var x in mpxs)
                {
                    mapForCompiler.Add(new MapAlais
                    {
                        Member = x.Member,
                        ParamExpr = x.Expression as ParameterExpression,
                        TableName = "sql"
                    });

                }
                var pLstParams = ret.Params;
                var strWhere=Compiler.GetLogicalExpression(this.compiler, expr.Body, mFields, mapForCompiler, ref pLstParams);
                ret.SetDataSource("(" + ret.ToSQLString() + ") " + ret.compiler.GetQName("sql"));
                ret.fields.Clear();

                ret.mapAlias = ret.elemetType.GetProperties().Select(p => new MapAlais
                {
                    TableName="sql",
                    Member=p,
                    ParamExpr=Expression.Parameter(ret.elemetType)
                }).ToList();
                ret.table_name = "sql";
                return strWhere;
            }
            var Cmp = ret.compiler;
            var lstParams = ret.Params;
            //var mps = utils.GetAllMemberExpression(expr);
            //mps.ForEach(p =>
            //{
            //    var m = utils.FindAliasByMemberExpression(ret.mapAlias, p);
            //    //ret.mapAlias.Add(new MapAlais
            //    //{
            //    //    AliasName=ret.,
            //    //    Member=p.Member,

            //    //});
            //});
            var isSingleQuery = (ret.fields.Count(p => p.Field == "*" || p.Field.IndexOf(".*") > -1) == ret.fields.Count);
            if (ForFilter)
            {
                if ((this.IsSubQueryOnNextPhase() || this.hasExpressionInSelector)&&(!isSingleQuery))
                {
                    ret.subQueryCount++; ;
                    ret.datasource = "(" + this.ToSQLString() + ") as " + Cmp.GetQName("", "T" + ret.subQueryCount);
                    ret.unionSQLs.Clear();
                    ret.fields.Clear();
                    ret.groupbyList.Clear();
                    var tmp = new List<MapAlais>();
                    foreach (var px in expr.Parameters)
                    {
                        tmp.Add(new MapAlais()
                        {
                            ParamExpr = px,
                            AliasName = "T" + ret.subQueryCount
                        });
                    }
                    ret.table_name = "T" + ret.subQueryCount;
                    var strFilter1 = Compiler.GetLogicalExpression(this.compiler, expr.Body, ret.FieldsMapping, tmp, ref lstParams);
                    ret.mapAlias = tmp;
                    ret.Params = lstParams;
                    return strFilter1;
                }
            }
            else
            {
                if ((!string.IsNullOrEmpty(this.where))
                || this.selectTop > 0
                || this.skipNumber > 0
                || this.isSubQuery
                || (this.unionSQLs != null) && (this.unionSQLs.Count > 0))
                {

                }
            }

            var tmpMaps = new List<MapAlais>();
            tmpMaps.Add(new MapAlais()
            {
                Schema = this.Schema,
                TableName = this.table_name,
                Children = ret.mapAlias.Count > 0 ? ret.mapAlias : null,
                ParamExpr = expr.Parameters[0]

            });
            var strFilter = Compiler.GetLogicalExpression(this.compiler, expr.Body, ret.FieldsMapping, tmpMaps, ref lstParams);

            ret.Params = lstParams;
            return strFilter;
        }

        /// <summary>
        /// Where clause
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public QuerySet<T> Filter(Expression<Func<T, bool>> expr)
        {
            var ret = this.Clone<T>();
            var isWillRunAsSybQuery = false;
            ret.where = MakeLogical<T>(ret, expr, true,ref isWillRunAsSybQuery);
            return ret;
        }



        /// <summary>
        /// Get list of params
        /// </summary>
        /// <returns></returns>
        public List<object> GetParams()
        {
            return this.Params;
        }

        /// <summary>
        /// Select clause
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="selector">Example:p=>new {p.Code,p.Name,FullName=p.Code+" "+p.FirstName}</param>
        /// <returns></returns>
       //[System.Diagnostics.DebuggerStepThrough]
        public QuerySet<T2> Select<T2>(Expression<Func<T, T2>> selector)
        {
            var Cmp = this.compiler;
            var ret = new QuerySet<T2>(true);
            var moreAliasMapping = new List<MapAlais>();
            if (selector.Body is MemberExpression)
            {

                ret = this.Clone<T2>();
                ret.fields.Clear();
                var Params = ret.Params;
                var mb = selector.Body as MemberExpression;
                if (utils.CheckIfPrimitiveType(((PropertyInfo)mb.Member).PropertyType))
                {
                    if (ret.mapAlias.Count == 0)
                    {
                        var fFielName = mb.Member.Name;
                        var fmap = this.FieldsMapping.FirstOrDefault(p => p.Member == mb.Member);
                        if (fmap != null)
                        {
                            fFielName = fmap.FieldName;
                        }
                        ret.fields.Add(new QueryFieldInfo()
                        {
                            Field = Cmp.GetFieldName(ret.Schema, ret.table_name, fFielName)

                        });
                    }
                    else
                    {
                        var map = utils.FindAliasByMemberExpression(ret.mapAlias, mb);
                        var fFielName = mb.Member.Name;
                        var fmap = this.FieldsMapping.FirstOrDefault(p => p.Member == mb.Member);
                        if (fmap != null)
                        {
                            fFielName = fmap.FieldName;
                        }
                        ret.fields.Add(new QueryFieldInfo()
                        {
                            Field = Cmp.GetFieldName((map != null) ? map.Schema : this.Schema, (map != null) ? (map.AliasName ?? map.TableName) : this.table_name, fFielName)

                        });
                    }
                }
                else
                {
                    var mp = utils.FindAliasByMemberExpression(ret.mapAlias, mb);

                    if (mp == null)
                    {
                        throw new Exception("The expression in selector does not point to a table");
                    }
                    ret.fields.Add(new QueryFieldInfo()
                    {
                        Field = Cmp.GetQName(ret.Schema, mp.AliasName ?? mp.TableName) + ".*"

                    });
                }
                return ret;
            }
            if (selector.Body is ParameterExpression)
            {
                var px = selector.Body as ParameterExpression;
            }
            if (selector.Body is NewExpression)
            {
                var nx = selector.Body as NewExpression;
                foreach (var a in nx.Arguments)
                {
                    if (a is MemberExpression)
                    {
                        var mb = a as MemberExpression;
                        var x = ret.PropertiesMapping.FirstOrDefault(p => p.Property == mb.Member);
                        var y = ret.PropertiesMapping.FirstOrDefault(p => p.Property == nx.Members[nx.Arguments.IndexOf(a)]);
                        if (y != null && x != null)
                        {
                            if (x.TableName != null)
                            {
                                y.TableName = x.TableName;
                                y.Schema = x.Schema;
                            }
                        }
                        if (y != null && x == null)
                        {
                            ret.PropertiesMapping.Add(new PropertyMapping()
                            {
                                AliasName = y.AliasName,
                                OriginAliasName = y.OriginAliasName,
                                Property = mb.Member as PropertyInfo,
                                Schema = y.Schema,
                                TableName = y.TableName
                            });


                        }
                        if (y == null && x != null)
                        {
                            ret.PropertiesMapping.Add(new PropertyMapping()
                            {
                                AliasName = x.AliasName,
                                OriginAliasName = x.OriginAliasName,
                                Property = nx.Members[nx.Arguments.IndexOf(a)] as PropertyInfo,
                                Schema = x.Schema,
                                TableName = x.TableName
                            });
                        }
                    }
                    else if (a is ParameterExpression)
                    {
                        var tblMapping = utils.FindAliasByExpression(this.mapAlias, a);
                        if (tblMapping == null)
                        {
                            throw new Exception(string.Format("{0} in {1} is not map to table. Please, select propery in which mapped to data table", a, selector.ToString()));
                        }

                    }
                    else if (a is MethodCallExpression)
                    {
                        var Members = utils.GetAllMemberExpression(a);
                        foreach (var mb in Members)
                        {
                            var m = utils.FindAliasByMemberExpression(this.mapAlias, mb);
                            ret.mapAlias.Add(new MapAlais
                            {
                                AliasName = (m != null) ? m.AliasName : this.table_name,
                                Member = (m != null) ? m.Member : mb.Member,
                                ParamExpr = (m != null) ? m.ParamExpr : (mb.Expression as ParameterExpression),
                                Schema = this.Schema,
                                TableName = this.table_name
                            });
                        }
                    }
                    else if (a is BinaryExpression)
                    {
                        var Exprs = utils.GetAllParamsExpr(a);
                    }

                }
            }
            if (selector.Body is MemberInitExpression)
            {
                var nx = selector.Body as MemberInitExpression;
                foreach (MemberAssignment a in nx.Bindings)
                {
                    if (a.Expression is MemberExpression)
                    {
                        var mb = a.Expression as MemberExpression;
                        var x = this.PropertiesMapping.FirstOrDefault(p => p.Property.Name == mb.Member.Name);
                        if (x == null)
                        {
                            var mxs = utils.GetAllMemberExpression(mb);
                            if (mxs != null && mxs.Count > 0)
                            {
                                var aliasName = utils.FindAliasByMember(this.mapAlias, mxs[mxs.Count - 1].Member);
                                if (aliasName != null)
                                {
                                    x = new PropertyMapping()
                                    {
                                        AliasName = aliasName.AliasName,
                                        OriginAliasName = aliasName.AliasName,
                                        Property = mb.Member as PropertyInfo,
                                        Schema = this.Schema,
                                        TableName = this.table_name
                                    };
                                    moreAliasMapping.Add(new MapAlais()
                                    {
                                        AliasName = aliasName.AliasName,
                                        Member = mb.Member


                                    });
                                    //var px = utils.FindParameterExpression(mxs[mxs.Count - 1]);
                                    //if (px != null)
                                    //{
                                    //    if (moreAliasMapping.Count(p => p.ParamExpr == px) == 0)
                                    //    {
                                    //        moreAliasMapping.Add(new MapAlais()
                                    //        {
                                    //            AliasName=aliasName.AliasName,
                                    //            ParamExpr=px
                                    //        });
                                    //    }
                                    //}
                                }
                            }

                        }
                        var y = this.PropertiesMapping.FirstOrDefault(p => p.Property.Name == a.Member.Name);
                        if (y == null)
                        {
                            var mxs = utils.GetAllMemberExpression(mb);
                            if (mxs != null && mxs.Count > 0)
                            {
                                var aliasName = utils.FindAliasByMember(this.mapAlias, mxs[mxs.Count - 1].Member);
                                if (aliasName != null)
                                {
                                    y = new PropertyMapping()
                                    {
                                        AliasName = aliasName.AliasName,
                                        OriginAliasName = aliasName.AliasName,
                                        Property = mb.Member as PropertyInfo,
                                        Schema = this.Schema,
                                        TableName = this.table_name
                                    };
                                    moreAliasMapping.Add(new MapAlais()
                                    {
                                        AliasName = aliasName.AliasName,
                                        Member = mb.Member


                                    });
                                }
                            }

                        }
                        if (y != null && x != null)
                        {
                            if (x.TableName != null)
                            {
                                y.TableName = x.TableName;
                                y.Schema = x.Schema;
                            }
                        }
                        if (x != null && y == null)
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
                        if (x == null && y != null)
                        {
                            ret.PropertiesMapping.Add(new PropertyMapping
                            {
                                AliasName = y.AliasName,
                                OriginAliasName = y.OriginAliasName,
                                Property = a.Member as PropertyInfo,
                                Schema = y.Schema,
                                TableName = y.TableName
                            });
                        }
                    }
                    else if (a is ParameterExpression)
                    {
                        ret.hasExpressionInSelector = true;
                    }

                }
            }
            //((MemberExpression)((NewExpression)selector.Body).Arguments[0]).Member==((NewExpression)selector.Body).Members[0]
            this.SelectByExpression(ret, selector);
            ret.mapAlias.AddRange(moreAliasMapping);
            return ret;

        }

        //[System.Diagnostics.DebuggerStepThrough]


        /// <summary>
        /// Create new instance from this instance
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        internal QuerySet<T2> Clone<T2>()
        {
            var ret = new QuerySet<T2>(this.compiler, this.GetTableName());
            base.CopyTo(ret, typeof(T2));
            return ret;
        }
        /// <summary>
        /// Create real SQL statement with input parameters
        /// </summary>
        /// <returns></returns>
        public override string ToSQLString()
        {
            var Cmp = this.compiler;
            if (this.unionSQLs.Count > 0)
            {
                var ret = string.Join(" union  ", this.unionSQLs.Select(p => (p.IsUnionAll ? " all " : " ") + p.Sql));
                if (!string.IsNullOrEmpty(this.where))
                {
                    
                }
                return ret;
            }
            if (this.datasource == this.table_name)
            {
                var _select = Cmp.GetQName(this.Schema, this.table_name) + ".*";

                var tableName = Cmp.GetQName(this.Schema, this.table_name);
                if (this.fields.Count > 0)
                {
                    _select = string.Join(",", this.fields.Select(p => p.Field + " as " + Cmp.GetQName("", p.Alias)).ToArray());
                }
                if (this.isSelectDistinct)
                {
                    _select = " Distinct " + _select;
                }
                if (this.isFromNothing)
                {
                    return "select " + _select;
                }
                var sql = "select " + _select + " from " + tableName;
                if (!string.IsNullOrEmpty(this.where))
                {
                    sql = sql + " where " + this.where;
                }

                if (this.groupbyList != null && this.groupbyList.Count > 0)
                {
                    sql = sql + " group by " + string.Join(",", this.groupbyList.Select(p => p.Field).ToArray());
                }
                if (!string.IsNullOrEmpty(this.having))
                {
                    sql = sql + " having " + this.having;
                }

                if (this.sortList.Count > 0)
                {
                    var sorts = new List<string>();
                    foreach (var f in this.sortList)
                    {
                        sorts.Add(f.FieldName + " " + f.SortType);
                    }
                    sql = sql + " order by " + string.Join(",", sorts.ToArray());
                }
                return sql;
            }
            else
            {
                var _select = "*";
                if (!string.IsNullOrEmpty(this.table_name))
                {
                    if (string.IsNullOrEmpty(this.function_call))
                    {
                        _select = Cmp.GetQName(this.Schema, this.table_name) + ".*";
                    }
                    else
                    {
                        _select = Cmp.GetQName("", this.table_name) + ".*";
                    }
                }

                var tableName = Cmp.GetQName(this.function_Schema ?? this.Schema, this.function_call ?? this.table_name);
                if (!string.IsNullOrEmpty(this.function_call))
                {
                    tableName += " as " + Cmp.GetQName("", this.table_name);
                }
                if (this.fields.Count > 0)
                {
                    var _selectAll= this.compiler.GetQName(this.Schema, this.table_name) + ".*";
                    if (this.fields.Count > 1 && this.fields.Count(p => p.Field.Length >= 2 && p.Field.Substring(p.Field.Length - 2, 2) == ".*") == this.fields.Count())
                    {
                        _select = "*";
                    }
                     
                    else
                    {
                        _select = string.Join(",", this.fields.Select(p => p.Field +
                        (string.IsNullOrEmpty(p.Alias) ? "" :
                        " as " + Cmp.GetQName("", p.Alias))
                        ).ToArray());
                        if (this.fields.Count == 1 &&
                            this.fields.Count(p => p.Field.IndexOf(_selectAll) > -1) == 1)
                        {
                            _select = _select.Replace(_selectAll, "*");
                        }
                    }
                }
                var sql = "";
                if (this.isSelectDistinct)
                {
                    _select = " Distinct " + _select;
                }
                if (!isSubQuery)
                {
                    sql = "select " + _select + " from " + ((this.datasource != null) ? "" + this.datasource + "" : tableName);
                }
                else
                {
                    sql = "select " + _select + " from " + ((this.datasource != null) ? "" + this.datasource + "" : tableName);
                }
                if (!string.IsNullOrEmpty(this.where))
                {
                    sql = sql + " where " + this.where;
                }
                if (this.groupbyList != null && this.groupbyList.Count > 0)
                {
                    sql = sql + " group by " + string.Join(",", this.groupbyList.Select(p => p.Field).ToArray());
                }
                if (!string.IsNullOrEmpty(this.having))
                {
                    sql = sql + " having " + this.having;
                }

                var sorts = new List<string>();
                if (this.sortList.Count > 0)
                {
                    foreach (var f in this.sortList)
                    {
                        sorts.Add(f.FieldName + " " + f.SortType);
                    }
                    sql = sql + " order by " + string.Join(",", sorts.ToArray());
                }
                if (this.selectTop > 0 || this.skipNumber > 0)
                {
                    sql = Cmp.MakeLimitRow(sql, this.sortList, this.fields.Select(p => p.Field).ToList(), this.selectTop, this.skipNumber);
                }

                return sql;
            }
        }

        /// <summary>
        /// Make SQL statement for debugger
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sql = this.ToSQLString();
            var index = 0;
            var ParamsConts = this.Params.Cast<ParamConst>().ToList();
            foreach (var p in ParamsConts)
            {

                if (p.Value == null)
                {
                    sql = sql.Replace("{" + index.ToString() + "}", "NULL");
                }
                else if(p.Value is bool)
                {
                    sql = sql.Replace("{" + index.ToString() + "}", this.compiler.ConstVal((bool)p.Value));
                }
                else if (p.Value is string)
                {
                    sql = sql.Replace("{" + index.ToString() + "}", "'" + p.Value.ToString().Replace("'", "''") + "'");
                }
                else if (p.Value is DateTime)
                {
                    sql = sql.Replace("{" + index.ToString() + "}", "'" + ((DateTime)p.Value).ToString("yyyy-MM-dd hh:mm:ss") + "'");
                }
                else
                {
                    sql = sql.Replace("{" + index.ToString() + "}", "" + p.Value.ToString() + "");
                }
                index++;
            }
            return sql;
        }
        /// <summary>
        /// Get table name
        /// </summary>
        /// <returns></returns>
        public string GetTableName()
        {
            return this.table_name;
        }

        /// <summary>
        /// Get list of fields
        /// </summary>
        /// <returns></returns>
        public List<QueryFieldInfo> GetFields()
        {
            return this.fields;
        }

        public QuerySet<T> SortAsc(Expression<Func<T, object>> Expr)
        {
            var Body = ((LambdaExpression)Expr).Body;
            List<string> Fields = SortCompiler.GetFields(this, Body);
            foreach (var f in Fields)
            {
                this.sortList.Add(new SortingInfo()
                {
                    FieldName = f,
                    SortType = "asc"
                });
            }

            return this;
        }

        public QuerySet<T> SortDesc(Expression<Func<T, object>> Expr)
        {
            var Body = ((LambdaExpression)Expr).Body;
            List<string> Fields = SortCompiler.GetFields(this, Body);
            foreach (var f in Fields)
            {
                this.sortList.Add(new SortingInfo()
                {
                    FieldName = f,
                    SortType = "desc"
                });
            }

            return this;
        }
        public QuerySet<T> Skip(int num)
        {
            var ret = this.Clone<T>();
            ret.skipNumber = num;
            return ret;
        }
        public QuerySet<T> SortAsc(params Expression<Func<T, object>>[] Expr)
        {
            throw new NotImplementedException();
            //foreach (var x in Expr)
            //{
            //    if (x.Body is MemberExpression)
            //    {
            //        var pList = new List<object>();
            //        var mx = x.Body as MemberExpression;
            //        var Fields = 

            //            //SelectorCompiler.Gobble<string>(x.Body, new string[] { this.Schema }, new string[] { this.table_name }, x.Parameters.Cast<object>().ToList(), new MemberInfo[] { mx.Member }, 0, ref pList);

            //    }

            //    //SortCompiler.GetFields(x);
            //    //foreach (var f in Fields)
            //    //{
            //    //    this.sortList.Add(new SortingInfo()
            //    //    {
            //    //        FieldName = f,
            //    //        SortType = "asc"
            //    //    });
            //    //}
            //}
            //return this;
        }
        public QuerySet<T> Distinct()
        {
            var ret = this.Clone<T>();
            ret.isSelectDistinct = true;
            return ret;
        }
        public QuerySet<T> FirstOrDefault()
        {
            var ret = this.Clone<T>();
            ret.selectTop = 1;
            return ret;
        }
        public QuerySet<T> SortDesc(params Expression<Func<T, object>>[] Expr)
        {
            foreach (var x in Expr)
            {
                List<string> Fields = SortCompiler.GetFields(this, x);
                foreach (var f in Fields)
                {
                    this.sortList.Add(new SortingInfo()
                    {
                        FieldName = f,
                        SortType = "desc"
                    });
                }
            }
            return this;
        }

        public QuerySet<T> First()
        {
            var ret = this.Clone<T>();
            ret.selectTop = 1;
            return ret;
        }
        //[System.Diagnostics.DebuggerStepThrough]
        public QuerySet<T> GroupBy(Expression<Func<T, object>> GroupFields)
        {
            var Cmp = this.compiler;
            if (!(GroupFields.Body is NewExpression))
            {
                throw new Exception("It looks like you tried to use 'GroupBy' with non NewExpression\r\nPlease use p=>new {...group by fields herer..}");
            }
            var hasAliasOrExpr = false;
            var ret = this.Clone<T>();
            var isNextSubQuery =
                this.groupbyList.Count > 0 ||
                this.selectTop > 0 ||
                this.skipNumber > 0 ||
                this.isSubQuery ||
                (this.unionSQLs != null) &&
                (this.unionSQLs.Count > 0);
            if (isNextSubQuery || this.hasExpressionInSelector)
            {
                var tmp = new List<MapAlais>();
                ret.subQueryCount++;

                var newAlias = "T" + ret.subQueryCount;
                ret.datasource = "(" + this.ToSQLString() + ") as " + Cmp.GetQName("", newAlias);
                ret.fields.Clear();
                ret.groupbyList.Clear();
                ret.unionSQLs.Clear();
                ret.table_name = newAlias;
                ret.Schema = null;
                tmp.Add(new MapAlais()
                {
                    TableName = newAlias,
                    ParamExpr = GroupFields.Parameters[0]
                });
                var groupByFields = this.ExtractFieldsList(Cmp, ret, GroupFields, ref tmp, ref hasAliasOrExpr);


                ret.groupbyList = groupByFields;
                ret.fields = groupByFields;
                ret.where = null;
                return ret;
            }

            var newMapAlias = ret.mapAlias.ToList();
            newMapAlias = new List<MapAlais>();
            ret.Params = Params;
            ret.fields.Clear();
            var nx = GroupFields.Body as NewExpression;
            var fx = utils.FindAliasByExpression(ret.mapAlias, ((MemberExpression)nx.Arguments[0]).Expression);

            ret.groupbyList.AddRange(this.ExtractFieldsList(ret.compiler, ret, GroupFields, ref newMapAlias, ref hasAliasOrExpr));
            ret.hasExpressionInSelector = hasAliasOrExpr;
            ret.fields = ret.groupbyList;
            //if (hasAliasOrExpr)
            //{
            //    ret.mapAlias = newMapAlias;
            //}
            return ret;

        }

        public QuerySet<T> Having(Expression<Func<T, bool>> having)
        {
            var ret = this.Clone<T>();
            var lstParams = ret.Params;
            var IsWillRunAsSubQuery = false;
            ret.having = this.MakeLogical(ret, having, false,ref IsWillRunAsSubQuery);
            //SelectorCompiler.Gobble<string>(having.Body, new string[] { ret.Schema }, new string[] { ret.GetTableName() }, having.Parameters.Cast<object>().ToList(), null, -1, ref lstParams);
            ret.Params = lstParams;
            return ret;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public IList<T> ToList(DBContext db)
        {
            try
            {
                return db.ExecToList<T>(this, this.ToSQLString(), this.Params.Cast<ParamConst>().ToArray());
            }
            catch (Exception ex)
            {

                throw new Exceptions.ExcutationError(ex.Message +Environment.NewLine+"Sql:"+Environment.NewLine + this.ToString())
                {
                    Sql=this.ToString(),
                    Params=this.Params,
                    SqlWithParams=this.ToSQLString()
                };
            }
        }

        public DataTable ToDataTable(DBContext db)
        {
            return db.ExecToDataTable(this.ToSQLString(), this.Params.Cast<ParamConst>().ToArray());
        }


        public T Item(DBContext db)
        {
            if (this.selectTop == 0)
            {
                this.selectTop = 1;
            }
            var retList = this.ToList(db);
            return retList.FirstOrDefault();

        }
        public T Item()
        {
            return this.Item(this.dbContext);

        }


        public QuerySet<T> WhereOr(Expression<Func<T, bool>> expr)
        {
            var ret = this.Clone<T>();
            ret.Filter(expr);
            if (string.IsNullOrEmpty(this.where))
            {
                return ret;
            }
            else
            {
                ret.where = "(" + this.where + ") or (" + ret.where + ")";
            }
            return ret;

        }
        public QuerySet<T2> Join<T1, T2>(QuerySet<T1> qr, Expression<Func<T, T1, bool>> JoinClause, Expression<Func<T, T1, T2>> Selector)
        {
            return QueryBuilder.Join(this, qr, JoinClause, Selector);
        }

        public QuerySet<T2> LeftJoin<T1, T2>(QuerySet<T1> qr, Expression<Func<T, T1, bool>> JoinClause, Expression<Func<T, T1, T2>> Selector)
        {
            return QueryBuilder.LeftJoin(this, qr, JoinClause, Selector);
        }
        public QuerySet<T2> RightJoin<T1, T2>(QuerySet<T1> qr, Expression<Func<T, T1, bool>> JoinClause, Expression<Func<T, T1, T2>> Selector)
        {
            return QueryBuilder.RightJoin(this, qr, JoinClause, Selector);
        }
        [System.Diagnostics.DebuggerStepThrough]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            if (this.dbContext == null)
            {
                throw new Exception("Please, call ToList with dbContext.Ex:.ToList(db);");
            }
            if (this.dbContext is DBContext)
            {
                var ret = ((DBContext)this.dbContext).ExecToList<T>(this,this.ToSQLString(), this.Params.Cast<ParamConst>().ToArray());
                return ret.GetEnumerator();
            }
            var cls = Globals.dbContextAssembly.GetExportedTypes().FirstOrDefault(p => p.Name == "DataExtensions");

            var method = cls.GetMethods().Where(p => p.Name == "FromSql" &&
                                                p.GetParameters().Count() == 3 &&
                                           p.GetParameters()[2].ParameterType == typeof(object[])
            ).FirstOrDefault();
            MethodInfo genericMethod = method.MakeGenericMethod(typeof(T));
            var rawSQL = method.GetParameters()[1].ParameterType.GetConstructors().FirstOrDefault(p => p.GetParameters().Count() == 1 &&
                                                                                 p.GetParameters()[0].ParameterType == typeof(string));

            var sql = rawSQL.Invoke(new object[] { this.ToSQLString() });
            var retList = genericMethod.Invoke(null, new object[] { Globals.dbContext, sql, this.Params.ToArray() }) as IEnumerable<T>;

            return retList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.ToList<T>(this.dbContext).GetEnumerator();
        }
        [System.Diagnostics.DebuggerStepThrough]
        public QuerySet<T> SetFieldName<T1>(Expression<Func<T, T1>> Expr, string RealFieldName)
        {
            if (Expr.Body is MemberExpression)
            {
                var mb = Expr.Body as MemberExpression;
                this.FieldsMapping.Add(new FieldMapping()
                {
                    FieldName = RealFieldName,
                    Member = mb.Member
                });
                return this;
            }
            else
            {
                throw new Exception("Expr is invalid value. Example 'Expr' like that: p=>p.EmpCode ");
            }
        }



        public QuerySet<T> SortByFieldName(string FieldName, bool IsDesc = false)
        {

            var pp = this.mapAlias.FirstOrDefault(p => p.Member != null && p.Member.Name.ToLower() == FieldName.ToLower());
            if (pp == null)
            {
                var pInfo = this.elemetType.GetProperty(FieldName);
                if (pInfo == null)
                {
                    throw new Exception(string.Format("It looks like you try to access invisible field '{0}'", FieldName));
                }
                pp = new MapAlais
                {
                    Member = pInfo
                };
            }
            ParameterExpression parameter = Expression.Parameter
            (
                pp.Member.DeclaringType,
                "m"
            );
            if (this.table_name != null)
            {
                if (this.mapAlias.Count(p => p.TableName == this.table_name && p.ParamExpr == parameter) == 0)
                {
                    this.mapAlias.Add(new MapAlais()
                    {
                        TableName = this.table_name,
                        Schema = this.Schema,
                        ParamExpr = parameter
                    });
                }
            }
            MemberExpression member = Expression.MakeMemberAccess
            (
                parameter,
                pp.Member
            );
            var mb = Expression.Convert(member, typeof(object));
            var fx = Expression.Lambda<Func<T, object>>(mb, parameter);
            if (IsDesc)
            {
                return this.SortDesc(fx);
            }
            return this.SortAsc(fx);

        }

        internal QuerySet<T> SortByFieldName(object name, object asc)
        {
            throw new NotImplementedException();
        }
    }

    internal class FieldSelector
    {
        internal string FieldName { get; set; }
        internal string Alias { get; set; }
    }

    public class MapAlais
    {
        public ParameterExpression ParamExpr { get; internal set; }
        public string AliasName { get; set; }
        public List<MapAlais> Children { get; internal set; }
        public MemberInfo Member { get; internal set; }
        public string TableName { get; internal set; }
        public string Schema { get; internal set; }

    }
    public class PropertyMapping
    {
        public string AliasName { get; set; }
        public string TableName { get; internal set; }
        public string Schema { get; internal set; }
        public PropertyInfo Property { get; internal set; }
        public string OriginAliasName { get; internal set; }
    }
}