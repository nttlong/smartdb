using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LV.Db.Common
{
    public class ExpressionBuilder
    {
        /// <summary>
        /// Add More update field from updatter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ExprFields"></param>
        /// <param name="Field"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static Expression<Func<object,T>> AddField<T>(Expression<Func<object, T>> ExprFields, Expression<Func<T, object>> Field,object Value)
        {
            
            var body = ExprFields.Body;
            if(!(body is MemberInitExpression))
            {
                throw new Exception(string.Format("This function ca not use with anonymous class '{0}'", typeof(T)));
            }
            var mbr = Field.Body as MemberExpression;
            var mix = body as MemberInitExpression;
            var binds = mix.Bindings.ToList();
            MemberBinding assignment = Expression.Bind(typeof(T).GetProperty(mbr.Member.Name), Expression.Constant(Value));
            binds.Add(assignment);
            ParameterExpression parameter = Expression.Parameter
            (
                typeof(object),
                "m"
            );
            var ret = Expression.Lambda<Func<object, T>>(Expression.MemberInit(Expression.New(typeof(T)), binds), parameter);
            return ret;
            
        }
        public static Expression<Func<object, T>> AddField<T>(Expression<Func<object, T>> ExprFields, string FieldName, object Value)
        {

            var body = ExprFields.Body;
            if (!(body is MemberInitExpression))
            {
                throw new Exception(string.Format("This function ca not use with anonymous class '{0}'", typeof(T)));
            }
            var fp = typeof(T).GetProperties().FirstOrDefault(p => p.Name.ToLower() == FieldName.ToLower());
            if (fp == null)
            {
                throw new Exception(string.Format("Can not find property '{0}' in '{1}", FieldName, typeof(T)));
            }
            var mix = body as MemberInitExpression;
            var binds = mix.Bindings.ToList();
            MemberBinding assignment = Expression.Bind(fp, Expression.Constant(Value));
            binds.Add(assignment);
            ParameterExpression parameter = Expression.Parameter
            (
                typeof(object),
                "m"
            );
            var ret = Expression.Lambda<Func<object, T>>(Expression.MemberInit(Expression.New(typeof(T)), binds), parameter);
            return ret;

        }

        public static Expression<Func<T, bool>> And<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var l = expr1 as LambdaExpression;
            var r = expr2 as LambdaExpression;
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(l.Body, r.Body), Expression.Parameter(typeof(T)));
        }

        public static Expression<Func<object, T>> AddField<T>(Expression<Func<object, T>> ExprFields, Expression<Func<T, T>> ExprMoreFields)
        {

           
            var body = ExprFields.Body;
            if (!(body is MemberInitExpression))
            {
                throw new Exception(string.Format("This function ca not use with anonymous class '{0}'", typeof(T)));
            }
            var mbr = ExprMoreFields.Body as MemberInitExpression;
            var mix = body as MemberInitExpression;
            var binds = mix.Bindings.ToList();
            binds.Concat(mbr.Bindings);
            ParameterExpression parameter = Expression.Parameter
            (
                typeof(object),
                "m"
            );
            var ret = Expression.Lambda<Func<object, T>>(Expression.MemberInit(Expression.New(typeof(T)), binds), parameter);
            return ret;

        }
    }
}
