using GripMobile.ViewModel;

namespace GripMobile.View;

public partial class NFCPage : ContentPage
{
	public NFCPage(NFCPageViewModel nFCPageViewModel)
	{
		InitializeComponent();

		BindingContext = nFCPageViewModel;
	}
}