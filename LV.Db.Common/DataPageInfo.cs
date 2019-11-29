using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LV.Db.Common
{
    /// <summary>
    /// The information tell Db.Common how to get a page of data items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataPageInfo<T>
    {
        /// <summary>
        /// Num of item for each page
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// Which page would you like to take data, please set here
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// Before get one page of data items. Which information would you like to sort, please set here
        /// </summary>
        public DataPageInfoSorting[] Sort { get; set; }
        /// <summary>
        /// Before sorting and get page of item. Which data row would you like to show the end user, please set on conditional here
        /// </summary>
        public Expression<Func<T,bool>> Filter { get; set; }
        public FilterInfo[] Filters { get; set; }
        public DataPageInfo()
        {
            this.PageIndex = 0;
            this.PageSize = 50;
            this.Sort = new DataPageInfoSorting[] { };
        }

        public string GetFieldName(Expression<Func<T, object>> Expr)
        {
            if(Expr.Body is UnaryExpression)
            {
                var mb = ((UnaryExpression)Expr.Body).Operand;
                if(mb is MemberExpression)
                {
                    return ((MemberExpression)mb).Member.Name;
                }
            }
            if (Expr.Body is MemberExpression)
            {
                return ((MemberExpression)Expr.Body).Member.Name;
            }
            return Expr.Name;
        }
    }

    public class DataPageInfoSorting
    {
        /// <summary>
        /// Data field for sorting
        /// </summary>
        public string Field { get; set; }
        /// <summary>
        /// Default is always sort ascending, but ISDesc is true
        /// </summary>
        public bool IsDesc { get; set; }
    }
}