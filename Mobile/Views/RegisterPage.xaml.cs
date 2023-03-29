using GripMobile.ViewModels;

namespace GripMobile.Views;
public partial class RegisterPage : ContentPage
{
	public RegisterPage(RegisterPageViewModel registerPageViewModel)
	{
		InitializeComponent();

		BindingContext = registerPageViewModel;
	}
}