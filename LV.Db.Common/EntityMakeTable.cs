using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LV.Db.Common
{
    public class EntityMakeTable<T>
    {
        internal QuerySet<T> querySet;
        internal string schema;
        internal string tableName;
        internal List<QueryFieldInfo> fields;

        public bool IsTempTable { get; internal set; }

        public EntityMakeTable(QuerySet<T> querySet, string schema, string tableName)
        {
            this.querySet = querySet;
            this.schema = schema;
            this.tableName = tableName;
        }

        public EntityMakeTable<T> BySelect<T1>(Expression<Func<T, T1>> Selector)
        {

            var tmpQr = this.querySet.Clone<T>();
            this.fields = tmpQr.Select(Selector).fields;
            this.querySet.Params = tmpQr.Params;
            return this;
        }

        public string ToSQLString()
        {
            
            string sql = this.querySet.compiler.CreateSQLMakeTable(new SqlCommandInfo()
            {
                Sql = this.querySet.ToSQLString(),
                Source = this.querySet.compiler.GetQName(this.querySet.Schema, this.querySet.table_name),
                Fields = this.fields.Select(p=>new DMLFieldInfo() {
                    Source=p.Field,
                    ToField=p.Alias
                }).ToList(),
                JoinClause = this.querySet.datasource,
                JoinExpression = string.Join(" and ", this.querySet.strJoinClause),
                Filter = this.querySet.where,
                Tables = this.querySet.tableAlias,
                OriginTableInfo = new TableAliasInfo()
                {
                    Schema=this.schema,
                    TableName=this.tableName
                },
                IsTempTable=this.IsTempTable
            });
            return sql;
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