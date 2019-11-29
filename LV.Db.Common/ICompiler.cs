using System.Collections.Generic;
using System.Data;

namespace LV.Db.Common
{
    public enum ScopeTempTable
    {
        InSession=0,
        Global=1,
        Physical=2
    }
    /// <summary>
    /// The database engine compiler switcher
    /// Must be impletmented in each Provider
    /// </summary>
    public interface ICompiler
    {
        /// <summary>
        /// Combine table and field
        /// Example:
        ///     PosrtgreSQl is "my table name"."my field name"
        ///     MS SQL server is [my table name].[my field name]
        ///     MySQL is `my table name`.`my field name`
        /// </summary>
        /// <param name="table_name"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetFieldName(string schema, string table_name, string name);
        /// <summary>
        /// Make an alias field
        /// Example:
        ///     PosrtgreSQl is "my table name"."my field name" as "my alias"
        ///     MS SQL server is [my table name].[my field name] as [my alias]
        ///     MySQL is `my table name`.`my field name` as `my alias`
        /// </summary>
        /// <param name="field"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        string MakeAlias(string field, string alias);
        //TableInfo GetTableInfo(string ConnectionString,string schema, string table_name);
        string MakeRowNumber(string SQL, List<SQLDataPageInfoSorting> sort);

        /// <summary>
        /// Combination of quotes and name
        /// Example:
        ///     PostgreSQl is "my name"
        ///     MS SQL server is [my name]
        ///     MySQL is `my name`
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetQName(string schema, string name);
        /// <summary>
        /// Get real sql engine built-in function
        /// Example:
        /// For the same function name like 'LEN'
        ///     PostgreSQL is LENGTH
        ///     MS SQL Server is LEN
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetSqlFunction(string name, SqlFunctionParamInfo[] sqlFunctionParamInfo);
        string CreateSQLMakeTable(SqlCommandInfo sqlCommandInfo);
        string CreateSQLUpdateString(SqlCommandInfo sqlUpdateInfo);

        /// <summary>
        /// Combination of table name and alias name
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        string CreateAliasTable(string tableName, string alias);
        void SetConnectionString(string connectionString);
        string GetQName(string tableName);

        string GetTempTable(ScopeTempTable scope, string schema, string tableName);
        string MakeLimitRow(string sql, List<SortingInfo> sortList, List<string> fields, int selectTop,int Skip);
        string CreateSQLGetIdentity(string schema, string table_name, TableInfo tbl);
        string CreateSQLDeleteString(SqlCommandInfo sqlUpdateInfo);
        DataTable ExecSQLAndReturnDatatable(object connection, string sql, params object[] Params);
        string GetConnectionString();
        string ConstVal(object value);
        int GetMaxTextLen();
    }
}