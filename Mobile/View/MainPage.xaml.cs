using GripMobile.ViewModel;

namespace GripMobile.View;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageViewModel mainPageViewModel)
	{
		InitializeComponent();

		BindingContext = mainPageViewModel;
	}
}

