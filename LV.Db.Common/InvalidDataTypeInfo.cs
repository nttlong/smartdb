using System;

namespace LV.Db.Common
{
    internal class InvalidDataTypeInfo
    {
        public InvalidDataTypeInfo()
        {
        }

        public string FieldName { get; internal set; }
        public Type ExpectedDataType { get; internal set; }
        public Type ReceiveDataType { get; internal set; }
    }
}