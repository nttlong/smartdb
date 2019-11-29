using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LV.Db.Common
{
    public interface IDb
    {

        void CloseConnection();
        DataTable ExecToDataTable(string SQL, params ParamConst[] pars);
        IList<T> ExecToList<T>(string SQL, params ParamConst[] pars);
        IList<T> ExecToList<T>(IDbConnection Connection, string SQL, params ParamConst[] pars);
        DataActionResult InsertData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<ParamConst> InputParams);
        string GetServer();
        DataActionResult DeleteData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<ParamConst> InputParams);
        string GetPort();
        DataActionResult UpdateData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<ParamConst> InputParams);
        void CreateDbIfNotExists(string dbName);
        void SetConnectionString(string connectionString);
        void SetSchema(string schema);

        string GetSchema();

        void BeginTran(DbIsolationLevel level);
        void Commit();

        void RollBack();
        DataActionResult ExecCommand(string sql, bool returnError, ParamConst[] Params);
        string GetConnectionString();
        TableInfo GetTableInfo(string ConnectionString, string schema, string table_name);
        List<DataObjectInfo> GetAllTablesInfo(string schema);
        object CreateDbConnectionWithDbName(string dbName);
        bool CreateSchemaIfNotExist(object Connection, string schema);
        bool IsExistDbSchema( string schema);
        bool CreateTableIfNotExist(object Connection, string schema, string tableName, DbColumnInfo[] cols);
        List<PrimaryKeyInfo> GetAllPrimaryKeys(string schema);
        bool CreatePrimaryKey(object Connection, string schema, string tableName, string[] cols);
        List<SeqInfo> GetAllSeqs(string schema);
        bool CreateSeqIfNotExist(object Connection, string schema, string name, int start, int increment);
        bool ApplySeq(string schema, string tableName, string columnName, string seqName);
        void SetDataBaseName(string databaseName);
        string GetDataBase();
        T ExecCommandAndGetValue<T>(string ConnectionString, string Sql, params object[] Params);
        T ExecCommandAndGetValue<T>(IDbConnection Connection, string Sql, params object[] Params);
        T ExecCommandAndGetValue<T>(string Sql, params object[] Params);
        IList<T> ExecSqlToList<T>(string Sql, params object[] Params);
        IList<T> ExecSqlToList<T>(string ConnectionString, string Sql, params object[] Params);
        IList<T> ExecSqlToList<T>(IDbConnection Connection, string Sql, params object[] Params);
        void SyncTable(object Connection, Type ModelType, string table_name);
        void CreateTableIfNotExist(Type ModelType, object Connection, string table_name);
        string GetCreateTableCommand(Type ModelType, string table_name);
        bool IsExisTable(object Connection, string table_name);
        string GetRealDbType(Type PropertyType);
        string GetSQlCreateColumn(PropertyInfo pro);
        DbTypes GetDbType();
        void ModifyTable( Type ModelType,object Connection, string table_name);
        /// <summary>
        /// Check if propery of T has AutoInc
        /// Sync to Database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table_name"></param>
        void ModifyAutoIncrement(Type ModelType, object Connection, string table_name);
        /// <summary>
        /// Check and create primary key of table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table_name"></param>
        void ModifyPrimaryKey(Type ModelType, object Connection, string table_name);
        void ModifyUniqueConstraints(Type ModelType, object Connection, string table_name);
        void CreateForeignKey(object Connection, string FromTable, string ForeignTable, DbForeignKeyInfoFields[] Fields);
        /// <summary>
        /// Create empty schema
        /// </summary>
        /// <param name="schema"></param>
        void CreateDbSchema(string schema);
        object GetConnection();
        void SetTransaction(IDbTransaction transaction);
        IDbTransaction GetTransaction();
        IDbConnection CreateNewConnectionFromCurrentConnectionString();
    }
}