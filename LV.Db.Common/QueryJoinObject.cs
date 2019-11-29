namespace LV.Db.Common
{
    internal class QueryJoinObject
    {
        public QueryJoinObject()
        {
        }

        public object L { get; internal set; }
        public object R { get; internal set; }
        public string J { get; internal set; }
        public string C { get; internal set; }
    }
}