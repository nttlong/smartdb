using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LV.Db.Common.Exceptions
{
    [Serializable]
    internal class ExcutationError : Exception
    {
        public ExcutationError()
        {
        }

        public ExcutationError(string message) : base(message)
        {
        }

        public ExcutationError(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExcutationError(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Sql { get; set; }
        public string SqlWithParams { get; set; }
        public List<object> Params { get; set; }
    }
}