using System;
using System.Collections.Generic;
using System.Text;

namespace LV.Db.Common
{
    public class InsertDataResult<T>:DataActionResult
    {
        public T  ReturnData {get;set;}
        public object RecordId { get; set; }
        
    }
}
