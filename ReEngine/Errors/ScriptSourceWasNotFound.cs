using System;
using System.Runtime.Serialization;

namespace ReEngine.Errors
{
    [Serializable]
    public class ScriptSourceWasNotFound : Exception
    {
        public ScriptSourceWasNotFound()
        {
        }

        public ScriptSourceWasNotFound(string message) : base(message)
        {
        }

        public ScriptSourceWasNotFound(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ScriptSourceWasNotFound(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}