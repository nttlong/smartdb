using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace ReEngine
{
    public class DynamicTemplateInfo
    {
        public string Content { get; internal set; }
        public Type InstanceType { get; internal set; }
        public MethodInfo GetModelMethod { get; internal set; }
        public bool FileNotFound { get; internal set; }

        internal Context GetContext(HttpContext context)
        {
            return new Context
            {

            };
        }
    }
}