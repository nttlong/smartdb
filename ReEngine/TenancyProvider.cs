using System;
using System.Collections.Generic;
using System.Text;

namespace ReEngine
{
    public class TenancyProvider
    {
        static ITenancyProvider _ins;
        static ITenancyProvider Instance
        {
            get
            {
                if (_ins == null)
                {
                    _ins = ReEngine.ProviderLoader.Get<ITenancyProvider>("config.xml", "tenancy-manager");
                }
                return _ins;
            }
        }

        public static bool IsValid(string Tenancy)
        {
            return Instance.IsValid(Tenancy);
        }
    }
}
