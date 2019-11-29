using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LV.Db.Common
{
    public class QrPage<T>
    {
        private QuerySet<T> query;
        private DataPageInfo<T> pager;
      

        public QrPage(QuerySet<T> Qr)
        {
            this.query = Qr;
            this.pager = new DataPageInfo<T>();
            this.pager.Sort = new DataPageInfoSorting[] { };
        }

        public QrPage(IQueryable<T> qr)
        {
            this.query = qr.AsQuerySet();
            this.pager = new DataPageInfo<T>();
            this.pager.Sort = new DataPageInfoSorting[] { };
        }
        public QrPage<T> Sort(string FieldName)
        {
            var lst = this.pager.Sort.ToList();
            lst.Add(new DataPageInfoSorting
            {
                Field = FieldName
            });
            this.pager.Sort = lst.ToArray();
            return this;
        }
        public QrPage<T> Sort(string FieldName,bool IsDesc)
        {
            var lst = this.pager.Sort.ToList();
            lst.Add(new DataPageInfoSorting
            {
                Field = FieldName,
                IsDesc = IsDesc
            });
            this.pager.Sort = lst.ToArray();
            return this;
        }
        public QrPage<T> Sort(Expression<Func<T,object>> Expr)
        {
            var lst = this.pager.Sort.ToList();
            lst.Add(new DataPageInfoSorting
            {
                Field=((MemberExpression)Expr.Body).Member.Name
            });
            this.pager.Sort = lst.ToArray();
            return this;
        }
        public QrPage<T> Sort(Expression<Func<T, object>> Expr,bool IsDesc)
        {
            var lst = this.pager.Sort.ToList();
            lst.Add(new DataPageInfoSorting
            {
                Field = ((MemberExpression)Expr.Body).Member.Name,
                IsDesc=IsDesc
            });
            return this;
        }
        public QrPage<T> SetPageIndex(long index)
        {
            this.pager.PageIndex = (int)index ;
            return this;
        }
        public QrPage<T> SetPageIndex(int index)
        {
            this.pager.PageIndex = index;
            return this;
        }
        public QrPage<T> SetPageSize(long size)
        {
            this.pager.PageSize = (int)size;
            return this;
        }
        public QrPage<T> SetPageSize(int size)
        {
            this.pager.PageSize = size;
            return this;
        }
        public DataPage<T> Eval()
        {
            var ret = this.query.GetPage<T>(this.pager);
            return ret;
        }
        public QrPage<T> Filter(Expression<Func<T,bool>> expr)
        {
            this.pager.Filter = expr;
            return this;
        }
    }
}
