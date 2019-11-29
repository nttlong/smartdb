using System;
using System.Collections.Generic;

namespace ReEngine
{
    internal class SaveToJsonFileResult
    {
        public string Content { get; internal set; }
        
        public List<ResItem> Items { get; internal set; }
        public DateTime ModifiedTime { get; internal set; }
        public SortedDictionary<string, LocalizeTranslate> Data { get; internal set; }
    }
}