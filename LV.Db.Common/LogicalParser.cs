//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Text;

//namespace LV.Db.Common
//{
//    /// <summary>
//    /// The class is responsibility for common SQL built-in function parser such as: Case when, COALESCE, ISNULL,...
//    /// </summary>
//    internal class CommonSQLFunctionsParser
//    {
//        internal static object ParseCase(MethodCallExpression cx,string[] Schemas, object[] tables, IList<object> parameters, MemberInfo[] members, int memberIndex, ref List<object> paramList)
//        {
//            var condition = cx.Arguments[0];
//            var strConditoinal = string.Empty;
//            var isEmptyConditional = false;
//            if (condition is ConstantExpression && ((ConstantExpression)condition).Value == null)
//            {
//                isEmptyConditional = true;
//            }
//            if (cx.Arguments[1] is NewArrayExpression)
//            {
//                var nax = cx.Arguments[1] as NewArrayExpression;
//                if (nax.Expressions.Count < 2)
//                {
//                    throw new Exception(@"It looks like you missing parameter in Case\r\n . Call Case should be SqlFuncs.Case(<Expr>,SqlFuncs.When(<expr or null>,expr1,..,exprn,<else return value>))");
//                }
//                var elseExpr = nax.Expressions[nax.Expressions.Count - 1];
//                string strElse = SelectorCompiler.Gobble<string>(elseExpr, Schemas, tables, parameters.Cast<object>().ToList(), members, memberIndex, ref paramList);
//                var whenList = new List<string>();
//                if (!isEmptyConditional)
//                {
//                    strConditoinal = SelectorCompiler.Gobble<string>(condition, Schemas, tables, parameters.Cast<object>().ToList(), members, memberIndex, ref paramList);
//                }
//                for (var i = 0; i < nax.Expressions.Count - 1; i++)
//                {
//                    var x = nax.Expressions[i];
//                    if (x is MethodCallExpression)
//                    {
//                        var args = ((MethodCallExpression)x).Arguments;

//                        var strWhen = SelectorCompiler.Gobble<object>(args[0], Schemas,tables, parameters.Cast<object>().ToList(), null, memberIndex, ref paramList);
//                        var strThen = SelectorCompiler.Gobble<string>(args[1], Schemas,tables, parameters.Cast<object>().ToList(), null, memberIndex, ref paramList);
//                        whenList.Add(" when " + strWhen + " then " + strThen);
//                    }
//                    else if(x is UnaryExpression)
//                    {
//                        var ux = x as UnaryExpression;

//                    }
//                    else
//                    {
//                        throw new NotImplementedException();
//                    }
//                }
               
//                if (isEmptyConditional)
//                {
//                    return "Case " + string.Join("", whenList.ToArray()) + " else " + strElse + " end ";
//                }
//                else
//                {
//                    return "Case " + strConditoinal + " " + string.Join("", whenList.ToArray()) + " else " + strElse + " end ";
//                }
//                //var strElse = SelectorCompiler.Gobble<string>(elseExpr, tables,nax.Expressions.Cast<ParameterExpression>().ToList(), Members,-1,ref paramList);

//            }
//            if (cx.Arguments.Count < 3)
//            {

//            }
//            throw new NotImplementedException();
//        }

//        internal static object ParseCOALESCE(MethodCallExpression cx, string[] schemas, string[] tables, List<object> parameters, MemberInfo[] members, int memberIndex, ref List<object> paramList)
//        {
//            //new System.Linq.Expressions.Expression.NewArrayExpressionProxy(new System.Linq.Expressions.Expression.MethodCallExpressionProxy(cx).Arguments[0]).Expressions
//            if(cx.Arguments[0] is NewArrayExpression)
//            {
//                var nax = cx.Arguments[0] as NewArrayExpression;
//                var agrs = new List<string>();
//                foreach(var x in nax.Expressions)
//                {
//                    var strParam = SelectorCompiler.Gobble<string>(x,schemas, tables, parameters, members, memberIndex, ref paramList);
//                    agrs.Add(strParam);
//                }
//                return "COALESCE(" + string.Join(",", agrs.ToArray()) + ")";
//            }
//            throw new NotImplementedException();
//        }
//    }
//}
