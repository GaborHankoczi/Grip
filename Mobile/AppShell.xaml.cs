using GripMobile.View;

namespace GripMobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
        InitializeComponent();

        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(ForgotPasswordPopup), typeof(ForgotPasswordPopup));
        Routing.RegisterRoute(nameof(FirstLoginPage), typeof(FirstLoginPage));
        Routing.RegisterRoute(nameof(ResetPasswordPage), typeof(ResetPasswordPage));
        Routing.RegisterRoute(nameof(NFCPage), typeof(NFCPage));
        Routing.RegisterRoute(nameof(CurrentClassesPage), typeof(CurrentClassesPage));
        Routing.RegisterRoute(nameof(ExemptPage), typeof(ExemptPage));
        Routing.RegisterRoute(nameof(UserListPage), typeof(UserListPage));
        Routing.RegisterRoute(nameof(RegisterUserPage), typeof(RegisterUserPage));
    }
}
