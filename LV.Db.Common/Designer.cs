using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LV.Db.Common
{
    public class Designer
    {

        static Hashtable Cache = new Hashtable();
        static Hashtable CacheCreateTables = new Hashtable();
        static Hashtable HashAssembly = new Hashtable();
        static object objLock = new object();
        private readonly object objLockCacheCreateTables = new object();

        internal Designer()
        {
            this.Models = new List<DbModelSyncItem>();
        }

        public List<DbModelSyncItem> Models { get; private set; }

        public Designer AddModel<T>()
        {
            //typeof(T).Assembly.CreateInstance(typeof(T).FullName);
            if (HashAssembly[typeof(T).Assembly.FullName] == null)
            {
                var modelTypes = typeof(T).Assembly.GetExportedTypes().Where(p => p.GetCustomAttributes(false).Any(x => x is QueryDataTableAttribute)).ToList();
                foreach (var modelType in modelTypes)
                {
                    this.Models.Add(new DbModelSyncItem
                    {
                        ModelType = modelType,
                        Databases = new Dictionary<string, DBSyncItem>()
                    });
                }
                HashAssembly[typeof(T).Assembly.FullName] = typeof(T).Assembly;
            }
            
            return this;
        }

        public void BuildTables(DbTypes DbType, string ConnectionString, string DbName, string Schema)
        {

            var key = $"db={DbName};schema={Schema}";
            if (Cache[key] == null)
            {
                lock (objLock)
                {
                    Cache[key] = true;
                    if (Cache[key] is bool)
                    {
                        using (var db = new DBContext(DbType, ConnectionString, true))
                        {
                            if (((IDbConnection)db.GetConnection()).State != ConnectionState.Open)
                            {
                                ((IDbConnection)db.GetConnection()).Open();
                            }
                            db.CreateDbIfNotExists(DbName);
                            db.CreateSchemaIfNotExist(db.GetConnection(), Schema);
                            foreach (var m in this.Models)
                            {
                                if (!m.Databases.ContainsKey(DbName))
                                {
                                    m.Databases.Add(DbName, new DBSyncItem { });
                                }
                            }
                        }
                    }
                }
            }
            var items = this.Models.Where
                (p =>

                p.Databases.Any(x => x.Key == DbName && x.Value.Schema[Schema] == null)).SelectMany(p => p.Databases.Select(x => new
                {
                    p.ModelType,
                    DbName = x.Key,
                    x.Value
                })).ToList();
            if (items.Count() == 0) return;


            var Lockkey = $"db={DbName};schema={Schema}";
            if (CacheCreateTables[Lockkey] == null)
            {
                lock (objLockCacheCreateTables)
                {
                    CacheCreateTables[Lockkey] = false;
                    if (CacheCreateTables[Lockkey] is bool)
                    {
                        if (CacheCreateTables[Lockkey] is bool)
                        {

                            using (var db = new DBContext(DbType, ConnectionString, true))
                            {
                                db.db.SetDataBaseName(DbName);
                                db.db.SetSchema(Schema);
                                if (((IDbConnection)db.GetConnection()).State != ConnectionState.Open)
                                {
                                    ((IDbConnection)db.GetConnection()).Open();
                                }
                                foreach (var item in items)
                                {
                                    db.BuildTable(item.ModelType);
                                    item.Value.Schema[Schema] = Schema;
                                }
                            }
                            CacheCreateTables[Lockkey] = Lockkey;
                        }
                    }
                }
            }

     

            //throw new NotImplementedException();
        }

        public void BuildTables(IDb db, string DatabaseName, string Schema)
        {
            this.BuildTables(db.GetDbType(), db.GetConnectionString(), DatabaseName, Schema);
        }
    }
}