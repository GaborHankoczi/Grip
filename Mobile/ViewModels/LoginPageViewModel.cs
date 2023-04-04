using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using GripMobile.Model;
using GripMobile.Services;
using GripMobile.Views;
using CommunityToolkit.Maui.Core;
using System.Text.RegularExpressions;

namespace GripMobile.ViewModels
{
    public partial class LoginPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private string emailAddress = "";

        [ObservableProperty]
        private string password = "";
        
        private User user = new();

        private LoginService loginService;

        public LoginPageViewModel(LoginService loginService) => this.loginService = loginService;

        [GeneratedRegex(@"^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$")]
        private partial Regex EmailAddressRegex();

        [GeneratedRegex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$")]
        private partial Regex PasswordRegex();

        [RelayCommand]
        async void LogInUser()
        {
            //Validation
            if (!EmailAddressRegex().IsMatch(EmailAddress) || !PasswordRegex().IsMatch(Password))
            {
                CancellationTokenSource cancellationTokenSource = new();

                var toast = Toast.Make("A megadott adatok formátuma helytelen!", ToastDuration.Long, 14.0);

                await toast.Show(cancellationTokenSource.Token);

                return;
            }
            
            try
            {
                user = await loginService.CheckUserCredentials(EmailAddress, Password);

                //Validtaion
                if (user.UserName == null || user.Email == null || user.Roles.Length == 0)
                {
                    CancellationTokenSource cancellationTokenSource = new();

                    var toast = Toast.Make("A megadott adatokkal nincs regisztrálva felhasználó!", ToastDuration.Long, 14.0);

                    await toast.Show(cancellationTokenSource.Token);

                    return;
                }

                //TODO: a role-oknak megfelelő oldalra kell továbbítani a felhasználót.
                await Shell.Current.GoToAsync(nameof(UserDetailsPage));
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(@"\tERROR {0}", exception.Message);
                await Shell.Current.DisplayAlert("Error!", message: exception.Message, cancel: "OK");
            }
        }
        
        [RelayCommand]
        async void NavigateToForgotPasswordPage() => await Shell.Current.GoToAsync(nameof(ForgotPasswordPage));
    }
}
