using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LV.Db.Common
{
    public static class DynamicExtension
    {
        public static QuerySet<T> SortByFields<T>(this QuerySet<T> qr, SortByFieldNameInfo[] SortFields)
        {
            if (SortFields != null && SortFields.Length > 0)
            {
                qr = qr.SortByFieldName(SortFields[0].Name, SortFields[0].Asc);
            }
            return qr;
        }
        public static Expression<Func<T, bool>> GetSearchExpression<T>(this QuerySet<T> qr, ParameterExpression ParamsEpr, string Field, string SearchValue)
        {


            var member = Expression.Property(ParamsEpr, Field); //x.Id
            var constant = Expression.Constant(SearchValue);
            var body = Expression.Call(typeof(SqlFuncs).GetMethod("Contains"), member, constant);
            //var body = Expression.GreaterThanOrEqual(member, constant); //x.Id >= 3
            var finalExpression = Expression.Lambda<Func<T, bool>>(body, ParamsEpr); //x => x.Id >= 

            return finalExpression;
        }
        public static QuerySet<T> Search<T>(this QuerySet<T> qr, string TextFieldsSearch, string SearchValue)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var fields = TextFieldsSearch.Split(',');
            var expr = qr.GetSearchExpression(parameter, fields[0], SearchValue);
            for (var i = 1; i < fields.Length; i++)
            {
                var expr2 = qr.GetSearchExpression(parameter, fields[i], SearchValue);
                expr = Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr.Body, expr2.Body), parameter);
            }

            qr = qr.AsQuerySet().Filter(expr);
            return qr;
        }
    }
}
