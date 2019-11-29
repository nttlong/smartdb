using System.Collections.Generic;

namespace LV.Db.Common
{
    public class PrimaryKeyInfo
    {
        public string TableName { get; set; }
        public List<PrimaryKeyConstraintInfo> Constraint { get; set; }
    }
    public class PrimaryKeyConstraintInfo
    {
        public string Name { get; set; }
        public List<PrimaryKeyConstraintInfoColumn> Columns { get; set; }
    }
    public class PrimaryKeyConstraintInfoColumn
    {
        public string Name { get; set; }
    }
}