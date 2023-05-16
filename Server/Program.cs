using Microsoft.AspNetCore.Authentication;
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
using Server.Bll.Services;
using Server.Bll.Providers;
using Grip.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

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
    .AddTokenProvider<EmailTokenProvider<User>>(TokenOptions.DefaultProvider);

builder.Services.AddAuthentication()
    .AddIdentityServerJwt();

builder.Services.AddSingleton<IStationTokenProvider, HMACTokenProvider>();

builder.Services.AddControllers();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddProblemDetails(options =>
{
    options.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment();
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

builder.Services.AddSignalR();

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
    o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Grip.xml"));
});

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);





var app = builder.Build();

app.UseProblemDetails();

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
    });
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<DeChunkingMiddleware>();
app.UseMiddleware<ApiKeyValidationMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapHub<StationHub>("/hubs/station");

using var scope = app.Services.CreateScope();
using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
using var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<User>>();

var hostName = builder.Configuration.GetValue<string>("Host") ?? throw new Exception("HostName not found in configuration");

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
/*
var hmacProvieder = new HMACTokenProvider();
var hmacToken = hmacProvieder.GenerateToken("a","1_1681290689_1270216262");
logger.LogInformation($"Generated token: {hmacToken}");
*/

app.Run();