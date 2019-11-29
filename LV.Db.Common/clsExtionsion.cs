using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LV.Db.Common
{
    public static class clsExtionsion
    {
        public static T CreateField<T>(this object obj, string fieldName)
        {
            return default(T);
        }
        [System.Diagnostics.DebuggerStepThrough]
        public static QuerySet<T> AsQuerySet<T>(this IQueryable<T> qr)
        {
            return (QuerySet<T>)qr;
        }
        public static IQueryable<T> ToLinq2DbQueryable<T>(this IQueryable<T> QrSet, object DbContext)
        {
            if (DbContext.GetType().Assembly.GetName().Name == "linq2db")
            {
                var qr = (QuerySet<T>)QrSet;
                var cls = DbContext.GetType().Assembly.GetExportedTypes().FirstOrDefault(p => p.Name == "DataExtensions");
                var method = cls.GetMethods().Where(p => p.Name == "FromSql" &&
                                                    p.GetParameters().Count() == 3 &&
                                               p.GetParameters()[2].ParameterType == typeof(object[])
                ).FirstOrDefault();
                MethodInfo genericMethod = method.MakeGenericMethod(typeof(T));
                var rawSQL = method.GetParameters()[1].ParameterType.GetConstructors().FirstOrDefault(p => p.GetParameters().Count() == 1 &&
                                                                                     p.GetParameters()[0].ParameterType == typeof(string));

                var sql = rawSQL.Invoke(new object[] { qr.ToSQLString() });
                var retList = genericMethod.Invoke(null, new object[] { DbContext, sql, qr.Params.ToArray() }) as IQueryable<T>;
                return retList;
            }
            else
            {
                throw new Exception(string.Format("It looks like {0} is not 'linq2db'", DbContext));
            }
        }
        [System.Diagnostics.DebuggerStepThrough]
        public static IEnumerable<T> ToArray<T>(this IQueryable<T> QrSet,object DbContext){
            var qr = (QuerySet<T>)QrSet;
            if (DbContext.GetType().Assembly.GetName().Name == "linq2db")
            {
                return QrSet.ToLinq2DbQueryable(DbContext).ToArray();
            }
            else
            {
                throw new NotImplementedException();
            }
            
        }
        public static IEnumerable<T> ToList<T>(this IQueryable<T> QrSet, object dbCnt)
        {
            var qr = (QuerySet<T>)QrSet;
            if (dbCnt.GetType().Assembly.GetName().Name == "linq2db")
            {
                return QrSet.ToLinq2DbQueryable(dbCnt).ToList();
            }
            else
            {
                return qr.ToList((DBContext)dbCnt);
            }
        }
        public static DataTable ToDataTable<T>(this IQueryable<T> QrSet)
        {
            var qr = QrSet.AsQuerySet<T>();
            return qr.ToDataTable(qr.dbContext);
        }
        public static EntityInsert<T1,T> InsertOn<T,T1>(this IQueryable<T> QrSet,Expression<Func<T,T1>> TableSelector)
        {
            return QrSet.AsQuerySet<T>().InsertOn(TableSelector);
        }
        public static IQueryable<T> UnionAll<T,T1>(this IQueryable<T> QrSet,IQueryable<T1> Qr)
        {
            return QrSet.AsQuerySet().UnionAll(Qr.AsQuerySet());
        }
        public static DataActionResult UpdateData<T>(this IQueryable<T> QrSet, Expression<Func<object, T>> UpdateExpr)
        {
            return QrSet.AsQuerySet<T>().Update(UpdateExpr).Execute(QrSet.AsQuerySet<T>().dbContext);
        }
        
        public static IQueryable<T2> LeftJoin<T,T1, T2>(this IQueryable<T> QrSet,IQueryable<T1> qr, Expression<Func<T, T1, bool>> JoinClause, Expression<Func<T, T1, T2>> Selector)
        {
            var qrLeft = QrSet as QuerySet<T>;
            var qrRight = QrSet as QuerySet<T1>;
            return qrLeft.LeftJoin(qrRight, JoinClause, Selector);
            
        }
        public static IQueryable<T2> RightJoin<T, T1, T2>(this IQueryable<T> QrSet, IQueryable<T1> qr, Expression<Func<T, T1, bool>> JoinClause, Expression<Func<T, T1, T2>> Selector)
        {
            var qrLeft = QrSet as QuerySet<T>;
            var qrRight = QrSet as QuerySet<T1>;
            return qrLeft.RightJoin(qrRight, JoinClause, Selector);

        }
        public static IQueryable<T2> CrossJoin<T, T1, T2>(this IQueryable<T> QrSet, IQueryable<T1> qr, Expression<Func<T, T1, T2>> Selector)
        {
            var qrLeft = QrSet as QuerySet<T>;
            var qrRight = QrSet as QuerySet<T1>;
            return qrLeft.Join(qrRight,(p,q)=>true,  Selector);

        }
        public static IQueryable<T> Where<T,T1,T2>(this IQueryable<T> QrSet, Expression<Func<T,bool>> Where)
        {
            var qr = QrSet as QuerySet<T>;
            return qr.Filter(Where);

        }
        public static DataPage<T> GetPage<T>(this IQueryable<T> QrSet, DataPageInfo<T> PageInfo)
        {
            var retQr = QrSet.AsQuerySet().GetPage<T>(PageInfo);
            return retQr;
        }
        

    }
}
