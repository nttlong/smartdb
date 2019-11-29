namespace LV.Db.Common
{
    internal class ExceedFieldInfo
    {
        public ExceedFieldInfo()
        {
        }

        public string Value { get; set; }
        public string Name { get; internal set; }
        public int ColumnSize { get; internal set; }
    }
}