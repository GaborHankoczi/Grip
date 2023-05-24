using GripMobile.ViewModel;

namespace GripMobile.View;

public partial class CurrentClassesPage : ContentPage
{
	public CurrentClassesPage(CurrentClassesPageViewModel currentClassesPageViewModel)
	{
		InitializeComponent();

		BindingContext = currentClassesPageViewModel;
	}
}