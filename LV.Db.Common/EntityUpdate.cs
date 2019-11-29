using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LV.Db.Common
{
    public class EntityUpdate<T,T1>
    {
        internal QuerySet<T> querySet;
        

        internal List<DMLFieldInfo> fields;
        private string subQueryAlias;

        //[System.Diagnostics.DebuggerStepThrough]
        public EntityUpdate(QuerySet<T1> querySet)
        {
            if (querySet.unionSQLs.Count > 0)
            {
                throw new Exception("It look like you try to update or insert to union query");
            }
            if (querySet.groupbyList.Count > 0)
            {
                throw new Exception("It look like you try to update or insert to query with group by");
            }
            if (! string.IsNullOrEmpty(querySet.having))
            {
                throw new Exception("It look like you try to update or insert to query with having clause");
            }
            this.querySet = querySet.Clone<T>();
            this.fields = new List<DMLFieldInfo>();
        }
        public EntityUpdate<T, T1> Set(Expression<Func<T, object>> field, Expression<Func<T1, object>> source)
        {
            var Params = this.querySet.Params;
            var mapAlais = new List<MapAlais>();
            mapAlais.Add(new MapAlais()
            {
                //Schema = this.schemaName ?? this.querySet.Schema,
                TableName =  this.querySet.table_name,
                ParamExpr = field.Parameters[0],
                Children = this.querySet.mapAlias
            });

            var toField = Compiler.GetField(this.querySet.compiler, field.Body,this.querySet.FieldsMapping, mapAlais, ref Params);
            var isSubQr = (this.querySet.groupbyList.Count > 0 ||
                this.querySet.selectTop > 0 ||
                this.querySet.skipNumber > 0 ||
                this.querySet.isSubQuery || (this.querySet.unionSQLs != null) && (this.querySet.unionSQLs.Count > 0)) ||
                this.querySet.hasExpressionInSelector;
            if (!isSubQr)
            {


                var sourceExpr = Compiler.GetField(this.querySet.compiler,source.Body, this.querySet.FieldsMapping, this.querySet.mapAlias, ref Params);
                fields.Add(new DMLFieldInfo()
                {
                    ToField = toField.Replace(this.querySet.compiler.GetQName("", this.querySet.table_name) + ".", ""),
                    Source = sourceExpr,
                    IsConstantValue = false
                });
                this.querySet.Params = Params;
            }
            else
            {
                var alias = "SQL";
                this.subQueryAlias = alias;
                var mapAlias = new List<MapAlais>();
                mapAlias.Add(new MapAlais()
                {
                    AliasName = alias,
                    ParamExpr = source.Parameters[0]
                });
                var sourceExpr = Compiler.GetField(this.querySet.compiler,source.Body,this.querySet.FieldsMapping, mapAlias, ref Params);
                fields.Add(new DMLFieldInfo()
                {
                    ToField = toField.Replace(this.querySet.compiler.GetQName("", this.querySet.table_name) + ".", ""),
                    Source = sourceExpr,
                    IsConstantValue = false
                });
                this.querySet.Params = Params;
            }
            return this;
        }
        
        public EntityUpdate<T, T1> Set(Expression<Func<T, object>> field, object value)
        {
            var Params = this.querySet.Params;
            var mapAlais = new List<MapAlais>();
            mapAlais.Add(new MapAlais()
            {
                Schema = this.querySet.Schema,
                TableName = this.querySet.table_name,
                ParamExpr = field.Parameters[0],
                Children = this.querySet.mapAlias
            });
            var toField = Compiler.GetField(this.querySet.compiler,field.Body,this.querySet.FieldsMapping, mapAlais, ref Params);
            Params.Add(value);
            fields.Add(new DMLFieldInfo()
            {
                ToField = toField.Replace(this.querySet.compiler.GetQName(this.querySet.Schema, this.querySet.table_name) + ".", ""),
                Source = "{" + (Params.Count - 1) + "}",
                IsConstantValue = true
            });
            this.querySet.Params = Params;
            return this;
        }
        public DataActionResult Execute(DBContext db)
        {
            return db.ExcuteCommand(this.ToSQLString(), this.querySet.Params.Cast<ParamConst>().ToArray());
        }

        public string ToSQLString()
        {
            
            if (this.querySet.tableAlias.Count > 0)
            {
                var OriginTableInfo = this.querySet.tableAlias.FirstOrDefault(p => p.TableName == this.querySet.table_name ||p.Alias==this.querySet.table_name);
                TableInfo tableInfo = utils.GetTableInfo<T>(this.querySet.compiler, OriginTableInfo.Schema, OriginTableInfo.TableName);
                var keyFields = tableInfo.Columns.Where(p => p.IsKey).Select(p=>p.Name).ToList();
                string sql = this.querySet.compiler.CreateSQLUpdateString(new SqlCommandInfo()
                {
                    OriginTableInfo= OriginTableInfo,
                    KeyFields=keyFields,
                    Sql = this.querySet.ToSQLString(),
                    Source = this.querySet.compiler.GetQName(this.querySet.Schema, this.querySet.table_name),
                    Fields = this.fields,
                    JoinClause = this.querySet.datasource,
                    JoinExpression = string.Join(" and ", this.querySet.strJoinClause),
                    Filter = this.querySet.where,
                    Tables = this.querySet.tableAlias
                });
                return sql;
            }
            else
            {
                TableInfo tableInfo = utils.GetTableInfo<T>(this.querySet.compiler, this.querySet.Schema, this.querySet.table_name);
                var ret = "update " + this.querySet.compiler.GetQName(this.querySet.Schema, this.querySet.table_name) +
                        " set " + string.Join(",", this.fields.Select(p => p.ToField + "=" + p.Source));
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

        
    }
}