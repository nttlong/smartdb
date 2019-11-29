using System.Collections.Generic;

namespace LV.Db.Common
{
    public class SqlCommandInfo
    {
        public SqlCommandInfo()
        {
        }

        public string Source { get; internal set; }
        public List<DMLFieldInfo> Fields { get; internal set; }
        public string JoinClause { get; internal set; }
        public string Filter { get; internal set; }
        /// <summary>
        /// For Cross Join this value will be null
        /// </summary>
        public string JoinExpression { get; internal set; }
        /// <summary>
        /// List of origin table
        /// </summary>
        public List<TableAliasInfo> Tables { get; internal set; }
        public string Sql { get; internal set; }
        public TableAliasInfo OriginTableInfo { get; internal set; }
        public List<string> KeyFields { get; internal set; }
        public bool IsTempTable { get; set; }
    }
}