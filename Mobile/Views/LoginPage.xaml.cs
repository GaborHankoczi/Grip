using GripMobile.ViewModels;

namespace GripMobile.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginPageViewModel loginPageViewModel)
	{
		InitializeComponent();

		BindingContext = loginPageViewModel;
	}
}