namespace ReEngine
{
    public class LocalizeTranslate
    {
        public string Default { get;  set; }
        public string Translate { get;  set; }
        public string Description { get; set; }
        public string GetTranslateWithEscape()
        {
            return EscapeContent(this.Translate);
        }
        public static string EscapeContent(string Content)
        {
            return Content.Replace(@"""", @"\""").Replace("'", "\'");
        }
    }
}