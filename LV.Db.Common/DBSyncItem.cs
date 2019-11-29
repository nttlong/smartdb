using System.Collections;

namespace LV.Db.Common
{
    public class DBSyncItem
    {
        public DBSyncItem()
        {
            this.Schema = new Hashtable();
        }
        public string DbName { get; set; }
        public Hashtable Schema { get; set; }
    }
}