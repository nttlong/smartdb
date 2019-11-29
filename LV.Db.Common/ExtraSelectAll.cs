using System.Collections.Generic;
using System.Linq.Expressions;

namespace LV.Db.Common
{
    internal class ExtraSelectAll
    {
        public Expression Expr { get; internal set; }
        public List<string> Tables { get; internal set; }
    }
}