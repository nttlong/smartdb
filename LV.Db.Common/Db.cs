using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LV.Db.Common
{
    public class DBContext : IDisposable, IDbContext
    {
        public DbProvider Provider { get; private set; }



        //private IDb __db;
        internal IDb db;

        public Hashtable CacheTableInfo { get; private set; }

        public string GetServer()
        {
            return db.GetServer();
        }
        /// <summary>
        /// Create new Database if not exists
        /// </summary>
        /// <param name="DbName"></param>
        public void CreateDbIfNotExists(string DbName)
        {
            db.CreateDbIfNotExists(DbName);
        }

        public string GetPort()
        {
            return db.GetPort();
        }

        public string Value { get; private set; }
        public Assembly ProviderAssembly { get; private set; }
        public IDbTransaction Transaction { get; private set; }
        public bool IsUseTransaction { get; private set; }
        public DbIsolationLevel TrasnsactionLevel { get; private set; }

        private void InitContext(bool IgnoreBuildTables)
        {
            if (string.IsNullOrEmpty(Globals.ConnectionString))
            {
                throw new Exception(string.Format("It looks like you forgot set {0}.ConnectionString ", typeof(Globals).FullName));
            }
            if (!string.IsNullOrEmpty(Globals.ConnectionString))
            {
                InitDbContext(Globals.DbType, Globals.ConnectionString);
                this.Provider = Globals.ProviderTypes[Globals.DbType.ToString()] as DbProvider;
                this.db = this.Provider.Asm.CreateInstance(this.Provider.DbProviderType.FullName) as IDb;
                this.db.SetConnectionString(Globals.ConnectionString);

            }
            else if (Globals.ProviderType != null)
            {
                this.db = Globals.ProviderAssembly.CreateInstance(Globals.ProviderType.FullName) as IDb;
            }
            else if (Globals.ProviderTypes[Globals.DbType.ToString()] != null)
            {
                this.db = Globals.ProviderTypes[Globals.DbType.ToString()] as IDb;
            }
            else
            {
                this.db.SetConnectionString(Globals.ConnectionString);
            }
            if (!IgnoreBuildTables)
            {
                Globals.Design.BuildTables(this.db, this.GetDbName(), this.db.GetSchema());
            }
        }
        internal DBContext(bool IgnoreBuildTables)
        {
            InitContext(IgnoreBuildTables);
        }
        public DBContext()
        {
            InitContext(false);

        }

        public object GetConnection()
        {
            return this.db.GetConnection();
        }

        public void BuildTable(Type ModelType)
        {
            var attr = ModelType.GetCustomAttribute<QueryDataTableAttribute>();
            if (attr == null) return;
            this.db.SyncTable(this.db.GetConnection(), ModelType, attr.TableName);
        }

        public QrPage<T> Pager<T>(QuerySet<T> Qr)
        {
            return new QrPage<T>(Qr);
        }
        public QrPage<T> Pager<T>(IQueryable<T> Qr)
        {
            return new QrPage<T>(Qr);
        }
        public DataPage<T2> GetPage<T, T2>(DataPageInfo<T> PageInfo)
        {
            return this.Query<T>().GetPage<T2>(PageInfo);
        }
        public void InitDBContext(DbTypes DbType, string ConnectionString, bool IgnoreBuildTable)
        {
            if (Globals.ProviderTypes[DbType.ToString()] == null)
            {
                InitDbContext(DbType, ConnectionString);
            }

            var __db = ((LV.Db.Common.DbProvider)Globals.ProviderTypes[DbType.ToString()]).Asm.CreateInstance(((LV.Db.Common.DbProvider)Globals.ProviderTypes[DbType.ToString()]).DbProviderType.FullName);
            this.db = __db as IDb;

            this.db.SetConnectionString(ConnectionString);
            if (!IgnoreBuildTable)
            {
                Globals.Design.BuildTables(this.db, this.db.GetDataBase(), this.db.GetSchema());
            }

        }
        //[System.Diagnostics.DebuggerStepThrough]
        public DBContext(DbTypes DbType, string ConnectionString)
        {
            if (Globals.Mode == SysnMode.OnNewDbContext)
            {
                InitDBContext(DbType, ConnectionString, false);
            }
            else
            {
                InitDBContext(DbType, ConnectionString, true);
            }
            //this.db.SetConnectionString(ConnectionString);


        }
        internal DBContext(DbTypes DbType, string ConnectionString, bool IgnoreBuildTable)
        {
            InitDBContext(DbType, ConnectionString, IgnoreBuildTable);
            //this.db.SetConnectionString(ConnectionString);


        }
        public DbProvider CreateProvider(string ConnectionString)
        {
            if (ProviderAssembly == null)
            {
                var location = System.Reflection.Assembly.GetEntryAssembly().Location;
                var directory = System.IO.Path.GetDirectoryName(location);
                if (Globals.DbType == DbTypes.None)
                {
                    throw new Exception(string.Format("It looks like '{0}.{1}' was not set", typeof(Globals).FullName, "DbType"));
                }
                ProviderAssembly = utils.LoadFile(directory, string.Format("LV.Db.{0}.dll", Globals.DbType));
            }
#if NET_CORE
                var ret= new DbProvider()
                {
                    DbProviderType = ProviderAssembly.DefinedTypes.First(p => p.Name == "Database"),
                    Compiler= ProviderAssembly.CreateInstance(ProviderAssembly.DefinedTypes.First(p => p.Name == "Compiler").FullName) as ICompiler,
                    Asm=ProviderAssembly
                };
                ret.Compiler.SetConnectionString(ConnectionString);
            return ret;
#else
            var ret = new DbProvider()
            {
                DbProviderType = ProviderAssembly.GetExportedTypes().First(p => p.Name == "Database"),
                Compiler = ProviderAssembly.CreateInstance(ProviderAssembly.GetExportedTypes().First(p => p.Name == "Compiler").FullName) as ICompiler,
                Asm = ProviderAssembly
            };
            ret.Compiler.SetConnectionString(ConnectionString);
            return ret;
#endif
        }
        //[System.Diagnostics.DebuggerStepThrough]
        private void InitDbContext(DbTypes DbType, string ConnectionString)
        {
            if (Globals.ProviderTypes[DbType.ToString()] == null)
            {

                var location = System.Reflection.Assembly.GetEntryAssembly().Location;
                var directory = System.IO.Path.GetDirectoryName(location);
                ProviderAssembly = utils.LoadFile(directory, string.Format("LV.Db.{0}.dll", DbType));
#if NET_CORE
                Globals.ProviderTypes[DbType.ToString()] = ProviderAssembly.DefinedTypes.First(p => p.Name == "Database");
                Globals.Compiler = Globals.ProviderAssembly.CreateInstance(Globals.ProviderAssembly.DefinedTypes.First(p => p.Name == "Compiler").FullName) as ICompiler;

                Globals.ProviderTypes[DbType.ToString()] = new DbProvider()
                {
                    DbProviderType = ProviderAssembly.DefinedTypes.First(p => p.Name == "Database"),
                    Compiler= ProviderAssembly.CreateInstance(ProviderAssembly.DefinedTypes.First(p => p.Name == "Compiler").FullName) as ICompiler,
                    Asm=ProviderAssembly
                };
                ((DbProvider)Globals.ProviderTypes[DbType.ToString()]).Compiler.SetConnectionString(ConnectionString);
#else
                Globals.ProviderTypes[DbType.ToString()] = new DbProvider()
                {
                    DbProviderType = ProviderAssembly.GetExportedTypes().First(p => p.Name == "Database"),
                    Compiler = ProviderAssembly.CreateInstance(ProviderAssembly.GetExportedTypes().First(p => p.Name == "Compiler").FullName) as ICompiler,
                    Asm = ProviderAssembly
                };
                ((DbProvider)Globals.ProviderTypes[DbType.ToString()]).Compiler.SetConnectionString(ConnectionString);
#endif

            }
            this.Provider = Globals.ProviderTypes[DbType.ToString()] as DbProvider;
            this.db = this.Provider.Asm.CreateInstance(this.Provider.DbProviderType.FullName) as IDb;
            this.db.SetConnectionString(ConnectionString);
        }

        public DBContext(string ConnectionString)
        {
            if (Globals.ProviderType == null)
            {
                InitDbContext(Globals.DbType, ConnectionString);

            }

        }

        internal QuerySet<T> FromFunction<T>(ICompiler Provider, string Schema, string FunctionName, params ParamConst[] Params)
        {
            var ret = new QuerySet<T>(Provider, Schema, FunctionName, Params);
            return ret;
        }


        /// <summary>
        /// Create dynamic query with Schema, Table and Field Mapping Expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Schema"></param>
        /// <param name="TableName"></param>
        /// <param name="Expr"></param>
        /// <returns></returns>
        public QuerySet<T> CreateDynamicQuery<T>(string Schema, string TableName, Expression<Func<T>> Expr)
        {
            var ret = this.Query<T>(true);
            ret.table_name = TableName;
            ret.Schema = Schema;
            ret.MapFields(Expr);
            return ret;
        }
        /// <summary>
        /// Insert new data item into database
        /// </summary>
        /// <param name="DataItem"></param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough]
        public T InsertData<T>(T DataItem)
        {
            return this.Query<T>().InsertItem(DataItem);
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
        public T InsertData<T>(Expression<Func<object, T>> DataItem)
        {
            return this.Query<T>().InsertItem(DataItem);
        }
        /// <summary>
        /// Create query set with schema and table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="SchemaName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public QuerySet<T> Query<T>(string SchemaName, string TableName)
        {
            QuerySet<T> ret = this.Query<T>(true);
            ret.table_name = TableName;
            ret.Schema = SchemaName;
            return ret;
        }
        //[System.Diagnostics.DebuggerStepThrough]
        public T FirstOrDefault<T>(Expression<Func<T, bool>> Expr)
        {
            var ret = this.Query<T>().FirstOrDefault(Expr);
            return ret;
        }

        public QuerySet<T> Query<T>(string SchemaName, string TableName, Expression<Func<T>> Expr)
        {
            QuerySet<T> ret = this.Query<T>(true);
            ret.table_name = TableName;
            ret.Schema = SchemaName;
            ret.MapFields(Expr);
            return ret;
        }
        public QuerySet<T> Query<T>(bool inogreCheck)
        {
            var ret = QueryBuilder.FromEntity<T>(this.Provider.Compiler, inogreCheck);
            ret.dbContext = this;
            string schema = this.db.GetSchema();
            if (!string.IsNullOrEmpty(schema))
            {
                ret.Schema = schema;
            }
            return ret;
        }


        public void BeginTran(DbIsolationLevel level = DbIsolationLevel.ReadCommitted)
        {
            this.IsUseTransaction = true;
            this.TrasnsactionLevel = level;

            //if(((IDbConnection)this.GetConnection()).State!=ConnectionState.Open && ((IDbConnection)this.GetConnection()).State != ConnectionState.Connecting)
            //{
            //    ((IDbConnection)this.GetConnection()).Open();
            //}
            //this.Transaction= ((IDbConnection)this.GetConnection()).BeginTransaction(IsolationLevel.ReadCommitted);
            //this.db.SetTransaction(this.Transaction);
        }

        public void Commit()
        {
            this.db.Commit();

        }

        public void RollBack()
        {
            this.db.RollBack();

        }

        public DBContext WithSchema(string schema)
        {
            if (string.IsNullOrEmpty(schema))
            {
                return this;
            }
            schema = schema.ToLower();
            this.db.SetSchema(schema);
            return this;
        }
        public DBContext WithDatabase(string DatabaseName)
        {
            if (Globals.Mode == SysnMode.OnDemand)
            {
                if (CacheHasCheckDatabase[DatabaseName] == null)
                {
                    this.db.CreateDbIfNotExists(DatabaseName);
                    CacheHasCheckDatabase[DatabaseName] = true;
                }
            }
            this.db.SetDataBaseName(DatabaseName);
            return this;
        }
        public DBContext WithDatabase(string DatabaseName, string dbSchema)
        {
            if (string.IsNullOrEmpty(DatabaseName) && string.IsNullOrEmpty(dbSchema)) return this;
            if ((!string.IsNullOrEmpty(DatabaseName)) && string.IsNullOrEmpty(dbSchema)) return this.WithDatabase(DatabaseName);
            if ((string.IsNullOrEmpty(DatabaseName)) && (!string.IsNullOrEmpty(dbSchema))) return this.WithSchema(dbSchema);
            return this.WithDatabase(DatabaseName).WithSchema(dbSchema);
        }
        public QuerySet<T> Query<T>(string TableName)
        {
            var ret = QueryBuilder.FromEntity<T>(this.Provider.Compiler, true);
            ret.SetTableName(TableName);
            ret.Schema = this.db.GetSchema();
            ret.dbContext = this;
            return ret;
        }



        //[System.Diagnostics.DebuggerStepThrough]
        public QuerySet<T> Query<T>()
        {
            if (CacheDbprovider[this.GetDbName()] == null)
            {
                lock (objLockCacheDbprovider)
                {
                    if (CacheDbprovider[this.GetDbName()] == null)
                    {
                        
                        CacheDbprovider[this.GetDbName()] = this.CreateProvider(this.GetConnectionString());
                    }
                }
            }
            //Globals.ProviderTypes[DbType.ToString()]
            var ret = QueryBuilder.FromEntity<T>(((DbProvider)CacheDbprovider[this.GetDbName()]).Compiler);

            ret.dbContext = this;
            string schema = this.db.GetSchema();
            if (!string.IsNullOrEmpty(schema))
            {
                ret.Schema = schema;
            }
            if (Globals.Mode == SysnMode.OnDemand)
            {
                //var key = $"db={this.GetDbName()},schema={this.db.GetSchema()},table={typeof(T).FullName}".ToLower();
                if (!(Globals.TableSyncCache[this.GetDbName()] is Hashtable))
                {

                    lock (Globals.TableSyncCache)
                    {
                        //Globals.TableSyncCache[this.GetDbName()] = false;
                        lock (ret)
                        {

                            if (!(Globals.TableSyncCache[this.GetDbName()] is Hashtable))
                            {
                                Globals.TableSyncCache[this.GetDbName()] = new Hashtable();
                            }
                        }
                    }
                }
                var Schema = ((Hashtable)Globals.TableSyncCache[this.GetDbName()])[this.db.GetSchema()] as Hashtable;
                if (Schema == null)
                {
                    lock (Globals.TableSyncCache[this.GetDbName()])
                    {
                        if (((Hashtable)Globals.TableSyncCache[this.GetDbName()])[this.db.GetSchema()] == null)
                        {


                            if (((Hashtable)Globals.TableSyncCache[this.GetDbName()])[this.db.GetSchema()] == null)
                            {
                                ((Hashtable)Globals.TableSyncCache[this.GetDbName()])[this.db.GetSchema()] = true;
                                lock (ret)
                                {
                                    var types = typeof(T).Assembly.GetExportedTypes().Where(p => p.GetCustomAttribute<QueryDataTableAttribute>(false) != null).ToList();
                                    ((Hashtable)Globals.TableSyncCache[this.GetDbName()])[this.db.GetSchema()] = types;

                                    IDbConnection cnn = this.CreateNewConnectionFromCurrentConnectionString() as IDbConnection;
                                    if ((cnn.State != ConnectionState.Open) && (cnn.State != ConnectionState.Connecting))
                                    {
                                        lock (cnn)
                                        {
                                            if ((cnn.State != ConnectionState.Open) && (cnn.State != ConnectionState.Connecting))
                                            {
                                                try
                                                {
                                                    cnn.Open();
                                                }
                                                catch (Exception ex)
                                                {

                                                    throw;
                                                }
                                            }
                                        }
                                    }

                                    try
                                    {
                                        foreach (var type in types)
                                        {
                                            //try
                                            //{
                                                var attr = type.GetCustomAttribute<QueryDataTableAttribute>();
                                                Console.WriteLine($"Create tables {attr.TableName}");
                                                this.db.SyncTable(cnn, type, attr.TableName);
                                            //}
                                            //catch (Npgsql.PostgresException ex)
                                            //{

                                            //    throw;
                                            //}
                                        }

                                        cnn.Close();
                                    }
                                    catch (Exception ex)
                                    {
                                        cnn.Close();
                                        if (Globals.OnError!=null)
                                        {
                                            Globals.OnError(new SyncDataSchemaError
                                            {
                                                Exception = ex,
                                                DatabaseName= this.GetDbName(),
                                                DbSchema= this.db.GetSchema(),
                                                Server=this.db.GetServer(),
                                                DbType=this.db.GetDbType().ToString()
                                            });
                                        }


                                    }
                                }

                            }

                        }
                    }
                }


            }
            if (this.IsUseTransaction)
            {
                this.db.BeginTran(this.TrasnsactionLevel);
            }

            return ret;
        }

        public IDbConnection CreateNewConnectionFromCurrentConnectionString()
        {
            return this.db.CreateNewConnectionFromCurrentConnectionString();
        }

        /// <summary>
        /// Just a select statement from nothing such as: 'select 1+1 as Test'
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Selector"></param>
        /// <returns></returns>
        public QuerySet<T> FromNothing<T>(Expression<Func<object, T>> Selector)
        {
            var ret = QueryBuilder.FromNothing<T>(this.Provider.Compiler, Selector);
            ret.dbContext = this;
            return ret;
        }

        public T FirstOrDefault<T>()
        {
            return this.Query<T>().AsQueryable().FirstOrDefault();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public IList<T> ExecToList<T>(BaseQuerySet bQr, string SQL, params ParamConst[] pars)
        {
            try
            {
                return this.db.ExecToList<T>(SQL, pars);
            }
            catch (Exception ex)
            {

                throw new Exceptions.ExcutationError(ex.Message + Environment.NewLine + "Sql:" + Environment.NewLine + bQr.ToString())
                {
                    Sql = SQL,
                    SqlWithParams = SQL,
                    Params = pars.Cast<object>().ToList()
                };
            }
        }

        public DataTable ExecToDataTable(string SQL, params ParamConst[] pars)
        {
            return this.db.ExecToDataTable(SQL, pars);
        }

        public void Dispose()
        {
            this.db.CloseConnection();
        }

        #region The implementation of DML

        public DataActionResult UpdateData<T>(Expression<Func<T, bool>> Conditional, Expression<Func<object, T>> UpdateExpr)
        {
            return this.Query<T>().Where(Conditional).UpdateData(UpdateExpr);
        }
        public DataActionResult Update<T>(Expression<Func<object, T>> UpdateExpr, Expression<Func<T, bool>> Conditional)
        {
            return this.Query<T>().Where(Conditional).UpdateData(UpdateExpr);
        }
        public DataActionResult UpdateDataItem<T>(Expression<Func<T, bool>> Conditional, T Data)
        {
            throw new NotImplementedException();
            //return this.Query<T>().Where(Conditional).UpdateData(Data);
        }
        public UpdateDataResult<T> UpdateData<T>(string Schema, string TableName, Expression<Func<T, bool>> Where, object DataItem, bool ReturnError = false)
        {
            if (DataItem == null)
            {
                throw new Exception("It looks like you forgot set data parameter for update command");
            }
            var ret = new UpdateDataResult<T>();
            var properties = DataItem.GetType().GetProperties().ToList();
            var Params = properties.Select(p => new ParamInfo()
            {
                Name = p.Name,
#if NET_CORE
                Value = p.GetValue(DataItem),
#else
                Value = p.GetValue(DataItem, new object[] { }),
#endif
                Index = properties.IndexOf(p),
                DataType = p.PropertyType
            }).ToList();
            var tbl = this.GetTableInfo(this.db.GetConnectionString(), Schema, TableName);
            //Check require fields:
            var requiredFields = this.GetRequireFieldsForUpdate(tbl, Params);
            var invalidDataType = this.GetInvalidDataTypeFields(tbl, Params);
            if (requiredFields.Count() > 0)
            {
                var err = new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.MissingFields,
                    Fields = requiredFields.Select(p => p.Name).ToArray()
                };
                if (ReturnError)
                {
                    ret.Error = err;
                    return ret;
                }
                else
                {
                    throw (new DataActionError("Missing fields:" + string.Join(",", err.Fields))
                    {
                        Detail = err
                    });
                }
            }
            if (invalidDataType.Count() > 0)
            {
                var err = new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.InvalidDataType,
                    Fields = invalidDataType.Select(p => p.Name).ToArray(),
                    InvalidDataTypeFields = invalidDataType.Select(p => new InvalidDataTypeInfo()
                    {
                        FieldName = p.Name,
                        ExpectedDataType = p.type1,
                        ReceiveDataType = p.type2
                    }).ToArray()
                };
                ret.Error = err;
                if (ReturnError)
                {
                    ret.Error = err;
                    return ret;
                }
                else
                {
                    throw (new DataActionError("Invalid data type at fields:" + string.Join(",", err.Fields))
                    {
                        Detail = err
                    });
                }

            }

            var exceedFields = this.GetExceedFields(tbl, Params);
            if (exceedFields.Count > 0)
            {
                var err = new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.ExceedSize,
                    Fields = exceedFields.Select(p => p.Name).ToArray()
                };
                ret.Error = err;

                if (ReturnError)
                {
                    ret.Error = err;
                    return ret;
                }
                else
                {
                    throw (new DataActionError("Exceed field len at fields:" + string.Join(",", err.Fields))
                    {
                        Detail = err
                    });
                }
            }

            var ex = Where.Body as BinaryExpression;
            var lstParams = new List<ParamConst>();
            lstParams.AddRange(Params.Select(p => new ParamConst
            {
                Value = p.Value,
                Type = p.DataType
            }));
            var lstObjParams = lstParams.Cast<object>().ToList();
            //var strWhere = SelectorCompiler.Gobble<string>(Where.Body, new string[] { Schema }, new string[] { TableName },
            //    Where.Parameters.Cast<object>().ToList(), null, -1, ref lstParams);
            var strWhere = Compiler.GetLogicalExpression(this.Provider.Compiler, Where.Body, new List<FieldMapping>(), (new MapAlais[] {
                 new MapAlais()
                 {
                     Schema=Schema,
                     TableName=TableName
                 }

            }).ToList(), ref lstObjParams);
            lstParams = lstObjParams.Cast<ParamConst>().ToList();
            var Sql = "Update " + this.Provider.Compiler.GetQName(Schema, TableName) +
                " set " + String.Join(",", Params.Select(p => this.Provider.Compiler.GetQName("", p.Name) + "={" + p.Index + "}").ToArray()) + " where " +
                strWhere;
            var retUpdate = this.db.UpdateData(Sql, Schema, TableName, tbl, lstParams.Cast<ParamConst>().ToList());
            if (retUpdate.Error != null)
            {
                if (!ReturnError)
                {
                    throw new DataActionError(retUpdate.ErrorMessage)
                    {
                        Detail = retUpdate.Error
                    };
                }
            }
            ret.Data = retUpdate.Data;
            ret.DataItem = retUpdate.DataItem;
            ret.EffectedRowCount = retUpdate.EffectedRowCount;
            ret.Error = retUpdate.Error;
            ret.NewID = retUpdate.NewID;

            return ret;

        }

        public bool ApplySeq(string Schema, string TableName, string ColumnName, string SeqName)
        {
            return this.db.ApplySeq(Schema, TableName, ColumnName, SeqName);
        }

        public bool CreateSeqIfNotExist(object Connection, string Schema, string name, int start, int increment)
        {
            return this.db.CreateSeqIfNotExist(Connection, Schema, name, start, increment);
        }

        public List<SeqInfo> GetAllSeqs(string schema)
        {
            return this.db.GetAllSeqs(schema);
        }

        public bool CreatePrimaryKey(object Connection, string schema, string TableName, string[] Cols)
        {
            return this.db.CreatePrimaryKey(Connection, schema, TableName, Cols);
        }

        public List<PrimaryKeyInfo> GetAllPrimaryKeys(string schema)
        {
            return this.db.GetAllPrimaryKeys(schema);
        }

        public bool CreateTableIfNotExist(object Connection, string schema, string TableName, DbColumnInfo[] Cols)
        {
            return this.db.CreateTableIfNotExist(Connection, schema, TableName, Cols);
        }

        public bool IsExistDbSchema(string Schema)
        {
            return this.db.IsExistDbSchema(Schema);
        }

        public bool CreateSchemaIfNotExist(object Connection, string Schema)
        {
            var ret = this.db.CreateSchemaIfNotExist(Connection, Schema);
            return ret;
        }

        public List<DataObjectInfo> GetAllTablesInfo(string Schema)
        {
            var ret = this.db.GetAllTablesInfo(Schema);
            return ret;
        }

        public DeleteDataResult<T> DeleteData<T>(Expression<Func<T, bool>> Where, bool ReturnError = false)
        {
            var tblName = QueryBuilder.GetTableName<T>();
            string schema = this.db.GetSchema();
            return DeleteData(schema, tblName, Where, ReturnError);
        }
        /// <summary>
        /// Delete data from schema and table with conditional
        /// </summary>
        /// <exception cref="DataActionError"></exception>
        /// <typeparam name="T"></typeparam>
        /// <param name="Schema"></param>
        /// <param name="TableName"></param>
        /// <param name="Where"></param>
        /// <param name="ReturnError"></param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough]
        public DeleteDataResult<T> DeleteData<T>(string Schema, string TableName, Expression<Func<T, bool>> Where, bool ReturnError = false)
        {
            //throw new NotImplementedException();
            var ex = Where.Body as BinaryExpression;
            var mbxs = utils.GetAllMemberExpression(Where);

            var lstParams = new List<object>();
            var mapAlias = (new MapAlais[]{
                new MapAlais()
                {
                    Schema=Schema,
                    TableName=TableName
                }

            }).ToList();
            foreach (var mb in mbxs)
            {
                mapAlias.Add(new MapAlais
                {
                    Schema = Schema,
                    TableName = TableName,
                    Member = mb.Member,
                    ParamExpr = mb.Expression as ParameterExpression
                });
            }
            string strWhere = Compiler.GetLogicalExpression(this.Provider.Compiler, Where.Body, new List<FieldMapping>(), mapAlias, ref lstParams);
            var sql = "delete from " + this.Provider.Compiler.GetQName(Schema, TableName) + " Where " + strWhere;
            var ret = this.db.ExecCommand(sql, ReturnError, lstParams.Cast<ParamConst>().ToArray());
            return null;
        }
        private List<ColumInfo> GetRequireFields(TableInfo tbl, IEnumerable<ParamInfo> Params)
        {
            return tbl.Columns.Where(p => (!p.AllowDBNull) && ((!p.IsAutoIncrement) &&
                                                      (!p.IsExpression) && (!p.IsIdentity) && (!p.IsReadOnly)))
                                                      .Where(p => !Params.Any(x => x.Name == p.Name & x.Value != null))
                                                    .ToList();
        }
        private List<ColumInfo> GetRequireFieldsForUpdate(TableInfo tbl, List<ParamInfo> Params)
        {
            return tbl.Columns.Where(p => (!p.AllowDBNull) && ((!p.IsAutoIncrement) &&
                                                      (!p.IsExpression) && (!p.IsIdentity) && (!p.IsReadOnly)))
                                                      .Join(Params, p => p.Name, q => q.Name, (p, q) => new
                                                      {
                                                          p.Name,
                                                          q.Value
                                                      })
                                                      .Where(p => p.Value == null).Select(p => new ColumInfo()
                                                      {
                                                          Name = p.Name
                                                      })
                                                    .ToList();
        }
        private List<ExceedFieldInfo> GetExceedFields(TableInfo tbl, IEnumerable<ParamInfo> Params)
        {
            return tbl.Columns.Where(p => (Type)p.DataType == typeof(string)).Join(Params.Where(x => x.DataType == typeof(string) && x.Value != null), p => p.Name, q => q.Name, (p, q) => new ExceedFieldInfo()
            {
                Name = p.Name,
                ColumnSize = p.ColumnSize,
                Value = Value = q.Value.ToString()
            }).Where(p => p.Value.Count() > p.ColumnSize && p.ColumnSize > 0).ToList();
        }
        static Type[] IgnoreTypeCompare = new Type[] {
                 typeof(uint),typeof(UInt32),typeof(UInt64),typeof(Int32),typeof(Int16),
                 typeof(int),typeof(Int16),typeof(Int32),typeof(Int64),
                 typeof(Byte),typeof(ushort),typeof(float),typeof(decimal),typeof(short),typeof(double)
            };
        static Hashtable CacheHasCheckDatabase = new Hashtable();
        static Hashtable CacheDbprovider = new Hashtable();
        static readonly object objLockCacheDbprovider = new object();
        static readonly object objLockCacheHasCheckDatabase = new object();

        private List<ParamInfo> GetInvalidDataTypeFields(TableInfo tbl, IEnumerable<ParamInfo> Params)
        {

            return tbl.Columns.Join(Params, p => p.Name, q => q.Name, (p, q) => new ParamInfo()
            {
                type1 = Nullable.GetUnderlyingType((Type)p.DataType) ?? (Type)p.DataType,
                type2 = Nullable.GetUnderlyingType(q.DataType) ?? q.DataType,
                Name = p.Name
            }).Where(p => p.type1 != p.type2 && IgnoreTypeCompare.Count(x => x == p.type1) == 0 && IgnoreTypeCompare.Count(x => x == p.type2) == 0).ToList();
        }
        /// <summary>
        /// Insert data into table in schema
        /// </summary>
        /// <exception cref="DataActionError"></exception>
        /// <typeparam name="T"></typeparam>
        /// <param name="Schema"></param>
        /// <param name="TableName"></param>
        /// <param name="DataItem"></param>
        /// <param name="ReturnError"></param>
        /// <returns></returns>
        public InsertDataResult<T> InsertData<T>(string Schema, string TableName, object DataItem, bool ReturnError = false)
        {
            if (DataItem == null)
            {
                throw new Exception("It looks like yop forgot set data ('Data' parameter is null)");
            }
            var ret = new InsertDataResult<T>();
            var properties = DataItem.GetType().GetProperties().ToList();
            var Params = properties.Select(p => new ParamInfo()
            {
                Name = p.Name,
#if NET_CORE
                Value = p.GetValue(DataItem),
#else
                Value = p.GetValue(DataItem, new object[] { }),
#endif
                Index = properties.IndexOf(p),
                DataType = p.PropertyType
            });
            var tbl = this.GetTableInfo(this.db.GetConnectionString(), Schema, TableName);
            //Check require fields:
            var requiredFields = this.GetRequireFields(tbl, Params);
            var invalidDataType = this.GetInvalidDataTypeFields(tbl, Params);
            if (requiredFields.Count() > 0)
            {
                var err = new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.MissingFields,
                    Fields = requiredFields.Select(p => p.Name).ToArray()
                };
                if (ReturnError)
                {
                    ret.Error = err;
                    return ret;
                }
                else
                {
                    throw (new DataActionError("Missing fields:" + string.Join(",", err.Fields))
                    {
                        Detail = err
                    });
                }
            }
            if (invalidDataType.Count() > 0)
            {
                var err = new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.InvalidDataType,
                    Fields = invalidDataType.Select(p => p.Name).ToArray()
                };
                ret.Error = err;
                if (ReturnError)
                {
                    ret.Error = err;
                    return ret;
                }
                else
                {
                    throw (new DataActionError("Invalid data type at fields:" + string.Join(",", err.Fields))
                    {
                        Detail = err
                    });
                }

            }

            var exceedFields = this.GetExceedFields(tbl, Params);
            if (exceedFields.Count > 0)
            {
                var err = new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.ExceedSize,
                    Fields = exceedFields.Select(p => p.Name).ToArray()
                };
                ret.Error = err;

                if (ReturnError)
                {
                    ret.Error = err;
                    return ret;
                }
                else
                {
                    throw (new DataActionError("Exceed field len at fields:" + string.Join(",", err.Fields))
                    {
                        Detail = err
                    });
                }
            }
            var sqlGetIdentitySQL = this.Provider.Compiler.CreateSQLGetIdentity(Schema, TableName, tbl);
            var sql = "insert into " + this.Provider.Compiler.GetQName(Schema, TableName) +
                "(" +
                string.Join(",", Params.Select(p => this.Provider.Compiler.GetQName("", p.Name)).ToArray()) + ") " +
                "values(" + string.Join(",", Params.Select(p => "{" + p.Index + "}").ToArray()) + ")";
            sql = sql + ";" + sqlGetIdentitySQL;
            var retInsert = this.db.ExecCommand(sql, ReturnError, Params.Cast<ParamConst>().ToArray());
            retInsert.DataItem = DataItem;
            ret.Data = retInsert.Data;
            ret.DataItem = retInsert.DataItem;
            ret.EffectedRowCount = retInsert.EffectedRowCount;
            ret.Error = retInsert.Error;
            ret.NewID = retInsert.NewID;
            return ret;
        }
        public TableInfo GetTableInfo(string ConnectionString, string Schema, string TableName)
        {
            var hashkey = string.Format("cnn={0};schem={1};table={2}", ConnectionString, Schema, TableName);
            if (Globals.TableInfo == null)
            {
                Globals.TableInfo = new Hashtable();
            }
            if (Globals.TableInfo[hashkey] != null)
            {
                return (TableInfo)Globals.TableInfo[hashkey];
            }
            lock (Globals.TableInfo)
            {
                var ret = this.db.GetTableInfo(ConnectionString, Schema, TableName);
                Globals.TableInfo[hashkey] = ret;
            }
            return (TableInfo)Globals.TableInfo[hashkey];


        }
        /// <summary>
        /// Excute sql command
        /// </summary>
        /// <param name="SQL">Sql for excutation</param>
        /// <param name="pars">Input param</param>
        /// <returns></returns>
        /// <exception cref="LV.Db.Common.DataActionError">DataActionError</exception>
        /// <exception cref="Exceptions.Error"></exception>
       // [System.Diagnostics.DebuggerStepThrough]
        public DataActionResult ExcuteCommand(string SQL, params ParamConst[] pars)
        {
            try
            {
                var retInsert = this.db.ExecCommand(SQL, false, pars);
                return retInsert;
            }
            catch (DataActionError ex)
            {
                if (ex.Detail.ErrorType == DataActionErrorTypeEnum.DuplicateData)
                {
                    throw new Exceptions.Error(ex.Message)
                    {
                        ErrorType = Exceptions.ErrorType.DuplicatetData,
                        Fields = ex.Detail.Fields,

                    };
                }
                if (ex.Detail.ErrorType == DataActionErrorTypeEnum.ExceedSize)
                {
                    throw new Exceptions.Error(ex.Message)
                    {
                        ErrorType = Exceptions.ErrorType.ValueIsExceed,
                        Fields = ex.Detail.Fields,

                    };
                }
                if (ex.Detail.ErrorType == DataActionErrorTypeEnum.ForeignKey)
                {
                    throw new Exceptions.Error(ex.Message)
                    {
                        ErrorType = Exceptions.ErrorType.ForeignKey,
                        Fields = ex.Detail.Fields,
                        RefTables = ex.Detail.RefTables
                    };
                }
                if (ex.Detail.ErrorType == DataActionErrorTypeEnum.InvalidDataType)
                {
                    throw new Exceptions.Error(ex.Message)
                    {
                        ErrorType = Exceptions.ErrorType.InvalidDataType,
                        Fields = ex.Detail.Fields,
                        DetailInvalidataTypeFields = ex.Detail.InvalidDataTypeFields

                    };
                }
                if (ex.Detail.ErrorType == DataActionErrorTypeEnum.MissingFields)
                {
                    throw new Exceptions.Error(ex.Message)
                    {
                        ErrorType = Exceptions.ErrorType.MissFields,
                        Fields = ex.Detail.Fields

                    };
                }
                throw ex;
            }
            catch (Exception ex)
            {
                var ret = new ExcuteCommandException(string.Format("Error in {0}. Detail :{1}", SQL, ex.Message));
                throw ret;
            }
        }
        [System.Diagnostics.DebuggerStepThrough]
        public int Count<T>(Expression<Func<T, bool>> Expr)
        {
            return this.Query<T>().AsQuerySet().Filter(Expr).Count();
        }
        [System.Diagnostics.DebuggerStepThrough]
        public int Count<T>()
        {
            return this.Query<T>().AsQuerySet().Count();
        }

        public string GetDbName()
        {
            return this.db.GetDataBase();
        }

        public string GetConnectionString()
        {
            return this.db.GetConnectionString();
        }


        #endregion
    }
}