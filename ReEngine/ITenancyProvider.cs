using System;
using System.Collections.Generic;
using System.Text;

namespace ReEngine
{
    public interface ITenancyProvider
    {
        bool IsValid(string tenancy);
    }
}
