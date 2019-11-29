using LV.Db.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace LV.Db.PostgreSQL
{
    public class Compiler : ICompiler
    {
        const int MAX_VAR_CHAR_LEN = 10485761;
        private string connectionString;

        public string CreateAliasTable(string tableName, string alias)
        {
            return string.Format(@"{0} as {1}", tableName, alias);
        }

        

        public string CreateSQLGetIdentity(string schema, string table_name, TableInfo tbl)
        {
            var autoFields = tbl.Columns.Where(p => p.IsAutoIncrement).Select(p => p.Name);
            if (autoFields.Count() == 0)
            {
                return "";
            }
            var sqlReturn = "select " + string.Join(",", autoFields.Select(p => @"max(""" + p + @""") as """ + p + @"""").ToArray())+ " from "+this.GetQName(schema,table_name);
            return sqlReturn;
        }

       
        public string CreateSQLDeleteString(SqlCommandInfo sqlUpdateInfo)
        {
            /*
             * 
                WITH qr  AS (
		            select "r10"."ID",'XXXX' as "DepartmentName" 
		            from "hr"."Employees" as "l10" 
		            inner join "hr"."Departments" as "r10" on l10."DepartmentCode" = r10."DepartmentCode" 
		            inner join "hr"."Positions" as "r110" on "l10"."PositionCode" = r110."Code" 
		            inner join "hr"."Employees" as "r1110" on "l10"."IDCard" != r1110."IDCard"
	            ) 
	            DELETE   FROM "hr"."Departments" 
	            USING "qr" 
	            WHERE "qr"."ID"="hr"."Departments"."ID"
             * */

            var subQuery = "select " + string.Join(",", sqlUpdateInfo.KeyFields.Select(
                p => GetFieldName("", sqlUpdateInfo.OriginTableInfo.Alias, p)).ToArray()) + 
                " from " + sqlUpdateInfo.JoinClause;
            var ret = "WITH qr  AS (" + subQuery + ")"+
                " DELETE FROM " + GetQName(sqlUpdateInfo.OriginTableInfo.Schema, sqlUpdateInfo.OriginTableInfo.TableName)+
                " USING  " + GetQName("qr")+
                " WHERE " + string.Join(" and ",
                        sqlUpdateInfo.KeyFields.Select(p => GetFieldName("", "qr", p) +
                                                        "=" +
                                                            GetFieldName(sqlUpdateInfo.OriginTableInfo.Schema, sqlUpdateInfo.OriginTableInfo.TableName, p)
                                                            ).ToArray());

            return ret;
        }

        public string CreateSQLUpdateString(SqlCommandInfo sqlUpdateInfo)
        {
            /*
            WITH subquery AS (
                select "l10"."DepartmentCode"
		
		            from "hr"."Employees" as "l10" 
			             right join "hr"."Departments" as "r10" on l10."DepartmentCode" = r10."DepartmentCode" 
	            )
	            UPDATE "hr"."Employees"
	            SET "Salary" = 12345
	            from subquery
	            where subquery."DepartmentCode" = "hr"."Employees" ."DepartmentCode" 
             */
            var subQuery = "select " + string.Join(",", sqlUpdateInfo.KeyFields.Select(
                p => GetFieldName("", sqlUpdateInfo.OriginTableInfo.Alias, p)).ToArray()) + ","+
                string.Join(",", sqlUpdateInfo.Fields.Select(p=>p.Source+" as "+p.ToField).ToArray()) +
                " from " + sqlUpdateInfo.JoinClause;
            var ret = "WITH qr  AS (" + subQuery + ") UPDATE " + GetQName(sqlUpdateInfo.OriginTableInfo.Schema, sqlUpdateInfo.OriginTableInfo.TableName) +
                " SET " + string.Join(",", sqlUpdateInfo.Fields.Select(p=>p.ToField+"="+ "qr."+ p.ToField).ToArray()) +
                " FROM qr " +
                " WHERE " + string.Join(" and ", 
                        sqlUpdateInfo.KeyFields.Select(p => GetFieldName("", "qr", p) + 
                                                        "=" + 
                                                            GetFieldName(sqlUpdateInfo.OriginTableInfo.Schema, sqlUpdateInfo.OriginTableInfo.TableName, p)
                                                            ).ToArray());

            return ret;
        }

        public string GetFieldName(string schema, string table_name, string name)
        {
            if (string.IsNullOrEmpty(schema))
            {
                return string.Format(@"""{0}"".""{1}""", table_name, name);
            }
            else
            {
                return string.Format(@"""{0}"".""{1}"".""{2}""",schema, table_name, name);
            }
        }

       

        public string GetQName(string schema, string name)
        {
            if (string.IsNullOrEmpty(schema))
            {
                return @"""" + name + @"""";
            }
            else
            {
                return string.Format( @"""{0}"".""{1}""",schema,name);
            }
            
        }

        public string GetQName(string tableName)
        {
            return GetQName("", tableName);
        }

        public string GetSqlFunction(string name, SqlFunctionParamInfo[] sqlFunctionParamInfo)
        {
            if (name == "Len")
            {
                return "length({0})";
            }
            if (name == "Month")
            {
                return "EXTRACT(MONTH FROM {0})";
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
            if (name == "When")
            {
                return "when {0} then {1}";
            }
            if (name == "IsNull")
            {
                return string.Format("COALESCE({0},{1})",sqlFunctionParamInfo.Select(p=>p.Value).ToArray());
            }
            if (name == "Concat")
            {
                return "concat("+string.Join(",",sqlFunctionParamInfo.Select(p=>p.Value).ToArray())+")";
            }
            if (name == "Like")
            {
                return string.Format("{0} ILIKE {1}", sqlFunctionParamInfo.Select(p=>p.Value).ToArray());
            }
            if (name == "Cast")
            {
                return string.Format("Cast({0} as {1})",sqlFunctionParamInfo[0].Value,sqlFunctionParamInfo[1].ParamValue);
            }
            if (name == "Count")
            {
                return "Count({0})";
            }
            if (name == "Year")
            {
                return "date_part('year', timestamp {0})";
            }
            if (name == "In")
            {
                return string.Format("{0} in {1}", sqlFunctionParamInfo[0].Value, sqlFunctionParamInfo[1].ParamValue);
                //return sqlFunctionParamInfo[0].Value + " in {0}";
            }
            if (name == "Contains")
            {
                return string.Format("{0} ILIKE {1}", sqlFunctionParamInfo.Select(p => "Concat('%'," + ((p.Value == null) ? "Null" : p.Value) + ",'%')").ToArray());
            }
            if (name == "FirstContains")
            {
                return string.Format("{0} ILIKE {1}", sqlFunctionParamInfo.Select(p => "Concat(" + ((p.Value == null) ? "Null" : p.Value) + ",'%')").ToArray());
            }
            if (name == "LastContains")
            {
                return string.Format("{0} ILIKE {1}", sqlFunctionParamInfo.Select(p => "Concat('%'," + ((p.Value == null) ? "Null" : p.Value) + ")").ToArray());
            }
            throw new NotImplementedException();
        }

        public TableInfo GetTableInfo(string ConnectionString, string schema, string table_name)
        {
            return (new Database()).GetTableInfo(ConnectionString, schema, table_name);
        }

        public string GetTempTable(ScopeTempTable scope, string schema, string tableName)
        {
            var table = this.GetQName(schema, tableName);
           

            if (scope == ScopeTempTable.InSession)
            {
                return " TEMP TABLE " + table;
            }
            else
            {
                return " TEMPORARY  TABLE " + table;
            }
        }

        public string MakeAlias(string field, string alias)
        {
            return field + @" as """ + alias + @"""";
        }

        public string MakeLimitRow(string sql, List<SortingInfo> sortList, List<string> fields, int selectTop,int skipNumber)
        {
            if (selectTop >0)
            {
                sql = sql + " limit " + selectTop;
            }
            if (skipNumber >0)
            {
                sql = sql + " offset  " + skipNumber;
            }
            return sql;
        }

        public string CreateSQLMakeTable(SqlCommandInfo sqlCommandInfo)
        {
            if (!sqlCommandInfo.IsTempTable)
            {
                var sql = "Select " + string.Join(",", sqlCommandInfo.Fields.Select(p => p.Source + " as " + p.ToField).ToArray()) +
                    " into table " + GetQName(sqlCommandInfo.OriginTableInfo.Schema, sqlCommandInfo.OriginTableInfo.TableName) +
                    " from " + sqlCommandInfo.JoinClause;
                if (sqlCommandInfo.Filter != null)
                {
                    sql += " where " + sqlCommandInfo.Filter;
                }
                return sql;
            }
            else
            {
                var sql = "Select " + string.Join(",", sqlCommandInfo.Fields.Select(p => p.Source + " as " + p.ToField).ToArray()) +
                    " into temp table " + GetQName(sqlCommandInfo.OriginTableInfo.Schema, sqlCommandInfo.OriginTableInfo.TableName) +
                    " from " + sqlCommandInfo.JoinClause;
                if (sqlCommandInfo.Filter != null)
                {
                    sql += " where " + sqlCommandInfo.Filter;
                }
                return sql;
            }
        }
        [System.Diagnostics.DebuggerStepThrough]
        public DataTable ExecSQLAndReturnDatatable(object connection, string sql, params object[] Params)
        {
            var sqlConnection = connection as Npgsql.NpgsqlConnection;
            if (sqlConnection.State != ConnectionState.Open)
            {
                sqlConnection.Open();
            }
            var cmd = sqlConnection.CreateCommand();
            cmd.CommandText = sql;
            var paramIndex = 0;
            foreach(var p in Params)
            {
                cmd.CommandText = cmd.CommandText.Replace("{" + paramIndex + "}", ":p" + paramIndex);
                if (p == null)
                {
                    cmd.Parameters.Add(new Npgsql.NpgsqlParameter()
                    {
                        ParameterName="p"+paramIndex,
                        Value=DBNull.Value
                    });
                }
                else
                {
                    cmd.Parameters.Add(new Npgsql.NpgsqlParameter()
                    {
                        ParameterName = "p" + paramIndex,
                        Value = p
                    });
                }
                paramIndex++;
            }
            var adp = new Npgsql.NpgsqlDataAdapter(cmd);
            var retTbl = new DataTable();
            adp.Fill(retTbl);
            return retTbl;
        }

        public void SetConnectionString(string ConnectionString)
        {
            this.connectionString = ConnectionString;
        }

        public string GetConnectionString()
        {
            return this.connectionString;
        }

        public string MakeRowNumber(string SQl, List<SQLDataPageInfoSorting> Sort)
        {
            /*
              SELECT *, ROW_NUMBER () OVER (ORDER BY "UserId")
                FROM "SYS_Users"
             */
            var ret= "select *, ROW_NUMBER() OVER(ORDER BY " +
               string.Join(",", Sort.Select(p => (p.IsDesc) ? (GetFieldName(p.Schema,p.TableName, p.Field) + " desc") : (GetFieldName(p.Schema,p.TableName, p.Field) + " asc")).ToArray()) + ") "+
               " "+GetQName("RowNumber")+" "+
               "FROM (" + SQl + ") " + GetQName("SQL");
            return ret;
            //throw new NotImplementedException();
        }

        public string ConstVal(object value)
        {
            if (value is bool)
            {
                if ((bool)value == true)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            throw new NotImplementedException();
        }

        public bool IsExistTable(string ConnectionString, string DbName, string schema, string table_name)
        {
            throw new NotImplementedException();
        }

        public int GetMaxTextLen()
        {
            return MAX_VAR_CHAR_LEN;
        }
    }
}
