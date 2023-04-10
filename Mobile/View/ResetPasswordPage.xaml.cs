using GripMobile.ViewModel;

namespace GripMobile.View;

public partial class ResetPasswordPage : ContentPage
{
	public ResetPasswordPage(ResetPasswordPageViewModel resetPasswordPageViewModel)
	{
		InitializeComponent();

		BindingContext = resetPasswordPageViewModel;
	}
}