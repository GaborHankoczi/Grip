using GripMobile.ViewModel;

namespace GripMobile.View;

public partial class UserDetailsPage : ContentPage
{
	public UserDetailsPage(UserDetailsPageViewModel userDetailsPageViewModel)
	{
		InitializeComponent();

		BindingContext = userDetailsPageViewModel;
	}
}