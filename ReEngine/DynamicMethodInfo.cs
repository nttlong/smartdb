using System.Reflection;

namespace ReEngine
{
    public class DynamicMethodInfo
    {
        /// <summary>
        /// The static method
        /// </summary>
        public MethodInfo Method { get; internal set; }
        //public MembershipAttribute MembershipType { get; internal set; }
        public object GetUserByToken { get; set; }
    }
}