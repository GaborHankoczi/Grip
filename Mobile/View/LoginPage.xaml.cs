using GripMobile.ViewModel;

namespace GripMobile.View;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginPageViewModel loginPageViewModel)
	{
		InitializeComponent();

		BindingContext = loginPageViewModel;
	}

    //Override needed because of the physical back buttons
	protected override bool OnBackButtonPressed()
    {
        return true;
    }
}