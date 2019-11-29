namespace LV.Db.Common
{
    public class DbColumnInfo
    {
        public string Name { get; set; }
        public int DbLen { get; set; }
        public string DbType { get; set; }
        public bool IsRequire { get; set; }
        public string DefaultValue { get; set; }
    }
}