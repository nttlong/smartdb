namespace LV.Db.Common
{
    public class UpdateDataResult<T>:DataActionResult
    {
        public T ReturnData { get; set; }
    }
}