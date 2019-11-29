using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace LV.Db.Common
{
    public class EntityInsert<T, T1>
    {
        internal QuerySet<T1> querySet;
        internal List<DMLFieldInfo> fields=new List<DMLFieldInfo>();
        internal string tableInsert;
        internal string schemaName;
        private string subQueryAlias;
        internal string aliasForInsert;
        internal string schemaForInsert;

        public EntityInsert(QuerySet<T1> querySet)
        {
            this.querySet = querySet.Clone<T1>();
        }


       
        public EntityInsert<T,T1> Set(Expression<Func<T, object>> field, Expression<Func<T1, object>> source)
        {
            var Params = this.querySet.Params;
            var mapAlais = new List<MapAlais>();
            mapAlais.Add(new MapAlais()
            {
                Schema = this.schemaName ?? this.querySet.Schema,
                TableName = this.tableInsert ?? this.querySet.table_name,
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
                
                
                var sourceExpr = Compiler.GetField(this.querySet.compiler, source.Body, this.querySet.FieldsMapping, this.querySet.mapAlias, ref Params);
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
                    AliasName=alias,
                    ParamExpr= source.Parameters[0]
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

        

        public EntityInsert<T, T1> Set(Expression<Func<T, object>> field, object value)
        {
            var Params = this.querySet.Params;
            var mapAlais = new List<MapAlais>();
            mapAlais.Add(new MapAlais()
            {
                Schema = this.schemaName ?? this.querySet.Schema,
                TableName = this.tableInsert ?? this.querySet.table_name,
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
        public string ToSQLString()
        {
            if (
                 (this.querySet.groupbyList.Count > 0|| 
                 this.querySet.selectTop > 0|| 
                 this.querySet.skipNumber > 0|| 
                 this.querySet.isSubQuery|| (this.querySet.unionSQLs != null) && (this.querySet.unionSQLs.Count > 0)) || 
                 this.querySet.hasExpressionInSelector)
            {
                var strTable = this.querySet.compiler.GetQName(this.schemaName ?? this.querySet.Schema, this.tableInsert ?? this.querySet.table_name);
                var ret = "insert into " + strTable;
                if (this.fields.Count > 0)
                {
                    ret += " (" + string.Join(",", this.fields.Select(p => p.ToField.Replace(strTable + ".", "")).ToArray()) + ")";
                    ret += " select " + string.Join(",", this.fields.Select(p => p.Source.Replace(strTable + ".", "")).ToArray()) +
                        " from (" + this.querySet.ToSQLString() + ") as "+ this.querySet.compiler.GetQName("",this.subQueryAlias);
                }
                else
                {
                    ret += "  " + this.querySet.ToSQLString();
                }
                
                return ret;
            }
            if (this.querySet.tableAlias.Count > 0)
            {
                var strTable = this.querySet.compiler.GetQName(this.schemaName, this.tableInsert);
                var ret = "insert into " + strTable;
                if (this.fields.Count > 0)
                {
                    ret += " (" + string.Join(",", this.fields.Select(p => p.ToField.Replace(strTable + ".", "")).ToArray()) + ")";
                    ret += " select " + string.Join(",", this.fields.Select(p => p.Source.Replace(strTable + ".", "")).ToArray()) +
                        " from " + this.querySet.datasource;
                }
                else
                {
                    ret += " select "+ this.querySet.compiler.GetQName(this.schemaForInsert,this.aliasForInsert)+".*  from " + this.querySet.datasource;
                }
                if (!string.IsNullOrEmpty(this.querySet.where))
                {
                    ret += " where " + this.querySet.where;
                }
                return ret;
            }
            else
            {
                var ret = "insert into " + this.querySet.compiler.GetQName(this.querySet.Schema, this.querySet.table_name) +
                             " (" + string.Join(",", this.fields.Select(p => p.ToField)) + ")" +
                             " values(" + string.Join(",", this.fields.Select(p => p.Source).ToArray()) + ")";
                if (this.querySet.where != null)
                {
                    ret += " where " + this.querySet.where;
                }

                TableInfo tableInfo = utils.GetTableInfo<T>(this.querySet.compiler, this.querySet.Schema, this.querySet.table_name);
                ret += ";" + this.querySet.compiler.CreateSQLGetIdentity(this.querySet.Schema, this.querySet.table_name, tableInfo);
                return ret;
            }
        }

       

        public DataActionResult Execute(DBContext db)
        {
            return db.ExcuteCommand(this.ToSQLString(), this.querySet.Params.Cast<ParamConst>().ToArray());
        }
        public DataActionResult Execute()
        {
            if (Globals.Connection == null)
            {
                return this.Execute(this.querySet.dbContext);
            }
            else
            {
                return this.ExecuteByConnection(Globals.Connection);
            }
            
        }

        private DataActionResult ExecuteByConnection(object connection)
        {
           
            DataTable retData = this.querySet.compiler.ExecSQLAndReturnDatatable(connection,this.ToSQLString(),this.querySet.Params.ToArray());
            return new DataActionResult()
            {
                
            };
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