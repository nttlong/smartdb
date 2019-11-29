namespace LV.Db.Common
{
    public class ColumInfo
    {
        public string Name { get; set; }
        //public string DefaultValue { get; set; }
        //public bool IsAuto { get; set; }
        //public string AutoConstraint { get; set; }
        public bool IsUnique { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool IsKey { get; set; }
        public bool AllowDBNull { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsExpression { get; set; }
        public bool IsIdentity { get; set; }
        public object DataType { get; set; }
        public int ColumnSize { get; set; }
        public object Value { get; internal set; }
        public string TableName { get; set; }
    }
}