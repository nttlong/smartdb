using System.Collections.Generic;

namespace LV.Db.Common
{
    public class TableInfo
    {
        internal string name;
       

        public TableInfo()
        {
        }

        public string schema { get; internal set; }
        public List<ColumInfo> Columns { get; set; }
    }
}