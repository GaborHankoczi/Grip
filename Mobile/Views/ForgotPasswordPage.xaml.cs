using GripMobile.ViewModels;

namespace GripMobile.Views;

public partial class ForgotPasswordPage : ContentPage
{
	public ForgotPasswordPage(ForgotPasswordPageViewModel forgotPasswordPageViewModel)
	{
		InitializeComponent();

		BindingContext = forgotPasswordPageViewModel;
	}
}