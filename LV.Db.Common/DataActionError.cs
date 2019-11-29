using System;
using System.Runtime.Serialization;

namespace LV.Db.Common
{
    [Serializable]
    public class DataActionError : Exception
    {
        public DataActionError()
        {
        }

        public DataActionError(string message) : base(message)
        {
        }

        public DataActionError(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DataActionError(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ErrorAction Detail { get;  set; }
    }
}