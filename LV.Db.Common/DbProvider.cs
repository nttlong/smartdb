using System;
using System.Reflection;

namespace LV.Db.Common
{
    public class DbProvider
    {
        public Type DbProviderType { get; internal set; }
        public ICompiler Compiler { get; internal set; }
        public Assembly Asm { get; internal set; }
    }
}