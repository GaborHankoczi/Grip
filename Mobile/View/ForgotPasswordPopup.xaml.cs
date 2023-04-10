using CommunityToolkit.Maui.Views;
using GripMobile.ViewModel;

namespace GripMobile.View;

public partial class ForgotPasswordPopup : Popup
{
	public ForgotPasswordPopup(ForgotPasswordPopupViewModel forgotPasswordPopupViewModel)
	{
		InitializeComponent();

		BindingContext = forgotPasswordPopupViewModel;
	}

	/// <summary>
	/// Method <c>ClosePopup</c> closes the active popup.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public void ClosePopup(object sender, EventArgs e) => Close();

}