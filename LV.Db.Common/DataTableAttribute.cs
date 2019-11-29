using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using System.Data;

namespace LV.Db.Common
{
    public class QueryDataTableAttribute : Attribute
    {
        public string TableName { get; set; }
        public string Schema { get;  set; }
        public QueryDataTableAttribute(string tableName)
        {
            this.TableName = tableName;
        }
        public QueryDataTableAttribute(string Schema, string tableName)
        {
            this.TableName = tableName;
            this.Schema = Schema;
        }

    }
    public class PrimaryKeysAttribute:Attribute
    {
        /// <summary>
        /// Declare Primary key if table
        /// </summary>
        /// <param name="Columns">Seperated by comma</param>
        public PrimaryKeysAttribute(string Columns)
        {
            this.Columns = Columns;
        }

        public string Columns { get; }
    }
    public class UniqueIndexAttribute : Attribute
    {
        /// <summary>
        /// Declare Primary key if table
        /// </summary>
        /// <param name="Columns">Seperated by comma</param>
        public UniqueIndexAttribute(params string[] Columns)
        {
            this.Columns = Columns;
        }

        public string[] Columns { get; set; }
    }
    public class CreateFKContext
    {
        public ForeignKeyInfo[] Data { get; set; }
        public bool IsNew { get; set; }
        public string ConnectionString { get; set; }
        public string DbSchema { get; set; }
        public string DataName { get; set; }
        public string Key { get; set; }
    }
    public interface IForeignKeyModel
    {
        ForeignKeyInfo[] CreateForeignKeys();
    }
    public class ForeignKeyInfo
    {
        public Expression Expr { get; internal set; }
        public string FromTable { get; internal set; }
        public Action<IDb,object> CreateTableAction { get; set; }
        public Type TableType { get; internal set; }
        
        public DbForeignKeyInfoFields[] Fields { get; internal set; }
        public string ForeignTable { get; internal set; }
    }
    internal static class Extractors
    {
        internal static DbForeignKeyInfo GetExpr(DbForeignKeyInfo Info, Expression Expr)
        {
            if(Expr is LambdaExpression)
            {
                var x = Expr as LambdaExpression;
                var bn = ((BinaryExpression)(x.Body));
                if (bn.NodeType == ExpressionType.Equal)
                {
                    var lmb =(MemberExpression)bn.Left;
                    var rmb = (MemberExpression)bn.Right;
                    if (lmb.Expression.Type==Info.FromType)
                    {
                        Info.Fields.Add(new DbForeignKeyInfoFields
                        {
                            LocalField=lmb.Member.Name,
                            ForeignField=rmb.Member.Name
                        });
                        return Info;
                    }
                    else
                    {
                        Info.Fields.Add(new DbForeignKeyInfoFields
                        {
                            LocalField = rmb.Member.Name,
                            ForeignField = lmb.Member.Name
                        });
                        return Info;
                    }
                }
                else if (bn.NodeType == ExpressionType.AndAlso)
                {
                    GetExpr(Info, bn.Left);
                    GetExpr(Info, bn.Right);
                    return Info;
                }
                else
                {
                    throw new NotSupportedException();
                }
                //((BinaryExpression)(x.Body)).NodeType
                //FindAllFields(Info)
                //return GetExpr(Info, x.Body as BinaryExpression);


            }
            if(Expr is BinaryExpression)
            {
                var bn = Expr as BinaryExpression;
                if (bn.NodeType == ExpressionType.Equal)
                {
                    var lmb = (MemberExpression)bn.Left;
                    var rmb = (MemberExpression)bn.Right;
                    if (lmb.Expression.Type == Info.FromType)
                    {
                        Info.Fields.Add(new DbForeignKeyInfoFields
                        {
                            LocalField = lmb.Member.Name,
                            ForeignField = rmb.Member.Name
                        });
                        return Info;
                    }
                    else
                    {
                        Info.Fields.Add(new DbForeignKeyInfoFields
                        {
                            LocalField = rmb.Member.Name,
                            ForeignField = lmb.Member.Name
                        });
                        return Info;
                    }
                }
                else if (bn.NodeType == ExpressionType.AndAlso)
                {
                    GetExpr(Info, bn.Left);
                    GetExpr(Info, bn.Right);
                    return Info;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            throw new NotImplementedException();
        }
    }
    public class DbForeignKeyInfoFields
    {
        public string LocalField { get; internal set; }
        public string ForeignField { get; internal set; }
    }
    internal class DbForeignKeyInfo
    {
        public Type FromType { get; internal set; }
        public string FromTable { get; internal set; }
        public string ForeignTable { get; internal set; }
        public List<DbForeignKeyInfoFields> Fields { get; internal set; }
        public LambdaExpression Expr { get; internal set; }
    }
    public class ForeignKeyObject<T>
    {
        public List<ForeignKeyInfo> Data { get; set; }
        internal ForeignKeyObject()
        {
            this.Data = new List<ForeignKeyInfo>();
        }
        public ForeignKeyObject<T> To<T2>(Expression<Func<T, T2,bool>> Conditional)
        {
            
            var table_name = typeof(T2).GetCustomAttributes(false).Where(x => x is QueryDataTableAttribute).Cast<QueryDataTableAttribute>().FirstOrDefault().TableName;
            var fromTable = typeof(T).GetCustomAttributes(false).Where(x => x is QueryDataTableAttribute).Cast<QueryDataTableAttribute>().FirstOrDefault().TableName;
            var tblInfo = new DbForeignKeyInfo
            {
                FromType=typeof(T),
                FromTable=fromTable,
                ForeignTable=table_name,
                Fields=new List<DbForeignKeyInfoFields>(),
                Expr=(LambdaExpression)Conditional
            };
            tblInfo = Extractors.GetExpr(tblInfo, Conditional);
            Data.Add(new ForeignKeyInfo
            {
                CreateTableAction = (db,cnn) =>
                {
                    if (!db.IsExisTable(cnn, table_name))
                    {
                        db.CreateTableIfNotExist(typeof(T2), cnn, table_name);
                        db.ModifyAutoIncrement(typeof(T2), cnn, table_name);
                        db.ModifyPrimaryKey(typeof(T2), cnn, table_name);
                    }
                    else
                    {
                        db.ModifyTable(typeof(T), cnn, table_name);
                    }
                    
                    db.ModifyUniqueConstraints(typeof(T2), cnn, table_name);
                },
                Expr= Conditional,
                TableType=typeof(T2),
                FromTable= tblInfo.FromTable,
                ForeignTable=tblInfo.ForeignTable,
                Fields= tblInfo.Fields.ToArray()
            });
            return this;
        }
        public ForeignKeyInfo[] ToForeignKeyInfo()
        {
            return this.Data.ToArray();
        }
    }
    public static class ForeignKeys
    {
        public static ForeignKeyObject<T> Create<T>(T Instance)
        {
            return new ForeignKeyObject<T>();
        }
        
    }
}
