using GripMobile.ViewModels;

namespace GripMobile.Views;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageViewModel mainPageViewModel)
	{
		InitializeComponent();

		BindingContext = mainPageViewModel;
	}
}

