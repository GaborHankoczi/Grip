using GripMobile.ViewModels;

namespace GripMobile.Views;

public partial class UserDetailsPage : ContentPage
{
	public UserDetailsPage(UserDetailsPageViewModel userDetailsPageViewModel)
	{
		InitializeComponent();

		BindingContext = userDetailsPageViewModel;
	}
}