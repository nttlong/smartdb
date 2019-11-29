using System.Collections;

namespace LV.Db.Common
{
    public class DataActionResult
    {
        public ErrorAction Error { get; set; }
        public object Data { get; set; }
        public int EffectedRowCount { get; set; }
        public object DataItem { get; internal set; }
        public Hashtable NewID { get; set; }
        public string ErrorMessage { get;  set; }
    }
}