using System;

namespace LV.Db.Common
{
    public class DataTableColumnAttribute : Attribute
    {
        public bool IsRequire { get; set; }
        public int Len { get; set; }
        public bool AutoInc { get; set; }

        public DataTableColumnAttribute()
        {

        }
        public DataTableColumnAttribute(bool IsRequire,int Len)
        {
            this.IsRequire = IsRequire;
            this.Len = Len;
        }
        public DataTableColumnAttribute(bool IsRequire, int Len,bool IsAutoIncrement)
        {
            this.IsRequire = IsRequire;
            this.Len = Len;
            this.AutoInc = IsAutoIncrement;
        }
    }
}