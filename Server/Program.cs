using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Grip.DAL;
using Microsoft.AspNetCore.Identity;
using Grip.DAL.Model;
using Grip.Services;
using AutoMapper;
using Grip.Providers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection");
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

/// Add Identity
builder.Services.AddIdentity<User, Role>(options => {
    /// Define username, password, email requirements
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.User.RequireUniqueEmail = true;    
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.User.AllowedUserNameCharacters= "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-. ";
    })
    .AddRoles<Role>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddTokenProvider<EmailTokenProvider<User>>(TokenOptions.DefaultProvider);

builder.Services.AddAuthentication()
    .AddIdentityServerJwt();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSwaggerGen();

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.Add(new ServiceDescriptor(typeof(StartupService), new StartupService()));

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI(o=>o.RoutePrefix = "swagger");
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapFallbackToFile("index.html");
//dbContext.Users.Where(u=>u.Roles.Any(r=>r.RoleId == "Admin")).ToList().ForEach(u=>u.Roles.Add(new IdentityUserRole<string>{RoleId = "Admin", UserId = u.Id}
/*var userManager = app.Services.GetRequiredService<UserManager<User>>();
var result = await userManager.CreateAsync(new User{UserName = "Admin", Email = "admin@localhost"}, "Admin123!");
if(result.Succeeded){
    var user = await userManager.FindByNameAsync("Admin");
    if(user == null)
        throw new Exception("User not found");
    await userManager.AddToRoleAsync(user, "Admin");
}*/


app.Run();