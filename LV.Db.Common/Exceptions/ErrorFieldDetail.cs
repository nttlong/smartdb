using System;
using System.Collections.Generic;
using System.Text;

namespace LV.Db.Common.Exceptions
{
    public class ErrorFieldDetail
    {
        public string Name { get; internal set; }
        public object DataType { get; internal set; }
        public object Value { get; internal set; }
        public int Len { get; internal set; }
        public int ExpectedLen { get; internal set; }
        public string TableName { get; internal set; }
    }
}
