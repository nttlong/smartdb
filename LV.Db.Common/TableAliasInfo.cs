namespace LV.Db.Common
{
    public class TableAliasInfo
    {
        public string Schema { get; internal set; }
        public string TableName { get; internal set; }
        public string Alias { get; internal set; }
    }
}