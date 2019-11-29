using System.Collections.Generic;

namespace LV.Db.Common
{
    public class PageFilterInfo
    {
        public PageFilterInfo()
        {
            this.Sort = new List<DataPageInfoSorting>();
        }
        public FilterInfo[] Filters { get;  set; }
        public int PageIndex { get;  set; }
        public int PageSize { get;  set; }
        public List<DataPageInfoSorting> Sort { get;  set; }
    }
}