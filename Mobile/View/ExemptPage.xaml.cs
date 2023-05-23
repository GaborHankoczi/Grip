using GripMobile.ViewModel;

namespace GripMobile.View;

public partial class ExemptPage : ContentPage
{
	public ExemptPage(ExemptPageViewModel exemptPageViewModel)
	{
		InitializeComponent();

		BindingContext = exemptPageViewModel;
	}
}