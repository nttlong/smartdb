using System.Collections.Generic;

namespace LV.Db.Common
{
    internal class DbDesigner
    {
        public DbDesigner()
        {
        }

        public string DbName { get; internal set; }
        public string Schema { get; internal set; }
        public List<DbModelSyncItem> Models { get; internal set; }
    }
}