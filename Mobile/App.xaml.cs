using Plugin.NFC;
using System.Diagnostics;

namespace GripMobile;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
