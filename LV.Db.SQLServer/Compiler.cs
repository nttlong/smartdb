using LV.Db.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace LV.Db.SQLServer
{
    public class Compiler : ICompiler
    {
        private string connectionString;

        public string CreateAliasTable(string tableName, string alias)
        {
            return string.Format(@"{0} as {1}", tableName, alias);
        }

        public string CreateSQLDeleteString(SqlCommandInfo sqlUpdateInfo)
        {
            string sql = string.Format(@"delete {0} from {1}",
                sqlUpdateInfo.Source,
                sqlUpdateInfo.JoinClause
                );
            if (!string.IsNullOrEmpty(sqlUpdateInfo.Filter))
            {
                sql += " Where " + sqlUpdateInfo.Filter;
            }
            return sql;
        }

        public string CreateSQLGetIdentity(string schema, string table_name, TableInfo tbl)
        {
            var autoFields = tbl.Columns.Where(p => p.IsAutoIncrement).Select(p => p.Name);
            if (autoFields.Count() == 0)
            {
                return "";
            }
            return "SELECT @@IDENTITY as ["+ autoFields.FirstOrDefault()+"]";
        }

        

        public string CreateSQLMakeTable(SqlCommandInfo sqlCommandInfo)
        {
            string sql = string.Format(@"select {0} into {1} from {2}",
               string.Join(",", sqlCommandInfo.Fields.Select(o => o.Source)),
               sqlCommandInfo.OriginTableInfo.TableName,
               sqlCommandInfo.JoinClause
               );
            if (!string.IsNullOrEmpty(sqlCommandInfo.Filter))
            {
                sql += " Where " + sqlCommandInfo.Filter;
            }
            return sql;
        }

        public string CreateSQLUpdateString(SqlCommandInfo sqlUpdateInfo)
        {
            string sql = string.Format( @"update {0} set {1} from {2}",
                GetQName("", sqlUpdateInfo.OriginTableInfo.Alias),
                string.Join(",", sqlUpdateInfo.Fields.Select(o=>o.ToField + "=" + o.Source)),
                sqlUpdateInfo.JoinClause
                );
            if  (!string.IsNullOrEmpty(sqlUpdateInfo.Filter))
            {
                sql += " Where " + sqlUpdateInfo.Filter;
            }
            return sql;
        }

        public DataTable ExecSQLAndReturnDatatable(object connection, string sql, params object[] Params)
        {
            var sqlConnection = connection as SqlConnection;
            if (sqlConnection.State != ConnectionState.Open)
            {
                sqlConnection.Open();
            }
            var cmd = sqlConnection.CreateCommand();
            cmd.CommandText = sql;
            var paramIndex = 0;
            foreach (var p in Params)
            {
                cmd.CommandText = cmd.CommandText.Replace("{" + paramIndex + "}", ":p" + paramIndex);
                if (p == null)
                {
                    cmd.Parameters.Add(new SqlParameter
                    {
                        ParameterName = "p" + paramIndex,
                        Value = DBNull.Value
                    });
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "p" + paramIndex,
                        Value = p
                    });
                }
                paramIndex++;
            }
            var adp = new SqlDataAdapter(cmd);
            var retTbl = new DataTable();
            adp.Fill(retTbl);
            return retTbl;

        }

        public string GetFieldName(string schema, string table_name, string name)
        {
            if (string.IsNullOrEmpty(schema))
            {
                return string.Format(@"[{0}].[{1}]", table_name, name);
            }
            else
            {
                return string.Format(@"[{0}].[{1}].[{2}]",schema, table_name, name);
            }
            
        }

        

        public string GetQName(string schema, string name)
        {
            if (string.IsNullOrEmpty(schema))
            {
                return @"[" + name + @"]";
            }
            else
            {
                return string.Format(@"[{0}].[{1}]", schema, name);
            }
        }

        public string GetQName(string tableName)
        {
            return @"[" + tableName + @"]";
        }

        public string GetSqlFunction(string name, SqlFunctionParamInfo[] sqlFunctionParamInfo)
        {
            if(name == "Len")
            {

                return "Len({0})";
            }
            if (name == "Month")
            {
                return "Month({0})";
            }
            if (name == "Year")
            {
                return "year({0})";
            }
            if (name == "Day")
            {
                return "day({0})";
            }
            if (name == "Sum")
            {
                return "sum({0})";
            }
            if (name == "Min")
            {
                return "min({0})";
            }
            if (name == "Max")
            {
                return "max({0})";
            }
            if(name== "IsNull")
            {
                return "IsNull({0},{1})";
            }
            if (name == "Concat")
            {
                return string.Join("+", sqlFunctionParamInfo.Select(p =>"cast("+ p.Value+" as nvarchar(MAX))"));
                //return "dbo.concat(" + string.Join(",", sqlFunctionParamInfo.Select(p => p.Value).ToArray()) + ")";
            }
            if (name == "Like")
            {
                return string.Format("{0} LIKE {1}", sqlFunctionParamInfo.Select(p => p.Value).ToArray());
            }
            if (name == "Cast")
            {
                return string.Format("Cast({0} as {1})", sqlFunctionParamInfo[0].Value, sqlFunctionParamInfo[1].ParamValue);
            }
            if (name == "Count")
            {
                return "Count({0})";
            }
            if (name == "When")
            {
                return "when {0} then {1}";
            }
            if (name == "Contains")
            {
                return string.Format("{0} LIKE {1}", sqlFunctionParamInfo.Select(p => "(N'%'+" + ((p.Value == null) ? "N''" : "cast(" + p.Value + " as nvarchar(MAX))") + "+N'%')").ToArray());
            }
            if (name == "FirstContains")
            {
                return string.Format("{0} LIKE {1}", sqlFunctionParamInfo.Select(p => "(" + ((p.Value == null) ? "''" : "cast(" + p.Value + " as nvarchar(MAX))") + "+N'%')").ToArray());
            }
            if (name == "LastContains")
            {
                return string.Format("{0} LIKE {1}", sqlFunctionParamInfo.Select(p => "(N'%'+" + ((p.Value == null) ? "''" : "cast(" + p.Value + " as nvarchar(MAX))") + ")").ToArray());
            }
            
            throw new NotImplementedException();
        }

        public TableInfo GetTableInfo(string ConnectionString, string schema, string table_name)
        {
            return (new Database()).GetTableInfo(ConnectionString, schema, table_name);
        }

        public string GetTempTable(ScopeTempTable scope, string schema, string tableName)
        {
            var table = "";
            if (scope == ScopeTempTable.InSession)
            {
                table= "#" + tableName;
            }
            else
            {
                table= "##" + tableName;
            }
            return this.GetQName(schema, table);
        }

        public string MakeAlias(string field, string alias)
        {
            return field + @" as """ + alias + @"""";
        }

        public string MakeLimitRow(string sql, List<SortingInfo> sortList, List<string> fields, int selectTop,int skipNumber)
        {
            if (sql.Length > 6)
                return "select top " + selectTop.ToString() + " " + sql.Substring(6);
            return sql;
        }
        public void SetConnectionString(string ConnectionString)
        {
            this.connectionString = ConnectionString;
        }

        public string GetConnectionString()
        {
            return this.connectionString;
        }

        public string MakeRowNumber(string SQL, List<SQLDataPageInfoSorting> Sort)
        {
            /*
              SELECT *, ROW_NUMBER () OVER (ORDER BY "UserId")
                FROM "SYS_Users"
             */
            var ret = "select *, ROW_NUMBER() OVER(ORDER BY " +
               string.Join(",", Sort.Select(p => (p.IsDesc) ? (GetFieldName(p.Schema, p.TableName, p.Field) + " desc") : (GetFieldName(p.Schema, p.TableName, p.Field) + " asc")).ToArray()) + ") " +
               " " + GetQName("RowNumber") + " " +
               "FROM (" + SQL + ") " + GetQName("SQL");
            return ret;
            //throw new NotImplementedException();
        }

        public string ConstVal(object value)
        {
            if(value is bool)
            {
                if ((bool)value == true)
                {
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            throw new NotImplementedException();
        }
    }
}
