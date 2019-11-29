using System;
using System.Runtime.Serialization;

namespace ReEngine.Errors
{
    [Serializable]
    public class ScriptSourceError : Exception
    {
        public ScriptSourceError()
        {
        }

        public ScriptSourceError(string message) : base(message)
        {
        }

        public ScriptSourceError(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ScriptSourceError(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}