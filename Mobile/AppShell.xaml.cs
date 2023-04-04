using GripMobile.Views;

namespace GripMobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
        InitializeComponent();

        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
        Routing.RegisterRoute(nameof(UserDetailsPage), typeof(UserDetailsPage));
    }
}
