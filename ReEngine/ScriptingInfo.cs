using System;

namespace ReEngine
{
    internal class ScriptingInfo
    {
        public string FullPath { get; set; }
        public Type RetType { get; set; }
        public string CachePath { get; internal set; }
    }
}