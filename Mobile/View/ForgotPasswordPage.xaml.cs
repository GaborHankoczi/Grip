using GripMobile.ViewModel;

namespace GripMobile.View;

public partial class ForgotPasswordPage : ContentPage
{
	public ForgotPasswordPage(ForgotPasswordPageViewModel forgotPasswordPageViewModel)
	{
		InitializeComponent();

		BindingContext = forgotPasswordPageViewModel;
	}
}