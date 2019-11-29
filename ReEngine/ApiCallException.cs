using System;
using System.Runtime.Serialization;

namespace ReEngine
{
    [Serializable]
    public class ApiCallException : Exception
    {
        public ApiErrorTypeEnum ErrorType { get; set; }
        public string ErrorTypeName { get { return this.ErrorType.ToString(); } }
        public ApiCallException()
        {
        }

        public ApiCallException(ApiErrorTypeEnum errorType, string message) : base(message)
        {
            this.ErrorType = errorType;
        }

       
    }
   
    public enum ApiErrorTypeEnum
    {
        None = 0,
        AccessDeny = 1,
        System = 2
    }
}