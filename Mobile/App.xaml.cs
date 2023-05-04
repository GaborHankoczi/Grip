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
	
	public static Window Window { get; private set; }

    protected override Window CreateWindow(IActivationState activationState)
    {
        Window window = base.CreateWindow(activationState);
		
		window.Stopped += (s, e) =>
		{
            Debug.WriteLine("=========stopped");
        };

        Window = window;

		return window;
    }
}
