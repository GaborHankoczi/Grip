using GripMobile.ViewModel;

namespace GripMobile.View;

public partial class FirstLoginPage : ContentPage
{
	public FirstLoginPage(FirstLoginPageViewModel firstLoginPageViewModel)
	{
		InitializeComponent();

		BindingContext = firstLoginPageViewModel;
	}
}