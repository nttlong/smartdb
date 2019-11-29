namespace LV.Db.Common
{
    public class ErrorAction
    {
        public DataActionErrorTypeEnum ErrorType { get; set; }
        public string[] Fields { get; set; }
        public string[] RefTables { get; set; }
        internal InvalidDataTypeInfo[] InvalidDataTypeFields { get; set; }
    }
    public enum DataActionErrorTypeEnum
    {
        //success
        None,
        // missing fields
        MissingFields,
        // duplicate data
        DuplicateData,
        // foreign key error
        ForeignKey,
        InvalidDataType,
        ExceedSize,
    }
}