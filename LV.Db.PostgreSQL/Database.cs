using LV.Db.Common;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LV.Db.PostgreSQL
{
    public class Database : IDb
    {
        private NpgsqlConnection cnn;
        private string schema;
        internal NpgsqlTransaction tran;
        private Dictionary<string, string> cnnInfo;
        public Database()
        {
            if (string.IsNullOrEmpty(this.ConnectionString))
            {
                this.ConnectionString = Globals.ConnectionString;
            }

        }
        public void SetConnectionString(string connectionString)
        {
            this.ConnectionString = connectionString;
        }
        public string ConnectionString
        {

            get
            {
                return _connectString;
            }
            set
            {
                _connectString = value;
                
                cnn = null;
                if (string.IsNullOrEmpty(value))
                {
                    cnnInfo = new Dictionary<string, string>();
                    return;
                }
                var items = _connectString.Split(';');
                cnnInfo = new Dictionary<string, string>();
                foreach (var item in items)
                {
                    if (item.IndexOf("=") > -1)
                    {
                        var key = item.Split('=')[0].TrimStart(' ').TrimEnd(' ').Replace("  ", " ");
                        var _value = item.Split('=')[1];
                        cnnInfo.Add(key, _value);
                    }
                }
            }
        }
        public void SetSchema(string schema)
        {
            this.schema = schema;
            if(!this.IsExistDbSchema(schema))
            {
                this.CreateDbSchema(schema);
            }
        }

        public void CreateDbSchema(string schema)
        {
            if (string.IsNullOrEmpty(schema) || schema == "public") return;
            //CREATE SCHEMA "ddddd"
            var cmd = new Npgsql.NpgsqlCommand();
            cmd.CommandText = $"CREATE SCHEMA \"{schema}\"";
            cmd.Connection = new NpgsqlConnection(this.ConnectionString);
            cmd.Connection.Open();
            try
            {
                cmd.ExecuteNonQuery();
                cmd.Clone();
            }
            catch(Exception ex)
            {
                cmd.Clone();
                throw (ex);
            }
    
            
        }

        public string GetSchema()
        {
            if (string.IsNullOrEmpty(this.schema))
            {
                this.schema = "public";
            }
            return this.schema;
        }
        public NpgsqlConnection Connection
        {
            get
            {
                if (cnn == null) cnn = new NpgsqlConnection(this.ConnectionString);
                return cnn;
            }
        }


        private void OpenConnect()
        {
            if (this.Connection.State == ConnectionState.Connecting) return;
            if (this.Connection.State != ConnectionState.Open)
            {
                this.Connection.Open();
            }
        }
        public void BeginTran(DbIsolationLevel level)
        {
            this.OpenConnect();

            if (tran == null)
            {
                tran = this.Connection.BeginTransaction();
            }
            
        }

        public void Commit()
        {
            if (tran != null)
                tran.Commit();
            tran = null;
        }

        public void RollBack()
        {
            if (tran != null)
            {
                if (!tran.IsCompleted)
                {
                    tran.Rollback();
                }
                tran.Dispose();
                tran = null;
            }
            
            
        }
        public void CloseConnection()
        {
            this.RollBack();
            this.Connection.Close();
            this.Connection.Dispose();
        }
        [System.Diagnostics.DebuggerStepThrough]
        public DataTable ExecToDataTable(NpgsqlConnection Conn, string SQL, params ParamConst[] pars)
        {
            if (Conn == null)
            {
                throw new Exception("Connection can not be null");
            }
           // NpgsqlCommand command = new NpgsqlCommand(SQL, Conn);
            var command = Conn.CreateCommand();
            command.CommandText = SQL;
            command.CommandType = CommandType.Text;
            try
            {
                command.Transaction = this.tran;
                var paramIndex = 0;
                foreach (var p in pars)
                {
                    var paramName = string.Format(":p{0}", paramIndex);
                    command.CommandText = command.CommandText.Replace("{" + paramIndex.ToString() + "}", paramName);
                    var sqlParam = new NpgsqlParameter();
                    sqlParam.ParameterName = paramName;
                    if (p.Value != null)
                    {
                        sqlParam.Value = p.Value;
                        sqlParam.DbType = GetDbType(p.Type);
                    }
                    else
                    {
                        sqlParam.Value = DBNull.Value;
                        sqlParam.DbType = GetDbType(p.Type);

                    }
                    command.Parameters.Add(sqlParam);
                    paramIndex++;

                }
                var adp = new NpgsqlDataAdapter(command);
                var tbl = new DataTable();
                adp.Fill(tbl);
                return tbl;
            }
            catch (Exception ex)
            {
                throw(ex);
            }
        }
        public DataTable ExecToDataTable(string SQL, params ParamConst[] pars)
        {
            if (this.Connection.State != ConnectionState.Open)
            {
                this.Connection.Open();
            }
            return ExecToDataTable(this.Connection, SQL, pars);
        }
        Dictionary<Type, DbType> typeMap = new Dictionary<Type, DbType>();
        bool isInitDbType = false;
        private string _server;
        private string _port;
        private string _password;
        private string _userId;
        static  Hashtable CacheTableSync = new Hashtable();
        private string _dbName;
        private string _connectString;
        static Dictionary<string, CreateFKContext> CacheAsyncCreateFK=new Dictionary<string, CreateFKContext>();
        static Hashtable CacheLockFK =new Hashtable();
        private static Hashtable CheckIfPrimitiveTypeCache;
        private readonly object objLockCacheTableSync = new object();
        private readonly object objLockCacheLockFK=new object();

        [System.Diagnostics.DebuggerStepThrough]
        private DbType GetDbType(Type type)
        {
            if (isInitDbType)
            {
                return typeMap[type];
            }
            typeMap = new Dictionary<Type, DbType>();
            typeMap[typeof(byte)] = DbType.Byte;
            typeMap[typeof(sbyte)] = DbType.SByte;
            typeMap[typeof(short)] = DbType.Int16;
            typeMap[typeof(ushort)] = DbType.UInt16;
            typeMap[typeof(int)] = DbType.Int32;
            typeMap[typeof(uint)] = DbType.UInt32;
            typeMap[typeof(long)] = DbType.Int64;
            typeMap[typeof(ulong)] = DbType.UInt64;
            typeMap[typeof(float)] = DbType.Single;
            typeMap[typeof(double)] = DbType.Double;
            typeMap[typeof(decimal)] = DbType.Decimal;
            typeMap[typeof(bool)] = DbType.Boolean;
            typeMap[typeof(string)] = DbType.String;
            typeMap[typeof(char)] = DbType.StringFixedLength;
            typeMap[typeof(Guid)] = DbType.Guid;
            typeMap[typeof(DateTime)] = DbType.DateTime;
            typeMap[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            typeMap[typeof(byte[])] = DbType.Binary;
            typeMap[typeof(byte?)] = DbType.Byte;
            typeMap[typeof(sbyte?)] = DbType.SByte;
            typeMap[typeof(short?)] = DbType.Int16;
            typeMap[typeof(ushort?)] = DbType.UInt16;
            typeMap[typeof(int?)] = DbType.Int32;
            typeMap[typeof(uint?)] = DbType.UInt32;
            typeMap[typeof(long?)] = DbType.Int64;
            typeMap[typeof(ulong?)] = DbType.UInt64;
            typeMap[typeof(float?)] = DbType.Single;
            typeMap[typeof(double?)] = DbType.Double;
            typeMap[typeof(decimal?)] = DbType.Decimal;
            typeMap[typeof(bool?)] = DbType.Boolean;
            typeMap[typeof(char?)] = DbType.StringFixedLength;
            typeMap[typeof(Guid?)] = DbType.Guid;
            typeMap[typeof(DateTime?)] = DbType.DateTime;
            typeMap[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
            typeMap[typeof(object)] = DbType.Object;
            isInitDbType = true;
            //typeMap[typeof(System.Data.Linq.Binary)] = DbType.Binary;
            return typeMap[type];
        }

        [System.Diagnostics.DebuggerStepThrough]
        public IList<T> ExecToList<T>(string SQL, params ParamConst[] pars)
        {

            return ExecToList<T>(this.Connection, SQL, pars);


        }
        [System.Diagnostics.DebuggerStepThrough]
        public IList<T> ExecToList<T>(string ConnectionString, string SQL, params ParamConst[] pars)
        {
            var cnn = new Npgsql.NpgsqlConnection(ConnectionString);
            cnn.Open();
            try
            {
                var ret= ExecToList<T>(cnn, SQL, pars);
                cnn.Close();
                return ret;

            }
            catch(Exception ex)
            {
                cnn.Close();
                throw (ex);
                
            }

        }
        public IList<T> ExecToList<T>(IDbConnection Connection, string SQL, params ParamConst[] pars)
        {
            var cnn = (NpgsqlConnection)Connection;
            var ret = ExecToList<T>(cnn, SQL, pars);
            return ret;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public IList<T> ExecToList<T>(NpgsqlConnection Connection, string SQL, params ParamConst[] pars)
        {

            var dataTable = this.ExecToDataTable(Connection, SQL, pars);
            return DataLoader.ToList<T>(dataTable);


        }
        #region Implemmetation of DML   



        public string GetConnectionString()
        {
            return this.ConnectionString;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public TableInfo GetTableInfo(string ConnectionString, string schema, string table_name)
        {
            var ret = new TableInfo();

            var sql = @"SELECT column_name, column_default,is_nullable,data_type,character_maximum_length
                            FROM information_schema.columns
                            WHERE table_schema = :schema
                            AND table_name   = :table";
            var cnn = new Npgsql.NpgsqlConnection(ConnectionString);
            var cmd = new Npgsql.NpgsqlCommand("select * from " + GetQName(schema, table_name), cnn);
            //cmd.Parameters.Add(new NpgsqlParameter()
            //{
            //    ParameterName="schema",
            //    Value=schema
            //});
            //cmd.Parameters.Add(new NpgsqlParameter()
            //{
            //    ParameterName = "table",
            //    Value = table_name
            //});
            DataTable tbl = new DataTable();
            //var adp = new Npgsql.NpgsqlDataAdapter(cmd);


            try
            {
                cnn.Open();
                tbl = cmd.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly).GetSchemaTable();

                //adp.Fill(tbl);
            }
            catch (Exception ex)
            {
                cnn.Close();
                throw (ex);
            }
            finally
            {
                cnn.Close();
            }
            ret.Columns = tbl.Rows.Cast<DataRow>().Select(p => new ColumInfo()
            {
                Name = p["ColumnName"].ToString(),
                IsUnique = (p["IsUnique"] == DBNull.Value) ? false : (bool)p["IsUnique"],
                IsAutoIncrement = (p["IsAutoIncrement"] == DBNull.Value) ? false : (bool)p["IsAutoIncrement"],
                IsKey = (p["IsKey"] == DBNull.Value) ? false : (bool)p["IsKey"],
                AllowDBNull = (p["AllowDBNull"] == DBNull.Value) ? false : (bool)p["AllowDBNull"],
                IsReadOnly = (p["IsReadOnly"] == DBNull.Value) ? false : (bool)p["IsReadOnly"],
                IsExpression = (p["IsReadOnly"] == DBNull.Value) ? false : (bool)p["IsReadOnly"],
                IsIdentity = (p["IsIdentity"] == DBNull.Value) ? false : (bool)p["IsIdentity"],
                DataType = p["DataType"],
                ColumnSize = (int)p["ColumnSize"],
                TableName = table_name
                //DefaultValue = p["column_default"].ToString(),
                //IsAuto = p["column_default"].ToString().Split("(")[0] == "nextval",
                //AutoConstraint = ((p["column_default"].ToString().Split("(")[0] == "nextval") ? p["column_default"].ToString().Split("(")[1].Split("::")[0] : "")
            }).ToList();

            return ret;
        }

        private string GetQName(string schema, string name)
        {
            if (string.IsNullOrEmpty(schema))
            {
                return @"""" + name + @"""";
            }
            else
            {
                return string.Format(@"""{0}"".""{1}""", schema, name);
            }
        }
        /// <summary>
        /// Exec command
        /// </summary>
        /// <exception cref="DataActionError"></exception>
        /// <param name="sql"></param>
        /// <param name="ReturnError"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough]
        public DataActionResult ExecCommand(string sql, bool ReturnError, params ParamConst[] Params)
        {
            var ret = new DataActionResult()
            {
                Data = Params
            };
            var cmd = this.Connection.CreateCommand();
            this.OpenConnect();
            cmd.Transaction = this.tran;
            var adp = new NpgsqlDataAdapter(cmd);
            cmd.CommandText = sql;
            var paramIndex = 0;
            foreach (var p in Params)
            {
                cmd.CommandText = cmd.CommandText.Replace("{" + paramIndex.ToString() + "}", ":p" + paramIndex.ToString());
                cmd.Parameters.Add(new NpgsqlParameter()
                {
                    ParameterName = ":p" + paramIndex.ToString(),
                    Value = ((p.Value == null) ? DBNull.Value : p.Value)
                });
                paramIndex++;
            }
            try
            {
                var tbl = new DataTable();
                adp.Fill(tbl);
                if (tbl.Rows.Count > 0)
                {
                    ret.NewID = new Hashtable();
                    foreach (DataColumn col in tbl.Columns)
                    {
                        ret.NewID[col.ColumnName] = tbl.Rows[0][col];
                    }
                }
            }
            catch (PostgresException ex)
            {
                //if(ex.SqlState== "23502")
                //{
                //    var dataField = ex.Message.Split('"')[1].Split('"')[0];
                //    throw new LV.Db.Common.Exceptions.DataInputError();
                //}
                ErrorAction error = Utils.ProcessException(ex);
                if (ReturnError)
                {
                    ret.Error = error;
                    ret.ErrorMessage = ex.Message;
                    return ret;
                }
                else
                {
                    throw (new DataActionError(ex.Message)
                    {
                        Detail = error
                    });
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return ret;
        }
        /// <summary>
        /// Update data by sql command
        /// </summary>
        /// <exception cref="DataActionError"></exception>
        /// <param name="SqlCommandText"></param>
        /// <param name="Schema"></param>
        /// <param name="TableName"></param>
        /// <param name="tblInfo"></param>
        /// <param name="InputParams"></param>
        /// <returns></returns>
        public DataActionResult UpdateData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<ParamConst> InputParams)
        {
            try
            {
                var ret = this.ExecCommand(SqlCommandText, true, InputParams.ToArray());
                return ret;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public DataActionResult InsertData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<ParamConst> InputParams)
        {
            throw new NotImplementedException();
        }

        public DataActionResult DeleteData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<ParamConst> InputParams)
        {
            throw new NotImplementedException();
        }

        public UpdateDataResult<T> UpdateData<T>(string schema, string table_name, Expression<Func<T, bool>> where, object dataUpdate, bool returnError)
        {
            throw new NotImplementedException();
        }

        public List<DataObjectInfo> GetAllTablesInfo(string schema)
        {
            var regexCheckSequence = new Regex(@"nextval\(\'.+\'\:\:regclass\)");
            var cmd = this.Connection.CreateCommand();
            cmd.CommandText = @"SELECT 
                                t.table_name as ""Name"", 
                                c.column_name as ""ColName"",
                                c.udt_name as ""DbType"",
                                c.is_nullable,
                                c.character_maximum_length,
                                c.column_default,
                                c.numeric_precision,
                                c.numeric_precision_radix,
                                c.numeric_scale
                                FROM information_schema.tables t
                                inner join information_schema.columns c on t.table_name = c.table_name and t.table_schema = c.table_schema
                                WHERE t.table_schema = :schema";
            cmd.Parameters.Add(new NpgsqlParameter()
            {
                ParameterName = "schema",
                Value = schema
            });
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
            }
            var tbl = new DataTable();
            var adp = new Npgsql.NpgsqlDataAdapter();
            adp.SelectCommand = cmd;
            adp.Fill(tbl);

            var tmp1 = tbl.Rows.Cast<DataRow>().Select(p => new
            {
                tblName = p["Name"].ToString(),
                colName = p["ColName"].ToString(),
                dbTpe = p["DbType"].ToString(),
                is_nullable = p["is_nullable"].ToString() == "YES",
                dbLen = (p["character_maximum_length"] != DBNull.Value) ? ((int)(p["character_maximum_length"])) : -1,
                column_default = (p["column_default"] != DBNull.Value) ? p["column_default"].ToString() : null,
                numeric_precision = (p["numeric_precision"] != DBNull.Value) ? ((int)p["numeric_precision"]) : -1,
                numeric_precision_radix = (p["numeric_precision_radix"] != DBNull.Value) ? ((int)p["numeric_precision_radix"]) : -1,
                numeric_scale = (p["numeric_scale"] != DBNull.Value) ? ((int)p["numeric_scale"]) : -1,
                is_sequence = (p["column_default"] != DBNull.Value) ? regexCheckSequence.IsMatch(p["column_default"].ToString()) : false,
                seqs = (p["column_default"] != DBNull.Value) ? regexCheckSequence.Match(p["column_default"].ToString()) : null,

            }).ToList();
            var tmp = tmp1.Select(p => new
            {
                p.is_nullable,
                p.is_sequence,
                p.numeric_precision,
                p.numeric_precision_radix,
                p.numeric_scale,
                seq_name = (p.seqs != null && p.seqs.Value != "") ? p.seqs.Value.Split("'")[1].Split("'")[0] : null,
                p.tblName,
                p.colName,
                column_default = (p.is_sequence) ? null : p.column_default,
                p.dbTpe,
                p.dbLen

            }).ToList();

            List<DataObjectInfo> ret = tmp.GroupBy(p => p.tblName).Select(p => new DataObjectInfo
            {
                Name = p.FirstOrDefault().tblName,
                ObjectType = "Table",
                Columns = p.Select(x => new DataObjectInfo
                {
                    Name = x.colName,
                    ObjectType = "Column",
                    DbType = x.dbTpe,
                    IsRequire = !x.is_nullable,
                    DbLen = x.dbLen,
                    DefaultValue = x.column_default,
                    Numeric_Precision = x.numeric_precision,
                    Numeric_Precision_Radix = x.numeric_precision_radix,
                    Numeric_Scale = x.numeric_scale,
                    IsSeq = x.is_sequence,
                    SeqName = x.seq_name
                }).ToList()

            }).ToList();
            return ret;


        }

        public bool CreateSchemaIfNotExist(object Connection, string schema)
        {
            var cnn = (Npgsql.NpgsqlConnection)Connection;
            var sqlCommand = string.Format(@"DO 
                              $$ 
                                BEGIN
                                  If not  EXISTS(SELECT 1 FROM information_schema.schemata 
                                          WHERE schema_name = '{0}') Then
                                        CREATE SCHEMA ""{0}"";
                                  end if;
                                END
                            $$;", schema);
            var cmd = cnn.CreateCommand();
            cmd.CommandText = sqlCommand;
            cmd.ExecuteNonQuery();
            return true;

        }

        public bool IsExistDbSchema(string schema)
        {
            var cmd = this.Connection.CreateCommand();
            cmd.CommandText = @"SELECT EXISTS(SELECT 1 FROM information_schema.schemata 
                                WHERE schema_name = :SchemaName)";
            cmd.Parameters.Add(new NpgsqlParameter
            {
                ParameterName = "SchemaName",
                Value = schema
            });
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
            }
            var drd = cmd.ExecuteReader();
            var ret = false;
            if (drd.Read())
            {
                ret = drd.GetBoolean(0);
            }
            drd.Close();
            return ret;
        }

        public bool CreateTableIfNotExist(object Connection, string schema, string tableName, DbColumnInfo[] cols)
        {
            var cnn = (Npgsql.NpgsqlConnection)Connection;
            var sqlCreateTable = string.Format(@"DO
                                $$
                                BEGIN
                                            if not exists(SELECT 1  
				                                FROM information_schema.tables t
				                                WHERE t.table_schema='{0}' and t.table_name='{1}') then
				                                CREATE TABLE ""{0}"".""{1}"" (
                                                       @Columns
       			                                );
                                             end if;
                                END
                                $$;", schema, tableName);
            var strCols = "";
            var lstCols = new List<string>();
            foreach (var col in cols)
            {
                var strFieldname = col.Name;
                var strType = @"""" + col.DbType + @"""";
                if (col.DbLen > -1)
                {
                    strType += "(" + col.DbLen + ")";
                }
                if (col.DefaultValue != null)
                {
                    strType += " default " + col.DefaultValue;
                }
                if (col.IsRequire)
                {
                    lstCols.Add(string.Format(@"""{0}"" {1} not null", strFieldname, strType));
                }
                else
                {
                    lstCols.Add(string.Format(@"""{0}"" {1}", strFieldname, strType));
                }
            }
            strCols = string.Join(",", lstCols.ToArray());
            var txtCommand = sqlCreateTable.Replace("@Columns", strCols);
            var cmd = cnn.CreateCommand();
            cmd.CommandText = txtCommand;
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
            }
            cmd.ExecuteNonQuery();
            return true;
        }

        public List<PrimaryKeyInfo> GetAllPrimaryKeys(string schema)
        {
            var sql = @"select kcu.table_schema,
                           kcu.table_name,
                           tco.constraint_name,
                           kcu.ordinal_position as position,
                           kcu.column_name as key_column
                    from information_schema.table_constraints tco
                    join information_schema.key_column_usage kcu 
                         on kcu.constraint_name = tco.constraint_name
                         and kcu.constraint_schema = tco.constraint_schema
                         and kcu.constraint_name = tco.constraint_name
                    where tco.constraint_type = 'PRIMARY KEY' and kcu.table_schema=:schema
                    order by kcu.table_schema,
                             kcu.table_name,
                             position;";
            var cmd = this.Connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new NpgsqlParameter
            {
                ParameterName = "schema",
                Value = schema
            });
            var adp = new Npgsql.NpgsqlDataAdapter();
            adp.SelectCommand = cmd;
            var tbl = new DataTable();
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
            }
            adp.Fill(tbl);
            var tmp = tbl.Rows.Cast<DataRow>().Select(p => new
            {
                table_name = p["table_name"].ToString(),
                constraint_name = p["constraint_name"].ToString(),
                column_name = p["key_column"].ToString()
            }).ToList();
            List<PrimaryKeyInfo> ret = tmp.GroupBy(p => p.table_name)
                .Select(p => new PrimaryKeyInfo
                {
                    TableName = p.Key,
                    Constraint = p.GroupBy(fx => new { fx.table_name, fx.constraint_name })
                    .Select(x => new PrimaryKeyConstraintInfo
                    {
                        Name = x.Key.constraint_name,
                        Columns = x.Select(fx => new PrimaryKeyConstraintInfoColumn
                        {
                            Name = fx.column_name

                        }).ToList()
                    }).ToList()
                }).ToList();
            return ret;
        }

        public bool CreatePrimaryKey(object Connection, string schema, string tableName, string[] cols)
        {
            var cnn = Connection as Npgsql.NpgsqlConnection;
            var sql = string.Format(@"do
                        $$
                        begin
                        If not  EXISTS(
			                          select * from information_schema.table_constraints 
			                          where table_name = '{1}' and 
			                          constraint_type = 'PRIMARY KEY' and 
			                          constraint_schema='{0}') then
                                      ALTER TABLE ""{0}"".""{1}""
                                          ADD PRIMARY KEY(@columns);
                                    End If;
                                    end
                        $$", schema, tableName);
            var txtCols = string.Join(",", cols.Select(p => @"""" + p + @"""").ToArray());
            var cmd = cnn.CreateCommand();
            cmd.CommandText = sql.Replace("@columns", txtCols);
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
            }
            var ret = cmd.ExecuteNonQuery();
            return ret > 0;
        }

        public List<SeqInfo> GetAllSeqs(string schema)
        {
            var cmd = this.Connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM information_schema.sequences s where s.sequence_schema =:schema";
            cmd.Parameters.Add(new NpgsqlParameter
            {
                ParameterName = "schema",
                Value = schema
            });
            var tbl = new DataTable();
            var adp = new Npgsql.NpgsqlDataAdapter();
            adp.SelectCommand = cmd;
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
            }
            adp.Fill(tbl);
            return tbl.Rows.Cast<DataRow>().Select(p => new SeqInfo
            {
                Name = p["sequence_name"].ToString(),
                DbType = p["data_type"].ToString(),
                Start = int.Parse(p["start_value"].ToString()),
                Increment = int.Parse(p["increment"].ToString())
            }).ToList();
        }

        public bool CreateSeqIfNotExist(object Connection, string schema, string name, int start, int increment)
        {
            var cnn = (Npgsql.NpgsqlConnection)Connection;
            var sql = string.Format(@"do
                        $$
                        begin
                        If not  EXISTS(
			                          SELECT * FROM information_schema.sequences s
			                          where s.sequence_schema='{0}' and
			                          s.sequence_name='{1}'
			                          ) then
                                      CREATE SEQUENCE ""{0}"".""{1}"" START {2} INCREMENT {3};
                                    End If;
                            end
                        $$", schema, name, start, increment);
            var cmd = cnn.CreateCommand();
            cmd.CommandText = sql;
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
            }
            var ret = cmd.ExecuteNonQuery();
            return ret > 0;

        }

        public bool ApplySeq(string schema, string tableName, string columnName, string seqName)
        {
            var cmd = this.Connection.CreateCommand();
            cmd.CommandText = string.Format(@"ALTER TABLE ""{0}"".""{1}"" ALTER COLUMN ""{2}"" SET DEFAULT nextval('{0}.{3}'::regclass);", schema, tableName, columnName, seqName);
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
            }
            var ret = cmd.ExecuteNonQuery();
            return ret > 0;
        }

        public void SetDataBaseName(string databaseName)
        {

            this.ConnectionString = "server=" + GetServer() + ";User Id=" + GetUserId() + ";port=" + GetPort() + ";password=" + GetPassword() + ";Database=" + databaseName;
        }

        public string GetServer()
        {
            if (string.IsNullOrEmpty(this._server))
            {
                this._server = this.cnnInfo.FirstOrDefault(p => string.Equals(p.Key, "server", StringComparison.OrdinalIgnoreCase)).Value;
            }
            return this._server;
        }
        public string GetPassword()
        {
            if (string.IsNullOrEmpty(this._password))
            {
                this._password = this.cnnInfo.FirstOrDefault(p => string.Equals(p.Key, "password", StringComparison.OrdinalIgnoreCase)).Value;
            }
            return this._password;
        }
        public string GetPort()
        {
            if (string.IsNullOrEmpty(this._port))
            {
                if (this.ConnectionString.ToLower().Replace(" ", "").IndexOf("port=") > -1)
                {
                    this._port = this.ConnectionString.ToLower().Replace(" ", "").Split("port=")[1].Split(';')[0];
                }
                else
                {
                    this._port = "5432";
                }
            }
            return this._port;
        }

        public string GetUserId()
        {
            if (string.IsNullOrEmpty(this._userId))
            {
                this._userId = this.cnnInfo.FirstOrDefault(p => string.Equals(p.Key, "user id", StringComparison.OrdinalIgnoreCase)).Value;
            }
            return this._userId;
        }

        public string GetDataBase()
        {
            if (string.IsNullOrEmpty(this._dbName))
            {
                this._dbName = this.cnnInfo.FirstOrDefault(p => string.Equals(p.Key, "database", StringComparison.OrdinalIgnoreCase)).Value;
            }
            return this._dbName;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public void CreateDbIfNotExists(string DbName)
        {
            if (string.IsNullOrEmpty(DbName))
            {
                throw new Exception("DbName is null");
            }
            if (this.ExecCommandAndGetValue<int>(this.ConnectionString, "SELECT count(*) FROM pg_database WHERE datname = {0}", DbName) == 0)
            {
                var sqlCommand = $"CREATE DATABASE \"{DbName}\" WITH OWNER = {GetUserId()} ENCODING = 'UTF8' CONNECTION LIMIT = -1;";

                var cmd = new Npgsql.NpgsqlCommand();
                cmd.CommandText = sqlCommand;
                cmd.Connection = new NpgsqlConnection(this.ConnectionString);
                cmd.Connection.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                catch (Exception ex)
                {
                    cmd.Connection.Close();
                    throw ex;
                }
            }

        }
        public T ExecCommandAndGetValue<T>(IDbConnection Connection, string Sql, params object[] Params)
        {
            var ret = this.ExecSqlToList<T>(Connection, Sql, Params);
            if (ret == null) return default(T);
            return ret.FirstOrDefault();
        }
        public T ExecCommandAndGetValue<T>(string ConnectionString, string Sql, params object[] Params)
        {
            var ret = this.ExecSqlToList<T>(ConnectionString,Sql, Params);
            if (ret == null) return default(T);
            return ret.FirstOrDefault();
        }
        public T ExecCommandAndGetValue<T>(string Sql, params object[] Params)
        {
            var ret = this.ExecSqlToList<T>(Sql, Params);
            return ret.FirstOrDefault();
        }
        public IList<T> ExecSqlToList<T>(string ConnectionString, string Sql, params object[] Params)
        {
            var ret = this.ExecToList<T>(ConnectionString,Sql, Params.Select(p => new ParamConst
            {

                Type = (p != null) ? p.GetType() : null,
                Value = p
            }).ToArray());
            return ret;
        }
        public IList<T> ExecSqlToList<T>(IDbConnection Connection, string Sql, params object[] Params)
        {
            var ret = this.ExecToList<T>(Connection, Sql, Params.Select(p => new ParamConst
            {

                Type = (p != null) ? p.GetType() : null,
                Value = p
            }).ToArray());
            return ret;
        }
        public IList<T> ExecSqlToList<T>(string Sql, params object[] Params)
        {
            var ret = this.ExecToList<T>(Sql, Params.Select(p => new ParamConst
            {

                Type = (p != null) ? p.GetType() : null,
                Value = p
            }).ToArray());
            return ret;
        }
       
        public void SyncTable(object Connection, Type ModelType, string table_name)
        {
            var schema = string.IsNullOrEmpty(this.schema) ? "public" : this.schema;
            var synKey = $"dbname={GetDataBase()};schema={schema};type={ModelType.FullName};table={table_name}";
            try
            {
                if (!this.IsExisTable(Connection, table_name))
                {
                    this.CreateTableIfNotExist(ModelType, Connection, table_name);
                    this.ModifyAutoIncrement(ModelType, Connection, table_name);
                    this.ModifyPrimaryKey(ModelType, Connection, table_name);
                }
                else
                {
                    this.ModifyTable(ModelType, Connection, table_name);
                }

                this.ModifyUniqueConstraints(ModelType, Connection, table_name);
                CacheTableSync[synKey] = ModelType;
                if (ModelType.GetInterfaces().Count(p => p == typeof(IForeignKeyModel)) > 0)
                {
                    var retInstance = (IForeignKeyModel)ModelType.Assembly.CreateInstance(ModelType.FullName);
                    var lst = retInstance.CreateForeignKeys();
                    foreach (var item in lst)
                    {
                        if (CacheTableSync[$"dbname={GetDataBase()};schema={schema};type={item.TableType.FullName};table={item.ForeignTable}"] == null)
                        {
                            item.CreateTableAction(this, Connection);
                            CacheTableSync[$"dbname={GetDataBase()};schema={schema};type={item.TableType.FullName};table={item.ForeignTable}"] = item.TableType;
                            var modelTypes = item.TableType.Assembly.GetExportedTypes().Where(p => p.GetCustomAttribute<QueryDataTableAttribute>(false) != null)
                                .Where(p=>p!=item.TableType).ToList();
                            foreach(var x in modelTypes)
                            {
                                var attr = x.GetCustomAttribute<QueryDataTableAttribute>();
                                if (CacheTableSync[$"dbname={GetDataBase()};schema={schema};type={x.FullName};table={attr.TableName}"] == null)
                                {
                                    if (!this.IsExisTable(Connection, attr.TableName))
                                    {
                                        this.CreateTableIfNotExist(x, Connection, attr.TableName);
                                        this.ModifyAutoIncrement(x, Connection, attr.TableName);
                                        this.ModifyPrimaryKey(x, Connection, attr.TableName);
                                    }
                                    else
                                    {
                                        this.ModifyTable(x, Connection, attr.TableName);
                                    }

                                    this.ModifyUniqueConstraints(x, Connection, attr.TableName);
                                    CacheTableSync[$"dbname={GetDataBase()};schema={schema};type={x.FullName};table={attr.TableName}"] = x;
                                }
                            }

                        }
                    }
                    foreach (var item in lst)
                    {
                        this.CreateForeignKey(Connection, item.FromTable, item.ForeignTable, item.Fields);
                    }
                }

            }
            catch(Npgsql.PostgresException ex)
            {
                if(ex.SqlState == "42P07")
                {
                    return;
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        

        public void CreateForeignKey(object Connection, string FromTable, string ForeignTable, DbForeignKeyInfoFields[] Fields)
        {
            try
            {
                var cnn = (NpgsqlConnection)Connection;
                var dbSchema = string.IsNullOrEmpty(this.schema) ? "public" : this.schema;
                /*
                 * ALTER TABLE public.emps
                       ADD CONSTRAINT "FK_CODE" FOREIGN KEY
                              ("Code", "Code")
                              REFERENCES public."Departments2" ("Code", "DepId")
                                  ON DELETE RESTRICT
                                    ON UPDATE CASCADE
                              NOT DEFERRABLE INITIALLY IMMEDIATE
                 * **/
                var constraintName = $"fk_{dbSchema}_{FromTable}_{ForeignTable}";
                var sqlCheckIsExistForeignKey = $"SELECT count(*) FROM information_schema.table_constraints " +
                    $"WHERE constraint_name='{constraintName}' " +
                    $"and table_schema='{dbSchema}'";
                if (this.ExecCommandAndGetValue<int>(cnn, sqlCheckIsExistForeignKey) == 0)
                {
                    var localFields = string.Join(',', Fields.Select(p => $"\"{p.LocalField}\""));
                    var foreignFields = string.Join(',', Fields.Select(p => $"\"{p.ForeignField}\""));
                    var sqlCommandCreate = $"ALTER TABLE {dbSchema}.\"{FromTable}\" " +
                        $"ADD CONSTRAINT \"{constraintName}\" FOREIGN KEY " +
                        $"({localFields}) " +
                        $"REFERENCES {dbSchema}.\"{ForeignTable}\" ({foreignFields}) " +
                        $"  ON DELETE RESTRICT " +
                        $"  ON UPDATE CASCADE" +
                        $" DEFERRABLE INITIALLY DEFERRED;";
                    var cmd = cnn.CreateCommand();
                    cmd.CommandText = sqlCommandCreate;
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
               
            }
            
            
        }

        private void CreateUniqueConstraint(object Connection,string table_name,string cols)
        {
            var cnn = (Npgsql.NpgsqlConnection)Connection;
            var dbSchema = string.IsNullOrEmpty(this.schema) ? "public" : this.schema;
            var constraintName = $"uk_{dbSchema}_{table_name}_{cols.Replace(",", "_")}".ToLower();
            var uk = string.Join(',', cols.Split(',').Select(p => $"\"{p}\""));
            var sqlCheckIsExistConstraintName = $"select count(*) " +
                $"from information_schema.constraint_column_usage " +
                $"where table_name = '{table_name}'  and " +
                $"table_schema = '{dbSchema}' and   " +
                $"constraint_name = '{constraintName}'";
            if (this.ExecCommandAndGetValue<int>(this.ConnectionString, sqlCheckIsExistConstraintName) == 0)
            {
                var sqlCreatePrimaryKeyContrainst = $"ALTER TABLE {dbSchema}.\"{table_name}\" ADD CONSTRAINT \"{constraintName}\" UNIQUE ({uk});";
                var cmd = cnn.CreateCommand();
                cmd.CommandText = sqlCreatePrimaryKeyContrainst;
               
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Npgsql.PostgresException ex)
                {
                    if (ex.SqlState == "42P16")
                    {
                        return;
                    }
                }
                
            }
        }
        public void ModifyUniqueConstraints(Type ModelType, object Connection, string table_name)
        {
            var dbSchema = string.IsNullOrEmpty(this.schema) ? "public" : this.schema;
            var ukAttr = ModelType.GetCustomAttribute<UniqueIndexAttribute>();
            if (ukAttr == null) return;
            foreach(var cols in ukAttr.Columns)
            {
                CreateUniqueConstraint(Connection, table_name, cols);
            }
        }

        public void ModifyPrimaryKey(Type ModelType, object Connection, string table_name)
        {
            var cnn = (NpgsqlConnection)Connection;
            /*
             * ALTER TABLE public."Departments2"
                    ADD CONSTRAINT "test" PRIMARY KEY ("Code")
             * **/
            var dbSchema = string.IsNullOrEmpty(this.schema) ? "public" : this.schema;
            var primaryKeyAttr = ModelType.GetCustomAttribute<PrimaryKeysAttribute>();
            if (primaryKeyAttr == null) return;
            var constraintName =$"pk_{dbSchema}_{table_name}_{primaryKeyAttr.Columns.Replace(",", "_")}".ToLower();
            var sqlCheckIsExistConstraintName = $"select count(*) " +
                $"from information_schema.constraint_column_usage " +
                $"where table_name = '{table_name}'  and " +
                $"table_schema = '{dbSchema}' and   " +
                $"constraint_name = '{constraintName}'";
            var pk = string.Join(',', primaryKeyAttr.Columns.Split(',').Select(p => $"\"{p}\""));
            if (this.ExecCommandAndGetValue<int>(cnn, sqlCheckIsExistConstraintName) == 0)
            {
                var sqlCreatePrimaryKeyContrainst = $"ALTER TABLE {dbSchema}.\"{table_name}\" ADD CONSTRAINT \"{constraintName}\" PRIMARY KEY({pk});";
                var cmd = cnn.CreateCommand();
                cmd.CommandText = sqlCreatePrimaryKeyContrainst;
                try
                {
                    cmd.ExecuteNonQuery();
                   
                }
                catch (Npgsql.PostgresException ex)
                {
                    if (ex.SqlState == "42P16")
                    {
                        return;
                    }
                    
                    
                }
                
            }
        }

        public void ModifyAutoIncrement(Type ModelType, object Connection, string table_name)
        {
            var dbSchema = string.IsNullOrEmpty(this.schema) ? "public" : this.schema;
            var autoIncProperties = ModelType.GetProperties().Where(
                p => p.GetCustomAttribute<LV.Db.Common.DataTableColumnAttribute>() != null &&
                p.GetCustomAttribute<DataTableColumnAttribute>().AutoInc).ToList();
            if (autoIncProperties.Count == 0) return;
            /*
             * CREATE SEQUENCE public."test" INCREMENT BY 1
                              MINVALUE 1
                              NO MAXVALUE
                              START WITH 1
                              NO CYCLE
             * **/
            var cnn = (Npgsql.NpgsqlConnection)Connection;
            foreach (var P in autoIncProperties)
            {
                var seqName = $"seq_{dbSchema}_{P.Name}_{table_name}".ToLower();
                var sqlCommandCheckIsExistSeq = "SELECT count(*) FROM pg_class WHERE relkind = 'S' AND relname = {0}";
                var sqlCommandAddSeqTotable = $"ALTER TABLE \"{dbSchema}\".\"{table_name}\"  ALTER COLUMN \"{P.Name}\" SET DEFAULT nextval('{dbSchema}.{seqName}'::regclass)";
                if (this.ExecCommandAndGetValue<int>(cnn, sqlCommandCheckIsExistSeq, seqName) == 0)
                {
                    var sqlCommandCreateSeq = $"CREATE SEQUENCE \"{dbSchema}\".\"{seqName}\" INCREMENT BY 1  MINVALUE 1 NO MAXVALUE START WITH 1 NO CYCLE";
                    
                    var cmd = cnn.CreateCommand();
                    cmd.CommandText = sqlCommandCreateSeq;
                    cmd.ExecuteNonQuery();

                }
                var sqlCommandTextCheckIsSeqAssignToColl = $"SELECT count(*) from information_schema.columns " +
                    $"where table_name = '{table_name}' and " +
                    $"table_schema = '{dbSchema}' and " +
                    $"column_default IS NULL and column_name='{P.Name}'";
                if (this.ExecCommandAndGetValue<int>(cnn, sqlCommandTextCheckIsSeqAssignToColl) > 0)
                {
                    var cmd = cnn.CreateCommand();
                    cmd.CommandText = sqlCommandAddSeqTotable;
                    cmd.ExecuteNonQuery();
                }
            }

        }
        internal static bool IsPrimitiveType(Type declaringType)
        {
            if (CheckIfPrimitiveTypeCache == null)
            {
                CheckIfPrimitiveTypeCache = new Hashtable();
            }
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
            return (bool)(CheckIfPrimitiveTypeCache[declaringType.FullName]);
        }
        public void ModifyTable(Type ModelType,object Connection, string table_name)
        {
            var cnn = (NpgsqlConnection)Connection;
            var schema = string.IsNullOrEmpty(this.schema) ? "public" : this.schema;
            var sql = $"SELECT \"column_name\" " +
                $"FROM information_schema.columns " +
                $"WHERE table_schema = '{schema}' AND table_name = '{table_name}'";
            var lst = this.ExecSqlToList<string>(cnn, sql);
            var properties = ModelType.GetProperties().Where(p => !lst.Any(x => x == p.Name)).Where(p => IsPrimitiveType(p.PropertyType)).ToList();
            var sqlCommandText = $"ALTER TABLE {schema}.\"{table_name}\"";
            //$" ADD COLUMN \"aaaa\" CHARACTER VARYING(20), ADD COLUMN \"FX\" CHARACTER VARYING(20)";
            if (properties.Count == 0)
            {
                return;
            }
            foreach (var pro in properties)
            {
                sqlCommandText += "ADD " + GetSQlCreateColumn(pro) + ",";
            }
            sqlCommandText = sqlCommandText.TrimEnd(',') + ";";
            var cmd = cnn.CreateCommand();
            cmd.CommandText = sqlCommandText;
            cmd.ExecuteNonQuery();

        }

        public void CreateTableIfNotExist(Type ModelType,object Connection, string table_name)
        {
            var cnn = (NpgsqlConnection)Connection;
            var sqlCommanfText = GetCreateTableCommand(ModelType,table_name);
            var cmd = cnn.CreateCommand();
            cmd.CommandText = sqlCommanfText;
            cmd.ExecuteNonQuery();
        }

        public string GetCreateTableCommand(Type ModelType, string table_name)
        {
            var schema = string.IsNullOrEmpty(this.schema) ? "public" : this.schema;
            var ret = $"CREATE TABLE {schema}.\"{table_name}\"";
            var txtCols = "";
            foreach (var pro in ModelType.GetProperties().Where(p=>IsPrimitiveType(p.PropertyType)))
            {
                txtCols += GetSQlCreateColumn(pro) + ",";

            }
            txtCols = txtCols.TrimEnd(',');
            ret += "(";
            ret += txtCols;
            ret += ")";
            return ret;
        }

        public string GetSQlCreateColumn(PropertyInfo pro)
        {
            var colAttr = pro.GetCustomAttribute<DataTableColumnAttribute>();
            var pType = pro.PropertyType;
            var uType = Nullable.GetUnderlyingType(pType);
            var isAllowNull = uType != null;
            if (uType != null)
            {
                pType = uType;
            }
            var strDbType = GetRealDbType(pType);
            if (colAttr == null)
            {
                if (pType == typeof(string))
                {
                    return $"\"{pro.Name}\" {strDbType}";
                }
                else
                {
                    if (!isAllowNull)
                    {
                        return $"\"{pro.Name}\" {strDbType} NOT NULL";
                    }
                    else
                    {
                        return $"\"{pro.Name}\" {strDbType}";
                    }
                }
            }
            else
            {
                if (pType == typeof(string))
                {
                    if (colAttr.IsRequire)
                    {
                        if (colAttr.Len > 0)
                        {
                            return $"\"{pro.Name}\" {GetRealDbType(pro.PropertyType)} ({colAttr.Len}) NOT NULL";
                        }
                        else
                        {
                            return $"\"{pro.Name}\" {GetRealDbType(pro.PropertyType)} (10485760) NOT NULL";
                        }
                    }
                    else
                    {
                        if (colAttr.Len > 0)
                        {
                            return $"\"{pro.Name}\" {strDbType} ({colAttr.Len})";
                        }
                        else
                        {
                            return $"\"{pro.Name}\" {strDbType} (10485760)";
                        }
                    }
                }
                else
                {
                    if (!isAllowNull)
                    {
                        return $"\"{pro.Name}\" {strDbType} NOT NULL";
                    }
                    else
                    {
                        return $"\"{pro.Name}\" {strDbType}";
                    }
                }
            }
        }

        public string GetRealDbType(Type PropertyType)
        {
            /*
             * Postgresql  NpgsqlDbType System.DbType Enum .Net System Type
                ----------  ------------ ------------------ ----------------
                int8        Bigint       Int64              Int64
                bool        Boolean      Boolean            Boolean
                bytea       Bytea        Binary             Byte[]
                date        Date         Date               DateTime
                float8      Double       Double             Double
                int4        Integer      Int32              Int32
                money       Money        Decimal            Decimal
                numeric     Numeric      Decimal            Decimal
                float4      Real         Single             Single
                int2        Smallint     Int16              Int16
                text        Text         String             String
                time        Time         Time               DateTime
                timetz      Time         Time               DateTime
                timestamp   Timestamp    DateTime           DateTime
                timestamptz TimestampTZ  DateTime           DateTime
                interval    Interval     Object             TimeSpan
                varchar     Varchar      String             String
                inet        Inet         Object             IPAddress
                bit         Bit          Boolean            Boolean
                uuid        Uuid         Guid               Guid
                array       Array        Object             Array
             * **/






            if (PropertyType == typeof(Int64)) return "int8";
            if (PropertyType == typeof(Boolean)) return "bool";
            if (PropertyType == typeof(Byte[])) return "bytea";
            if (PropertyType == typeof(DateTime)) return "timestamptz";
            if (PropertyType == typeof(Double)) return "float8";
            if (PropertyType == typeof(Int32)) return "int4";
            if (PropertyType == typeof(Decimal)) return "numeric";
            if (PropertyType == typeof(Single)) return "float4";
            if (PropertyType == typeof(Int16)) return "int2";
            if (PropertyType == typeof(String)) return "varchar";


            throw new NotImplementedException();
        }

        public bool IsExisTable(object Connection, string table_name)
        {
            var sql = "SELECT count(*) FROM information_schema.tables WHERE  table_schema = {0} AND table_name = {1}";
            if (string.IsNullOrEmpty(this.schema))
            {
                return this.ExecCommandAndGetValue<int>((IDbConnection) Connection, sql, "public", table_name) > 0;
            }
            else
            {
                return this.ExecCommandAndGetValue<int>((IDbConnection)Connection, sql, this.schema, table_name) > 0;
            }
        }

        public object GetConnection()
        {
            
            return this.Connection;
        }

        public object CreateDbConnectionWithDbName(string dbName)
        {
            var x = this.cnnInfo;
            throw new NotImplementedException();
        }

        public DbTypes GetDbType()
        {
            return DbTypes.PostgreSQL;
        }

        public void SetTransaction(IDbTransaction transaction)
        {
            this.tran = transaction as NpgsqlTransaction;
        }
        public IDbTransaction GetTransaction()
        {
            return this.tran as IDbTransaction;
        }

        public IDbConnection CreateNewConnectionFromCurrentConnectionString()
        {
            return new Npgsql.NpgsqlConnection(this.ConnectionString);
        }

        #endregion
    }
}
