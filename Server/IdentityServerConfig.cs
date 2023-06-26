using Duende.IdentityServer.Models;
using IdentityModel;

namespace Grip;

public class IdentityServerConfig
{
    private readonly IConfiguration Configuration;
    public IdentityServerConfig(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public IEnumerable<IdentityResource> IdentityResources
    {
        get =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource("roles", "Your role(s)", new List<string> { JwtClaimTypes.Role })
        };
    }
    public IEnumerable<ApiScope> ApiScopes
    {
        get =>
        new ApiScope[]
        {
            new ApiScope("scope1"),
            new ApiScope("scope2"),
        };
    }

    public IEnumerable<Client> Clients
    {
        get
        {
            var host = Configuration.GetValue<string>("Host");
            var clients = new List<Client>();
            var redurectURIs = new List<string>();
            if (Configuration.GetValue<bool>("UseSwagger"))
                redurectURIs.Add(host + "/swagger/oauth2-redirect.html");
            if (Configuration.GetValue<bool>("AllowPostmanOauth2RedirectUrl"))
                redurectURIs.Add("https://oauth.pstmn.io/v1/callback");

            redurectURIs.Add(host + "/signin-oidc");
            redurectURIs.Add("grip://"); // Android mobile client

            return new Client[]
            {
                // m2m client credentials flow client
                /*new Client
                {
                    ClientId = "m2m.client",
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                    AllowedScopes = { "scope1" }
                },*/

                // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "interactive",
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = redurectURIs,
                    FrontChannelLogoutUri = host+"/signout-oidc",
                    PostLogoutRedirectUris = { host + "/signout-callback-oidc", "grip://signout" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "scope2", "roles" }
                },
            };
        }
    }
}