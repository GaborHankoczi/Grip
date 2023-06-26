using GripMobile.Service.Auth;
using GripMobile.View;
using IdentityModel.OidcClient;

namespace GripMobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
        InitializeComponent();

        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(ForgotPasswordPopup), typeof(ForgotPasswordPopup));
        Routing.RegisterRoute(nameof(FirstLoginPage), typeof(FirstLoginPage));
        Routing.RegisterRoute(nameof(ResetPasswordPage), typeof(ResetPasswordPage));
        Routing.RegisterRoute(nameof(NFCPage), typeof(NFCPage));
        Routing.RegisterRoute(nameof(CurrentClassesPage), typeof(CurrentClassesPage));
        Routing.RegisterRoute(nameof(ExemptPage), typeof(ExemptPage));
        Routing.RegisterRoute(nameof(UserListPage), typeof(UserListPage));
        Routing.RegisterRoute(nameof(RegisterUserPage), typeof(RegisterUserPage));
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        var oidcClient = this.Handler.MauiContext.Services.GetService<OidcClient>();

        Preferences.Remove(OidcConsts.AccessTokenKeyName);
        var idToken = Preferences.Get(OidcConsts.IdTokenKeyName, null);
        Preferences.Remove(OidcConsts.IdTokenKeyName);
        Preferences.Remove(OidcConsts.RefreshTokenKeyName);
        LogoutResult logoutResult = await oidcClient.LogoutAsync(new LogoutRequest() { IdTokenHint = idToken });
        if (logoutResult.IsError)
        {
            await DisplayAlert("Logout",$"Error: {logoutResult.Error}","Ok");
        }
        else
        {
            await DisplayAlert("Logout", $"Sikeresen kijelentkezett", "Ok");
        }
        await Shell.Current.GoToAsync($"//MainPage");
    }
}
