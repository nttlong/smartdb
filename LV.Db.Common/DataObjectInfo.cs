using System;
using System.Collections.Generic;
using System.Text;

namespace LV.Db.Common
{
    public class DataObjectInfo
    {
        public string Name { get; set; }
        public string ObjectType { get; set; }
        public List<DataObjectInfo> Columns { get; set; }
        public string DbType { get; set; }
        public bool IsRequire { get; set; }
        public int DbLen { get; set; }
        public string DefaultValue { get; set; }
        public int Numeric_Precision { get; set; }
        public int Numeric_Precision_Radix { get; set; }
        public object Numeric_Scale { get; set; }
        public bool IsSeq { get; set; }
        public string SeqName { get; set; }
    }
}
