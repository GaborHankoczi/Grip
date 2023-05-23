using GripMobile.ViewModel;

namespace GripMobile.View;

public partial class RegisterUserPage : ContentPage
{
	public RegisterUserPage(RegisterUserPageViewModel registerUserPageViewModel)
	{
		InitializeComponent();

		BindingContext = registerUserPageViewModel;
	}
}