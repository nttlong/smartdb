namespace LV.Db.Common
{
    public class QueryFieldInfo
    {
        public string Field { get; set; }
        public string Alias { get; set; }
        public string FnCall { get; internal set; }
    }
}