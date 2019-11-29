using System;
using System.Collections.Generic;
using System.Text;

namespace ReEngine
{
    public class TenancyManager : ITenancyProvider
    {
        public bool IsValid(string tenancy)
        {
            return true;
        }
    }
}
