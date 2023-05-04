using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Grip.DAL;
using Microsoft.AspNetCore.Identity;
using Grip.DAL.Model;
using Grip.Services;
using Grip.Providers;
using AutoMapper;
using System.Security.Cryptography.X509Certificates;
using Grip.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection");
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Load certificate on deployed instances
if(builder.Configuration.GetValue<bool>("Certificate:LoadCertificate")){
    if(!builder.Environment.IsDevelopment()){
        builder.WebHost.ConfigureKestrel(options=>{
            options.ConfigureHttpsDefaults(o=>{
                string pemPath = builder.Configuration.GetValue<string>("Certificate:FullChainPath") ?? throw new Exception("Certificate:FullChainPath not found in configuration");
                string keyPath = builder.Configuration.GetValue<string>("Certificate:KeyPath")  ?? throw new Exception("Certificate:FullChainPath not found in configuration");
                var pem = File.ReadAllText(pemPath);
                var key = File.ReadAllText(keyPath);
                var x509 = X509Certificate2.CreateFromPem(pem, key);
                o.ServerCertificate = x509;
            });
        });
    }
}

/// Add Identity
builder.Services.AddIdentity<User, Role>(options => {
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

builder.Services.AddSingleton<IStationTokenProvider,HMACTokenProvider>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSingleton<EmailService>();

builder.Services.AddSwaggerGen();

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);





var app = builder.Build();


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



if(builder.Configuration.GetValue<bool>("UseSwagger")){
    app.UseSwagger();
    app.UseSwaggerUI(o=>{
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
app.MapRazorPages();

app.MapFallbackToFile("index.html");

using var scope = app.Services.CreateScope();
using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
using var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<User>>();

var hostName = builder.Configuration.GetValue<string>("Host") ?? throw new Exception("HostName not found in configuration");

// Create admin user if not exists
if(!(await userManager.GetUsersInRoleAsync("Admin")).Any()){
    logger.LogInformation("No admin user found, creating one");
    var result = await userManager.CreateAsync(new User{UserName = "Admin", Email = $"admin@{hostName}"}, "Admin123!");
    if(result.Succeeded){
        var user = await userManager.FindByNameAsync("Admin") ?? throw new Exception("User not found");
        if(!(roleManager.Roles.Where(r=>r.Name == "Admin")).Any()){
            await roleManager.CreateAsync(new Role{Name = "Admin", NormalizedName = "ADMIN"});
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