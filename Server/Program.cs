using Microsoft.EntityFrameworkCore;
using Grip.DAL;
using Microsoft.AspNetCore.Identity;
using Grip.DAL.Model;
using AutoMapper;
using System.Security.Cryptography.X509Certificates;
using Grip.Middleware;
using Grip.Bll.Services;
using Grip.Bll.Services.Interfaces;
using Hellang.Middleware.ProblemDetails;
using Grip.Bll.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Grip.Bll.Providers;
using Grip.Api.Hubs;
using Grip;
using Duende.IdentityServer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Grip.Api.Middleware;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.Net.Http.Headers;
using Duende.IdentityServer.AspNetIdentity;
using Grip.Bll.Everlink;


ILogger<User> logger = null;

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration["Proxy:UseProxy"] == "true")
{
    System.Net.WebRequest.DefaultWebProxy = new System.Net.WebProxy(builder.Configuration["Proxy:ProxyUrl"]);
}

builder.Services.AddRazorPages();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection");
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Load certificate on deployed instances
if (builder.Configuration.GetValue<bool>("Certificate:LoadCertificate"))
{
    if (!builder.Environment.IsDevelopment())
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ConfigureHttpsDefaults(o =>
            {
                string pemPath = builder.Configuration.GetValue<string>("Certificate:FullChainPath") ?? throw new Exception("Certificate:FullChainPath not found in configuration");
                string keyPath = builder.Configuration.GetValue<string>("Certificate:KeyPath") ?? throw new Exception("Certificate:FullChainPath not found in configuration");
                var pem = File.ReadAllText(pemPath);
                var key = File.ReadAllText(keyPath);
                var x509 = X509Certificate2.CreateFromPem(pem, key);
                o.ServerCertificate = x509;
            });
        });
    }
}
builder.Services.Configure<SecurityStampValidatorOptions>(opts =>
{
    opts.OnRefreshingPrincipal = SecurityStampValidatorCallback.UpdatePrincipal;
});
/// Add Identity
builder.Services.AddIdentity<User, Role>(options =>
{
    /// Define username, password, email requirements
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-. áéíőúűóüöÁÉÍŐÚŰÓÜÖ";
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
})
    .AddRoles<Role>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    //.AddDefaultTokenProviders()
    .AddTokenProvider<EmailTokenProvider<User>>(TokenOptions.DefaultProvider);


var identityServerConfig = new IdentityServerConfig(builder.Configuration);
// IdentityServer
builder.Services.AddIdentityServer(
    options =>
    {
        options.IssuerUri = builder.Configuration.GetValue<string>("Jwt:Issuer");
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;
        options.EmitStaticAudienceClaim = true;
        options.UserInteraction.LoginUrl = "/Account/Login";
        options.UserInteraction.LoginReturnUrlParameter = "returnUrl";
        options.UserInteraction.AllowOriginInReturnUrl = true;
        options.Logging.UnhandledExceptionLoggingFilter = (category, level) => true;
    })
    /*.AddConfigurationStore<ApplicationDbContext>()
    .AddOperationalStore<ApplicationDbContext>();*/
    .AddInMemoryIdentityResources(identityServerConfig.IdentityResources)
    .AddInMemoryApiScopes(identityServerConfig.ApiScopes)
    .AddInMemoryClients(identityServerConfig.Clients)
    .AddAspNetIdentity<User>()
    .AddProfileService<ProfileService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var issuer = builder.Configuration.GetValue<string>("Jwt:Issuer");

builder.Services.AddAuthentication(options =>
{
    /*options.DefaultAuthenticateScheme = "JWT_OR_COOKIE";
    options.DefaultChallengeScheme = "JWT_OR_COOKIE";
    options.DefaultScheme = "JWT_OR_COOKIE";*/
})
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
    {
        o.IncludeErrorDetails = true;
        o.RequireHttpsMetadata = false;
        // TODO don't use this in production
        o.BackchannelHttpHandler = new HttpClientHandler { ServerCertificateCustomValidationCallback = delegate { return true; } };
        o.Authority = builder.Configuration.GetValue<string>("Jwt:Issuer");
        o.TokenValidationParameters = new TokenValidationParameters
        {

            ValidateAudience = false,
            ValidateIssuerSigningKey = false
        };
    })
.AddGoogle(options =>
        {
            options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            options.ClientId = "172827484077-ek42qph1murp98dq15j6sepeq0tutp2n.apps.googleusercontent.com";
            options.ClientSecret = "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";
        })
    /*.AddPolicyScheme("JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
    {
        // runs on each request
        options.ForwardDefaultSelector = context =>
        {
            // filter by auth type
            string authorization = context.Request.Headers[HeaderNames.Authorization];
            if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                return "Bearer";

            // otherwise always check for cookie auth
            return "Identity.Application";
        };
    })*/;


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
    {
        policy.RequireRole("Admin");
    });
});
builder.Services.AddSingleton<IStationTokenProvider, HMACTokenProvider>();
builder.Services.AddSingleton<IEverlinkAdapterService, EverlinkAdapterService>();

builder.Services.AddControllers();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();



builder.Services.AddProblemDetails(options =>
{
    options.IncludeExceptionDetails = (ctx, ex) => true;//builder.Environment.IsDevelopment() || builder.Environment.ToString() == "TestEnvironment";
    options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);
    options.Map<NotFoundException>(ex =>
        new ProblemDetails
        {
            Title = String.IsNullOrEmpty(ex.Message) ? "Not found" : ex.Message,
            Status = StatusCodes.Status404NotFound,
            Detail = ex.Message
        });
    options.Map<BadRequestException>(ex =>
        new ProblemDetails
        {
            Title = String.IsNullOrEmpty(ex.Message) ? "Bad request" : ex.Message,
            Status = StatusCodes.Status400BadRequest,
            Detail = ex.Message
        });
    options.Map<UnauthorizedException>(ex =>
    new ProblemDetails
    {
        Title = String.IsNullOrEmpty(ex.Message) ? "Unauthorized" : ex.Message,
        Status = StatusCodes.Status401Unauthorized,
        Detail = ex.Message
    });
    options.MapToStatusCode<DbConcurrencyException>(StatusCodes.Status409Conflict);
});
builder.Services.AddSignalR(
    options =>
    {
        options.MaximumReceiveMessageSize = 1024 * 1024 * 200; // 200 MB
        if(builder.Environment.IsDevelopment()|| builder.Environment.EnvironmentName == "TestEnvironment")
            options.EnableDetailedErrors = true;
    }
);

builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IExemptService, ExemptService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IPassiveTagService, PassiveTagService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICurrentTimeProvider, CurrentTimeProvider>();

builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "Grip API", Version = "v1", Description = "Api for Grip attendance system" });
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows()
        {
            AuthorizationCode = new OpenApiOAuthFlow()
            {

                AuthorizationUrl = new Uri(builder.Configuration.GetValue<string>("Host") + "/connect/authorize"),
                TokenUrl = new Uri(builder.Configuration.GetValue<string>("Host") + "/connect/token"),
                Scopes = new Dictionary<string, string>
            {
                { "openid", "Identity" },
                { "profile", "Profile" },
                { "roles", "Roles" },
                { "offline_access", "Refresh token" },
            }
            }
        }
    });
    o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Grip.xml"));
    o.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme()
                    {
                        Reference = new OpenApiReference()
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "Bearer",
                        Type = SecuritySchemeType.Http,
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    }, new List<string>()
                }
            });
});

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();



#region Pipeline

var app = builder.Build();

app.MapRazorPages()
    .RequireAuthorization();
app.UseProblemDetails();

app.UseCookiePolicy(new CookiePolicyOptions
{
    HttpOnly = HttpOnlyPolicy.None,
    MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None,
    Secure = CookieSecurePolicy.Always
});

//Forwarding headers for external login
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using var currentScope = app.Services.CreateScope();
// Auto migrate database on developement and te
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "TestEnvironment")
{
    using var context = currentScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}


if (builder.Configuration.GetValue<bool>("UseSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.RoutePrefix = "swagger";
        o.ConfigObject.AdditionalItems.Add("syntaxHighlight", false);
        o.OAuthConfigObject.ClientId = "interactive";
        o.OAuthConfigObject.ClientSecret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0";
        o.OAuthUseBasicAuthenticationWithAccessCodeGrant();
        o.OAuthUsePkce();
    });
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();



app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();


app.UseMiddleware<DeChunkingMiddleware>();
app.UseMiddleware<ApiKeyValidationMiddleware>();
app.UseMiddleware<OptionsMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.MapHealthChecks("/healthz");
app.MapHub<StationHub>("/hubs/station");
app.MapHub<EverlinkAdapterHub>("/hubs/everlink");

using var scope = app.Services.CreateScope();
using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
using var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
logger = scope.ServiceProvider.GetRequiredService<ILogger<User>>();

var hostName = builder.Configuration.GetValue<string>("HostName") ?? throw new Exception("HostName not found in configuration");

var roles = new[] { "Student", "Teacher", "Admin", "Doorman" };

foreach (var role in roles)
{
    if ((await roleManager.FindByNameAsync(role)) == null)
    {
        logger.LogInformation($"No {role} role found, creating it");
        await roleManager.CreateAsync(new Role { Name = role, NormalizedName = role.ToUpper() });
    }
}

// Create admin user if not exists
if (!(await userManager.GetUsersInRoleAsync("Admin")).Any())
{
    logger.LogInformation("No admin user found, creating one");
    var result = await userManager.CreateAsync(new User { UserName = "Admin", Email = $"admin@{hostName}" }, "Admin123!");
    if (result.Succeeded)
    {
        var user = await userManager.FindByNameAsync("Admin") ?? throw new Exception("User not found");
        if (!(roleManager.Roles.Where(r => r.Name == "Admin")).Any())
        {
            await roleManager.CreateAsync(new Role { Name = "Admin", NormalizedName = "ADMIN" });
            logger.LogInformation("Admin role created");
        }
        await userManager.AddToRoleAsync(user, "Admin");
        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);
    }
}

#endregion

/*string key = "6a9a7384-a972-4520-bbcd-68a1350cabac";//dbContext.Stations.Find(1).SecretKey;
var hmacProvieder = new HMACTokenProvider();
var hmacToken = hmacProvieder.GenerateToken(key, "1_1685086218_1270216262");
logger.LogInformation($"Generated token: {hmacToken}");*/

app.Run();