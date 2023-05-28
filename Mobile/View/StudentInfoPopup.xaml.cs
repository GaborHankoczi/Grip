using CommunityToolkit.Maui.Views;
using GripMobile.ViewModel;

namespace GripMobile.View;

public partial class StudentInfoPopup : Popup
{
	public StudentInfoPopup(StudentInfoViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

	/// <summary>
	/// Method <c>ClosePopup</c> closes the active popup.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public void ClosePopup(object sender, EventArgs e) => Close();

}