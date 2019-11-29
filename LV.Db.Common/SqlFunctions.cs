using System;
using System.Linq.Expressions;

namespace LV.Db.Common
{
    public static class SqlFuncs
    {
        internal const string AggregateFuncs = ",min,max,sum,count,avg,stdev,";
        public static int Len(object Field)
        {
            throw new NotImplementedException();
        }

        public static object Month(object Field)
        {
            throw new NotImplementedException();
        }

        public static object Call(string FunctionSchema, string FunctionName, params object[] Pars)
        {
            throw new NotImplementedException();
        }
        public static T Call<T>(string FunctionSchema, string FunctionName, params object[] Pars)
        {
            throw new NotImplementedException();
        }

        public static int Year(DateTime birthDate)
        {
            throw new NotImplementedException();
        }

        public static decimal Sum(object expr)
        {
            throw new NotImplementedException();
        }

        public static bool Like(object Expr, object CompareExpr)
        {
            throw new NotImplementedException();
        }
        public static bool Contains(object Expr, object CompareExpr)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// return true if CompareExpr is in first Expr
        /// <para>
        /// Example:Expr='abc' and CompareExpr='ab' return true
        /// </para>
        /// </summary>
        /// <param name="Expr"></param>
        /// <param name="CompareExpr"></param>
        /// <returns></returns>
        public static bool FirstContains(object Expr, object CompareExpr)
        {
            throw new NotImplementedException();
        }
        public static bool LastContains(object Expr, object CompareExpr)
        {
            throw new NotImplementedException();
        }
        public static T Max<T>(T Expr)
        {
            throw new NotImplementedException();
        }

        public static T Avg<T>(T Expr)
        {
            throw new NotImplementedException();
        }

        public static double Cast(object FieldOrEpxression, string SqlDataTypeName)
        {
            throw new NotImplementedException();
        }

        public static T Min<T>(T expr)
        {
            throw new NotImplementedException();
        }
        public static object Max(object expr)
        {
            throw new NotImplementedException();
        }
        public static T Max<T>(object expr)
        {
            throw new NotImplementedException();
        }
        public static T Average<T>(object expr)
        {
            throw new NotImplementedException();
        }
        public static int Count(object expr)
        {
            throw new NotImplementedException();
        }
        public static string Concat(params object[] args)
        {
            throw new NotImplementedException();
        }
        public static T Concat<T>(params object[] args)
        {
            throw new NotImplementedException();
        }

        
        
        public static T IsNull<T>(T InputReturnIfNotNull, T ReturnIfInputNull)
        {
            throw new NotImplementedException();
        }
        
        public static object When(object Value, object ReturnValue)
        {
            throw new NotImplementedException();
        }
       
        public static T When<T>(bool Value, T ReturnValue)
        {
            throw new NotImplementedException();
        }
        public static T Case<T>(object owner,params object[] Branches )
        {
            throw new NotImplementedException();
        }
        public static T Case<T>( params object[] Branches)
        {
            throw new NotImplementedException();
        }
        public static object Case(object owner, params object[] Branches)
        {
            throw new NotImplementedException();
        }

        public static bool In(string objectId, params object[] Params)
        {
            throw new NotImplementedException();
        }
    }
    public class WhenStatement
    {
        private object expr;

        public WhenStatement()
        {
        }

        public WhenStatement(object expr)
        {
            this.expr = expr;
        }

        public ThenStatement When(bool v)
        {
            return new ThenStatement();
        }
    }
    public class ThenStatement
    {
        public bool Then(object Expr)
        {
            throw new NotImplementedException();
        }
    }
}