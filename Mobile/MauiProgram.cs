using GripMobile.Services;
using GripMobile.ViewModels;
using GripMobile.Views;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

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
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MainPageViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginPageViewModel>();
        builder.Services.AddTransient<LoginService>();

        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<RegisterPageViewModel>();

        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<ForgotPasswordPageViewModel>();

        builder.Services.AddTransient<UserDetailsPage>();
        builder.Services.AddTransient<UserDetailsPageViewModel>();

        return builder.Build();
    }
}