using System;
using System.Collections.Generic;
using System.Text;

namespace ReEngine
{
    public interface IAuth
    {
        void Validate(Context Context, Requires RequirePrivileges);
        LoginUserInfo GetUser(Context Context);
        string[] GetPrivileges(Context Context);
    }
}
