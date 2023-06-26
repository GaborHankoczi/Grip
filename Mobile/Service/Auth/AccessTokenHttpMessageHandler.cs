using IdentityModel.Client;
using IdentityModel.OidcClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GripMobile.Service.Auth
{
    public class AccessTokenHttpMessageHandler : DelegatingHandler
    {
        protected OidcClient OidcClient { get; }

        public AccessTokenHttpMessageHandler(OidcClient oidcClient) : base(new HttpClientHandler())
        {
            OidcClient = oidcClient;
        }

        private bool TryGetValue(string key, out string value)
        {
            value = Microsoft.Maui.Storage.Preferences.Get(key, string.Empty);
            return !string.IsNullOrEmpty(value);
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (TryGetValue(OidcConsts.AccessTokenKeyName, out string currentTokenValue) && currentTokenValue != null)
            {
                request.SetBearerToken(currentTokenValue?.ToString());
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (TryGetValue(OidcConsts.RefreshTokenKeyName, out string refreshTokenValue) && refreshTokenValue != null)
                {
                    var refreshResult = await OidcClient.RefreshTokenAsync(refreshTokenValue?.ToString());

                    Preferences.Set(OidcConsts.AccessTokenKeyName, refreshResult.AccessToken);
                    Preferences.Set(OidcConsts.RefreshTokenKeyName, refreshResult.RefreshToken);

                    request.SetBearerToken(refreshResult.AccessToken);

                    return await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    var result = await OidcClient.LoginAsync(new LoginRequest());
                    request.SetBearerToken(result.AccessToken);

                    Preferences.Set(OidcConsts.AccessTokenKeyName, result.AccessToken);
                    Preferences.Set(OidcConsts.AccessTokenKeyName, result.RefreshToken);
                    request.SetBearerToken(result.AccessToken);

                    return await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }
    }
}
