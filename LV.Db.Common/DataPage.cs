using System.Collections.Generic;

namespace LV.Db.Common
{
    public class DataPage<T>
    {
        public int TotalItems { get;  set; }
        public int PageSize { get;  set; }
        public int PageIndex { get;  set; }
        public int TotalPages { get;  set; }
        public List<T> Items { get; set; }
    }
    
}