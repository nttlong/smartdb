using System;

namespace ReEngine
{
    public class PrivilegesAttribute : Attribute
    {
        public bool IsPublic { get; set; }
        public Requires Require { get; set; }
        public string Special { get; set; }
    }
    public enum Requires
    {
        None = 0b0000000000000000,
        View = 0b0000000000000001,
        Edit = 0b0000000000000010,

        Update = 0b0000000000000100,
        AddNew = 0b000000000001000,
        Delete = 0b0000000000010000,
        Export = 0b0000000000100000,
        Import = 0b0000000001000000,
        Upload = 0b0000000010000000,
        Special = -1
    }
}