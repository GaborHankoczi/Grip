using GripMobile.ViewModel;

namespace GripMobile.View;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginPageViewModel loginPageViewModel)
	{
		InitializeComponent();

		BindingContext = loginPageViewModel;
	}
}