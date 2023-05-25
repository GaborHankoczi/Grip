using GripMobile.Service;
using GripMobile.ViewModel;
using GripMobile.View;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using GripMobile.Model.Api;

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

        builder.Services.AddTransient<Client>();

        return builder.Build();
    }
}