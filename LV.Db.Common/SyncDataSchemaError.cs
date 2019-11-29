using System;

namespace LV.Db.Common
{
    public class SyncDataSchemaError
    {
        public Exception Exception { get; internal set; }
        public string DatabaseName { get; internal set; }
        public string DbSchema { get; internal set; }
        public string Server { get; internal set; }
        public string DbType { get; internal set; }
    }
}