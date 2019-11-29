using LV.Db.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LV.Db.SQLServer
{
    public class Database : IDb
    {
        private SqlConnection cnn;
        private string schema;
        private SqlTransaction tran;
        public Database()
        {
            
        }

        public SqlConnection Connection
        {
            get
            {
                if (cnn == null) cnn = new SqlConnection(this.ConnectionString);
                return cnn;
            }
            set{
                cnn = value;
            }
        }
        [System.Diagnostics.DebuggerStepThrough]
        private void OpenConnect()
        {
            if (this.Connection.State != ConnectionState.Open)
            {
                this.Connection.Open();
            }
        }
        public void BeginTran(DbIsolationLevel level)
        {
            this.OpenConnect();

            if (tran==null)
                tran = this.Connection.BeginTransaction();
            else
            {
                throw new Exception("Transaction is use");
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
                tran.Rollback();
            tran = null;
        }

        public string ConnectionString { get; private set; }

        public void CloseConnection()
        {
            this.RollBack();
            this.Connection.Close();
            this.Connection.Dispose();
        }
        public void SetConnectionString(string connectionString)
        {
            this.ConnectionString = connectionString;
        }
        public void SetSchema(string schema)
        {
            this.schema = schema;
        }
        public string GetSchema()
        {
            return this.schema;
        }
       

       

        #region Implemmetation of DML
        

        

       

        

        

        public string GetConnectionString()
        {
            return this.ConnectionString;
        }

        public TableInfo GetTableInfo(string ConnectionString, string schema, string table_name)
        {
            TableInfo table = new TableInfo() { Columns=new List<ColumInfo>()};
            using(var cn = new SqlConnection(ConnectionString))
            using (SqlCommand command = cn.CreateCommand())
            {
                if (string.IsNullOrEmpty(schema))
                    command.CommandText = String.Format("SELECT TOP 0 * FROM [{0}]", table_name);
                else
                    command.CommandText = String.Format("SELECT TOP 0 * FROM [{0}].[{1}]", schema, table_name);
                command.CommandType = CommandType.Text;

                cn.Open();

                SqlDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);

                DataTable schemaTable = reader.GetSchemaTable();
                foreach (DataRow row in schemaTable.Rows)
                {
                    table.Columns.Add(new ColumInfo()
                    {
                        Name = row["ColumnName"] as string,
                        AllowDBNull = row["AllowDBNull"] == DBNull.Value ? false : (bool)row["AllowDBNull"],
                        ColumnSize = (int)row["ColumnSize"],
                        DataType = row["DataType"],
                        IsAutoIncrement = row["IsAutoIncrement"] == DBNull.Value ? false : (bool)row["IsAutoIncrement"],
                        IsExpression = row["IsExpression"] == DBNull.Value ? false : (bool)row["IsExpression"],
                        IsIdentity = row["IsIdentity"] == DBNull.Value ? false : (bool)row["IsIdentity"],
                        IsKey = row["IsKey"] == DBNull.Value ? false : (bool)row["IsKey"],
                        IsReadOnly = row["IsReadOnly"] == DBNull.Value ? false : (bool)row["IsReadOnly"],
                        IsUnique = row["IsUnique"] == DBNull.Value ? false : (bool)row["IsUnique"],
                        TableName=table_name
                    });
                    
                }
                reader.Close();
                cn.Close();
            }
            return table;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public DataTable ExecToDataTable(string SQL, params ParamConst[] pars)
        {
            using (var command = new SqlCommand(SQL, this.Connection))
            {
                this.OpenConnect();
                command.Transaction = this.tran;
                var paramIndex = 0;
                foreach (var p in pars)
                {
                    var paramName = string.Format("@p{0}", paramIndex);
                    command.CommandText = command.CommandText.Replace("{" + paramIndex.ToString() + "}", paramName);
                    var sqlParam = new SqlParameter();
                    sqlParam.ParameterName = paramName;
                    if (p != null)
                    {
                        sqlParam.Value = p.Value;
                    }
                    else
                    {
                        sqlParam.Value = DBNull.Value;
                    }
                    command.Parameters.Add(sqlParam);
                    paramIndex++;

                }
                var adp = new SqlDataAdapter(command);
                var tbl = new DataTable();
                adp.Fill(tbl);
                return tbl;
            }
        }
        [System.Diagnostics.DebuggerStepThrough]
        public IList<T> ExecToList<T>(string SQL, params ParamConst[] pars)
        {
            var ret = new List<T>();
            var dataTable = this.ExecToDataTable(SQL, pars);

            return DataLoader.ToList<T>(dataTable);
        }

        public DataActionResult InsertData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<ParamConst> InputParams)
        {
            return this.ExecCommand(SqlCommandText, false, InputParams.ToArray());
        }

        public DataActionResult DeleteData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<ParamConst> InputParams)
        {
            return this.ExecCommand(SqlCommandText, false, InputParams.ToArray());
        }

        public DataActionResult UpdateData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<ParamConst> InputParams)
        {
            return this.ExecCommand(SqlCommandText, false, InputParams.ToArray());
        }
        [System.Diagnostics.DebuggerStepThrough]
        public DataActionResult ExecCommand(string sql, bool returnError, ParamConst[] Params)
        {
            var v = Params;
            var ret = new DataActionResult()
            {
                Data=v
            };
            try
            {
                using (SqlCommand cm = this.Connection.CreateCommand())
                {
                    this.OpenConnect();
                    cm.Transaction = this.tran;
                    for (int i = 0; i < v.Length; i++)
                    {
                        sql = sql.Replace("{" + i.ToString() + "}", "@" + i.ToString());
                        cm.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = "@" + i.ToString(),
                            Value = (v[i].Value == null) ? DBNull.Value : v[i].Value
                        });

                    }
                    cm.CommandText = sql;
                    using (SqlDataAdapter da = new SqlDataAdapter(cm))
                    {
                        DataTable tbl = new DataTable();
                        da.Fill(tbl);
                        if (tbl.Rows.Count > 0)
                        {
                            ret.NewID = new Hashtable();
                            foreach (DataColumn col in tbl.Columns)
                            {
                                ret.NewID[col.ColumnName] = tbl.Rows[0][col];
                            }
                        }
                    }


                }
            }
            catch (SqlException exp)
            {

                if (exp.Number == 2627)
                {
                    ret.Error = new ErrorAction()
                    {
                        ErrorType = DataActionErrorTypeEnum.DuplicateData
                    };
                }
                else if (exp.Number == 547)
                {

                    ret.Error = new ErrorAction()
                    {
                        ErrorType = DataActionErrorTypeEnum.ForeignKey

                    };
                    try
                    {
                        var sp = exp.Message.Split('\"');
                        ret.Error.RefTables = new string[] { sp[5] };
                        ret.Error.Fields = new string[] { sp[6].Split("'".ToCharArray()[0])[1] };

                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }

                if (returnError && ret.Error != null)
                    return ret;
                throw exp;
            }
            catch (Exception exp)
            {
                throw exp;
            }
            return ret;
        }

        public List<DataObjectInfo> GetAllTablesInfo(string schema)
        {
            throw new NotImplementedException();
        }

        public bool CreateSchemaIfNotExist(string schema)
        {
            throw new NotImplementedException();
        }

        public bool IsExistDbSchema(string schema)
        {
            throw new NotImplementedException();
        }

        public bool CreateTableIfNotExist(string schema, string tableName, DbColumnInfo[] cols)
        {
            throw new NotImplementedException();
        }

        public List<PrimaryKeyInfo> GetAllPrimaryKeys(string schema)
        {
            throw new NotImplementedException();
        }

        public bool CreatePrimaryKey(string schema, string tableName, string[] cols)
        {
            throw new NotImplementedException();
        }

        public List<SeqInfo> GetAllSeqs(string schema)
        {
            throw new NotImplementedException();
        }

        public bool CreateSeqIfNotExist(string schema, string name, int start, int increment)
        {
            throw new NotImplementedException();
        }

        public bool ApplySeq(string schema, string tableName, string columnName, string seqName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
