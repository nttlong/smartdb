using LV.Db.Common;
using Npgsql;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace LV.Db.PostgreSQL
{
    public static class Utils
    {
        /// <summary>
        /// Process exception when excute Insert, Update, Delete
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <returns>ErrorAction</returns>
        //[System.Diagnostics.DebuggerStepThrough]
        public static ErrorAction  ProcessException(NpgsqlException ex)
        {
            /*
             * Yêu cầu sửa lại như sau:
             * Các lỗi kg xử lý được thì phải throw exception
             * Đoạn code này chỉ mới bắt 2 lỗi 23505,23502 còn các lỗi khác thì kg bắt
             */
            ErrorAction errorAction = new ErrorAction();
            //Duplicate unique fields case
            if (((PostgresException)ex).SqlState == "23505")
            {
                string[] fields = new string[] { };
                string detail = ((PostgresException)ex).Detail;
                string leftStr = detail.Split('=')[0];
                string strFieldNames = leftStr.Split('(')[1].Split(')')[0];
                string[] str = strFieldNames.Split(',');
                fields = str.Select(p => p.Replace(@"""", "")?.Replace(@"\", "")).ToArray();
                errorAction.ErrorType = DataActionErrorTypeEnum.DuplicateData;
                errorAction.Fields = fields;
            }
            //Require fields are null
            else if (((PostgresException)ex).SqlState == "23502")
            {
                string[] fields = new string[1];
                string message = ((PostgresException)ex).Message;                
                fields[0] = Regex.Split(message, @"\""")[1];                
                errorAction.ErrorType = DataActionErrorTypeEnum.MissingFields;
                errorAction.Fields = fields;                
            }
            //Foreign key exception
            else if (((PostgresException)ex).SqlState == "23503")
            {
                string[] fields = new string[1];
                string[] tables = new string[1];
                string detail = ((PostgresException)ex).Detail;
                string leftStr = detail.Split('=')[0];
                string tableName = Regex.Split(detail, @"""")[1].Replace(@"""", "");
                string fieldName = leftStr.Split('(')[1].Split(')')[0];
                fields[0] = fieldName;
                tables[0] = tableName;                
                errorAction.ErrorType = DataActionErrorTypeEnum.ForeignKey;
                errorAction.Fields = fields;
                errorAction.RefTables = tables;
            }
            else
            {
                throw (ex);
            }
            return errorAction;
        }

        internal static bool IsAnonymousType(Type type)
        {
            if (type.IsGenericType)
            {
                var d = type.GetGenericTypeDefinition();
                if (d.IsClass && d.IsSealed && d.Attributes.HasFlag(TypeAttributes.NotPublic))
                {
                    var attributes = d.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
                    if (attributes != null && attributes.Length > 0)
                    {
                        //WOW! We have an anonymous type!!!
                        return true;
                    }
                }
            }
            return false;

        }
    }
}
