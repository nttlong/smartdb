using System.Linq.Expressions;
using System.Reflection;

namespace LV.Db.Common
{
    internal class QueryMemberInfo
    {
        public Expression Expression { get; internal set; }
        public MemberInfo Member { get; internal set; }
    }
}