using System.Reflection;

namespace LV.Db.Common
{
    internal class FieldMapping
    {
        public string FieldName { get; internal set; }
        public MemberInfo Member { get; internal set; }
    }
}