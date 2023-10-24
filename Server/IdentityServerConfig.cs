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
            new ApiScope("everlink"),
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
                // Everlink machine to machine client
                new Client
                {
                    ClientId = "everlink.m2m.client",
                    ClientName = "Everlink machine to machine client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret(Configuration["Oauth2:Clients:everlinkM2M:ClientSecret"].Sha256()) },
                    Claims = new List<ClientClaim> { new ClientClaim("EverlinkAccess", "true") },
                    AllowedScopes = { "everlink" }
                },

                // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "interactive",
                    ClientSecrets = { new Secret(Configuration["Oauth2:Clients:interactive:ClientSecret"].Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = redurectURIs,
                    FrontChannelLogoutUri = host+"/signout-oidc",
                    PostLogoutRedirectUris = { host + "/signout-callback-oidc", "grip://signout" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "roles", "everlink" }
                },
            };
        }
    }
}