using GripMobile.Service;
using GripMobile.ViewModel;
using GripMobile.View;
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
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<MainPageViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginPageViewModel>();
        builder.Services.AddTransient<LoginService>();

        builder.Services.AddTransient<ForgotPasswordPopup>();
        builder.Services.AddTransient<ForgotPasswordPopupViewModel>();
        builder.Services.AddTransient<ForgotPasswordService>();

        builder.Services.AddTransient<UserDetailsPage>();
        builder.Services.AddTransient<UserDetailsPageViewModel>();

        builder.Services.AddTransient<FirstLoginPage>();
        builder.Services.AddTransient<FirstLoginPageViewModel>();
        builder.Services.AddTransient<FirstLoginService>();

        builder.Services.AddTransient<ResetPasswordPage>();
        builder.Services.AddTransient<ResetPasswordPageViewModel>();
        builder.Services.AddTransient<ResetPasswordService>();

        builder.Services.AddTransient<NFCPage>();
        builder.Services.AddTransient<NFCPageViewModel>();

        return builder.Build();
    }
}