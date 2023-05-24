using GripMobile.ViewModel;

namespace GripMobile.View;

public partial class GiveExemptPage : ContentPage
{
	public GiveExemptPage(GiveExemptPageViewModel giveExemptPageViewModel)
	{
		InitializeComponent();

		BindingContext = giveExemptPageViewModel;
	}
}