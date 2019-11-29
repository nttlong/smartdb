namespace LV.Db.Common
{
    public class DMLFieldInfo
    {
        /// <summary>
        /// Field will be update or insert
        /// </summary>
        public string ToField { get; internal set; }
        /// <summary>
        /// Source for update or insert. For constant the value is open and close bracket with a number inside.It looks like some thing that "{number}"
        /// </summary>
        public string Source { get; internal set; }
        public bool IsConstantValue { get; internal set; }
    }
}