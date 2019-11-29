using System;
using System.Linq;

namespace LV.Db.Common
{
    public class DeleteEntity<T>
    {
        internal QuerySet<T> querySet;


        public DeleteEntity(QuerySet<T> querySet)
        {
            this.querySet = querySet.Clone<T>();
        }

        public string ToSQLString()
        {
            if (this.querySet.tableAlias.Count > 0)
            {

                var OriginTableInfo = this.querySet.tableAlias.FirstOrDefault(p => p.TableName == this.querySet.table_name || p.Alias == this.querySet.table_name);
                TableInfo tableInfo = utils.GetTableInfo<T>(this.querySet.compiler, OriginTableInfo.Schema, OriginTableInfo.TableName);
                var keyFields = tableInfo.Columns.Where(p => p.IsKey).Select(p => p.Name).ToList();
                string sql = this.querySet.compiler.CreateSQLDeleteString(new SqlCommandInfo()
                {
                    OriginTableInfo = OriginTableInfo,
                    KeyFields = keyFields,
                    Sql = this.querySet.ToSQLString(),
                    Source = this.querySet.compiler.GetQName(this.querySet.Schema, this.querySet.table_name),
                    JoinClause = this.querySet.datasource,
                    JoinExpression = string.Join(" and ", this.querySet.strJoinClause),
                    Filter = this.querySet.where,
                    Tables = this.querySet.tableAlias
                });
                return sql;
            }
            else
            {
                var ret = "delete " + this.querySet.compiler.GetQName(this.querySet.Schema, this.querySet.table_name);
                if (this.querySet.where != null)
                {
                    ret += " where " + this.querySet.where;
                }
                return ret;
            }
        }

        public override string ToString()
        {
            var sql = this.ToSQLString();
            var index = 0;
            foreach (var p in this.querySet.Params)
            {

                if (p == null)
                {
                    sql = sql.Replace("{" + index.ToString() + "}", "NULL");
                }
                else if (p is string)
                {
                    sql = sql.Replace("{" + index.ToString() + "}", "'" + p.ToString().Replace("'", "''") + "'");
                }
                else if (p is DateTime)
                {
                    sql = sql.Replace("{" + index.ToString() + "}", "'" + ((DateTime)p).ToString("yyyy-MM-dd hh:mm:ss") + "'");
                }
                else
                {
                    sql = sql.Replace("{" + index.ToString() + "}", "" + p.ToString() + "");
                }
                index++;
            }
            return sql;
        }

        public DataActionResult Execute(DBContext db)
        {
            return db.ExcuteCommand(this.ToSQLString(), this.querySet.Params.Cast<ParamConst>().ToArray());
        }
    }
}