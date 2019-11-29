using System;
using System.Runtime.Serialization;

namespace LV.Db.Common
{
    [Serializable]
    internal class ExcuteCommandException : Exception
    {
        public ExcuteCommandException()
        {
        }

        public ExcuteCommandException(string message) : base(message)
        {
        }

        public ExcuteCommandException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExcuteCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}