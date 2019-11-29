namespace ReEngine
{
    public class WebApplication
    {
        public ApplicationInfo[] Apps { get; set; }
        public string AppsDirectory { get; set; }
        public string StaticDirectory { get; set; }
        public ServerLocalizeInfo LocalizeDefault { get;  set; }
    }
}