using System;
using System.Runtime.Serialization;

namespace LV.Db.Common.Exceptions
{
    [Serializable]
    public class DataInputError : Exception
    {
        public DataInputError()
        {
        }

        public DataInputError(string message) : base(message)
        {
        }

        public DataInputError(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DataInputError(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}