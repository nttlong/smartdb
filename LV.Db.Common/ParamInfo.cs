using System;

namespace LV.Db.Common
{
    internal class ParamInfo
    {
        internal Type type1;
        internal Type type2;

        public ParamInfo()
        {
        }

        public string Name { get; set; }
        public object Value { get; set; }
        public int Index { get; set; }
        public Type DataType { get; set; }
    }
}