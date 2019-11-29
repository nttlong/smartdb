using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LV.Db.Common
{
    /// <summary>
    /// this is a global variable where hold all global variables
    /// </summary>
    public static class Globals
    {
        public static string DBProviderType { get; private set; }

        /// <summary>
        /// Current ConnectionString, you can not change after init
        /// </summary>
        public static string ConnectionString { get;  set; }
        public static object Connection { get; private set; }

       

        public static void Context(object dbContext)
        {
            Globals.dbContext = dbContext;
            Globals.dbContextAssembly = dbContext.GetType().Assembly;
            if (dbContext.GetType().FullName == "LinqToDB.DataContext")
            {
#if NET_CORE
                var Provider = dbContext.GetType().GetProperty("DataProvider").GetValue(dbContext);
                Globals.DBProviderType = Provider.GetType().GetProperty("ConnectionNamespace").GetValue(Provider).ToString();
                Globals.ConnectionString = dbContext.GetType().GetProperty("ConnectionString").GetValue(dbContext).ToString();
#else
                var Provider = dbContext.GetType().GetProperty("DataProvider").GetValue(dbContext,new object[] { });
                Globals.DBProviderType = Provider.GetType().GetProperty("ConnectionNamespace").GetValue(Provider, new object[] { }).ToString();
                Globals.ConnectionString = dbContext.GetType().GetProperty("ConnectionString").GetValue(dbContext, new object[] { }).ToString();
#endif
                if (Globals.DBProviderType == "Npgsql")
                {
                    Settings.SetConnectionString(DbTypes.PostgreSQL, Globals.ConnectionString);
                }
                else if(Globals.DBProviderType== "System.Data.SqlClient")
                {
                    Settings.SetConnectionString(DbTypes.SqlServer, Globals.ConnectionString);
                }
                else
                {
                    throw new NotImplementedException();
                }
                
            }
            if (dbContext.GetType().FullName == "LinqToDB.Data.DataConnection")
            {
#if NET_CORE
                var Provider = dbContext.GetType().GetProperty("DataProvider").GetValue(dbContext);
                Globals.DBProviderType = Provider.GetType().GetProperty("ConnectionNamespace").GetValue(Provider).ToString();
                Globals.ConnectionString = dbContext.GetType().GetProperty("ConnectionString").GetValue(dbContext).ToString();
                Globals.Connection = Globals.dbContext.GetType().GetProperty("Connection").GetValue(Globals.dbContext);
#else
                var Provider = dbContext.GetType().GetProperty("DataProvider").GetValue(dbContext,new object[] { });
                Globals.DBProviderType = Provider.GetType().GetProperty("ConnectionNamespace").GetValue(Provider, new object[] { }).ToString();
                Globals.ConnectionString = dbContext.GetType().GetProperty("ConnectionString").GetValue(dbContext, new object[] { }).ToString();
                Globals.Connection = Globals.dbContext.GetType().GetProperty("Connection").GetValue(Globals.dbContext, new object[] { });
#endif
                if (Globals.DBProviderType == "Npgsql")
                {
                    Settings.SetConnectionString(DbTypes.PostgreSQL, Globals.ConnectionString);
                }
                else if (Globals.DBProviderType == "System.Data.SqlClient")
                {
                    Settings.SetConnectionString(DbTypes.SqlServer, Globals.ConnectionString);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        


        /// <summary>
        /// Current db type
        /// </summary>
        public static DbTypes DbType { get; set; }
        /// <summary>
        /// The type of provider where Database accessing is impletemented
        /// </summary>
#if NET_CORE
        public static TypeInfo ProviderType { get; internal set; }
#else
        public static Type ProviderType { get; internal set; }
#endif
        /// <summary>
        /// The Assembly of provider where Database accessing is impletemented
        /// </summary>
        public static Assembly ProviderAssembly { get; internal set; }
        ///// <summary>
        ///// This is switcher before SQL statement was compiler
        ///// Exmaple:
        ///// PostgreSQL is select "my table".* from "my table"
        ///// MySQL is select `my table`.* from `my table`
        ///// For the new type of RDBMS you should impletment this Compiler
        ///// </summary>
        //public static ICompiler Compiler { get; internal set; }
        public static Hashtable TableInfo { get; set; }
        public static object Provider { get; internal set; }
        public static object dbContext { get; private set; }
        public static Assembly dbContextAssembly { get; private set; }
        public static Designer Design
        {
            get
            {
                if (_designer == null)
                {
                    _designer = new Designer();
                }
                return _designer;
            }
        }

        public static Hashtable TableSyncCache = new Hashtable();
        public static object objLockTableSyncCache = new object();

        public static Hashtable ProviderTypes = new Hashtable();
        private static Designer _designer;
        public static SysnMode Mode { get; set; }
        public static Action<SyncDataSchemaError> OnError { get; set; }
    }
    public enum SysnMode
    {
        OnDemand,
        OnNewDbContext,
        Manual
    }
}