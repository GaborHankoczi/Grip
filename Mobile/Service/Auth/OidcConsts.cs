using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GripMobile.Service.Auth
{
    public static class OidcConsts
    {
        internal const string AccessTokenKeyName = "__access_token";
        internal const string RefreshTokenKeyName = "__refresh_token";
        internal const string IdTokenKeyName = "__id_token";
    }
}
