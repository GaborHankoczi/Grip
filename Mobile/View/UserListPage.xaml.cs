using GripMobile.ViewModel;

namespace GripMobile.View;

public partial class UserListPage : ContentPage
{
	public UserListPage(UserListPageViewModel userListPageViewModel)
	{
		InitializeComponent();

		BindingContext = userListPageViewModel;
	}
}