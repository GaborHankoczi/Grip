using GripMobile.Service;
using GripMobile.ViewModel;
using GripMobile.View;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using GripMobile.Model.Api;
using IdentityModel.OidcClient;
using GripMobile.Service.Auth;

namespace GripMobile;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        }).UseMauiCommunityToolkit();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<MainPageViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginPageViewModel>();
        builder.Services.AddTransient<LoginService>();

        builder.Services.AddTransient<ForgotPasswordPopup>();
        builder.Services.AddTransient<ForgotPasswordPopupViewModel>();
        builder.Services.AddTransient<ForgotPasswordService>();

        builder.Services.AddTransient<FirstLoginPage>();
        builder.Services.AddTransient<FirstLoginPageViewModel>();
        builder.Services.AddTransient<FirstLoginService>();

        builder.Services.AddTransient<ResetPasswordPage>();
        builder.Services.AddTransient<ResetPasswordPageViewModel>();
        builder.Services.AddTransient<ResetPasswordService>();

        builder.Services.AddTransient<NFCPage>();
        builder.Services.AddTransient<NFCPageViewModel>();
        builder.Services.AddTransient<NFCService>();

        builder.Services.AddTransient<CurrentClassesPage>();
        builder.Services.AddTransient<CurrentClassesPageViewModel>();
        builder.Services.AddTransient<CurrentClassesService>();

        builder.Services.AddTransient<ExemptPage>();
        builder.Services.AddTransient<ExemptPageViewModel>();
        builder.Services.AddTransient<ExemptGetService>();

        builder.Services.AddTransient<UserListPage>();
        builder.Services.AddTransient<UserListPageViewModel>();
        builder.Services.AddTransient<UserListService>();

        builder.Services.AddTransient<RegisterUserPage>();
        builder.Services.AddTransient<RegisterUserPageViewModel>();
        builder.Services.AddTransient<RegisterUserService>();

        builder.Services.AddTransient<CreateClassPage>();
        builder.Services.AddTransient<CreateClassViewModel>();


        builder.Services.AddTransient<CreateExemptPage>();
        builder.Services.AddTransient<CreateExemptViewModel>();


        builder.Services.AddTransient<CreateGroupPage>();
        builder.Services.AddTransient<CreateGroupViewModel>();

        builder.Services.AddTransient<AssignToGroupPage>();
        builder.Services.AddTransient<AssignToGroupViewModel>();

        builder.Services.AddTransient<StationWatchPage>();
        builder.Services.AddTransient<StationWatchViewModel>();

        builder.Services.AddTransient<WebAuthenticatorBrowser>();
        builder.Services.AddTransient<OidcClient>(sp =>
            new OidcClient(new OidcClientOptions
            {
                // Use your own ngrok url:
                //Authority = "https://nloc.duckdns.org:8025",
                Authority = "https://nloc.duckdns.org:8025",
                ClientId = "interactive",
                RedirectUri = "grip://",
                Scope = "openid profile roles offline_access",
                ClientSecret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0",
                PostLogoutRedirectUri = "grip://signout",
                Browser = sp.GetRequiredService<WebAuthenticatorBrowser>(),
            })
        );

        builder.Services.AddSingleton<AccessTokenHttpMessageHandler>();
        builder.Services.AddTransient<HttpClient>(sp =>
            new HttpClient(sp.GetRequiredService<AccessTokenHttpMessageHandler>())
            {
                BaseAddress = new Uri("https://nloc.duckdns.org:8025")
            });
        builder.Services.AddTransient<ApiClient>();

        return builder.Build();
    }
}