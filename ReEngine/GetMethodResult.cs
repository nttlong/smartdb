using System;
using System.Reflection;

namespace ReEngine
{
    public class GetMethodResult
    {
        public MethodInfo Method { get; set; }
        public Type Type { get; set; }
        public string Source { get; set; }
        public object StatusCode { get; set; }
        public string TypeName { get; internal set; }
        public string AppDirectory { get; internal set; }
        public string SourcePath { get; internal set; }
        public Assembly Asm { get; set; }
    }
}