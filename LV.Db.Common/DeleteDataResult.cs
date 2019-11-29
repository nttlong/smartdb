namespace LV.Db.Common
{
    public class DeleteDataResult<T>:DataActionResult
    {
        public T ReturnData { get; set; }
    }
}