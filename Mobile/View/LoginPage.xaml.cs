using GripMobile.Service.Auth;
using GripMobile.ViewModel;
using IdentityModel.OidcClient;

namespace GripMobile.View;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginPageViewModel loginPageViewModel)
	{
		InitializeComponent();
		BindingContext = loginPageViewModel;
	}
}