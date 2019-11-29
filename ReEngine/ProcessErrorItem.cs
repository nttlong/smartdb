using System;
using System.Collections.Generic;
using System.Text;

namespace ReEngine
{
    public class ProcessErrorItem
    {
        public object DataItem { get; set; }
        public int Index { get; set; }
        public string Message { get; set; }
        public ProcessErrorItemEnum ErrorCode { get; set; }
        public string ErrorName { get { return this.ErrorCode.ToString(); } }

        public string[] Fields { get; set; }
    }
    public enum ProcessErrorItemEnum
    {
        System,
        MissingField
    }
}
