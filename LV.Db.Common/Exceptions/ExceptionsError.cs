using System;
using System.Runtime.Serialization;

namespace LV.Db.Common.Exceptions
{
    [Serializable]
    public class Error : Exception
    {
        private ErrorType _errorType;
        private string _errorTypeName;

        public Error()
        {
        }

        public Error(string message) : base(message)
        {
        }

        public Error(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected Error(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ErrorType ErrorType
        {
            get
            {
                return _errorType;
            }
            set
            {
                _errorType = value;
                _errorTypeName = value.ToString();
            }
        }
        public string ErrorTypeName
        {
            get
            {
                return _errorTypeName;
            }
        }

        public string[] Fields { get; internal set; }
        public ErrrorDetail Detail { get; internal set; }
        public string[] RefTables { get; internal set; }
        internal InvalidDataTypeInfo[] DetailInvalidataTypeFields { get; set; }
    }
    public class ErrrorDetail
    {
        public ErrorFieldDetail[] Fields { get; internal set; }
    }
    public enum ErrorType
    {
        None,
        /// <summary>
        /// Chieu dai vuot qua chieu dai trong database
        /// </summary>
        ValueIsExceed,
        /// <summary>
        /// Thieu cac field ma yeu cau phai nhap
        /// </summary>
        MissFields,
        DuplicatetData,
        ForeignKey,
        InvalidDataType,
    }
}