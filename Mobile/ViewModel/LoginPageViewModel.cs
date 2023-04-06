using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using GripMobile.Model;
using GripMobile.Service;
using GripMobile.View;
using CommunityToolkit.Maui.Core;
using System.Text.RegularExpressions;

namespace GripMobile.ViewModel
{
    /// <summary>
    /// Class <c>LoginPageViewModel</c> implements the logic behind Login Page.
    /// </summary>
    public partial class LoginPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private string emailAddress;

        [ObservableProperty]
        private string password;
        
        private LoginUserDTO userData;
        private LoginResultDTO result;

        private readonly LoginService loginService;

        public LoginPageViewModel(LoginService loginService) => this.loginService = loginService;

        [GeneratedRegex("^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$")]
        private partial Regex EmailAddressRegex();

        [GeneratedRegex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$")]
        private partial Regex PasswordRegex();

        /// <summary>
        /// Logic behind the "Bejelentkezés" button. If the given data is valid and the communication with the server was successful,
        /// the user is navigated to the next page according to his/her roles.
        /// </summary>
        [RelayCommand]
        async void LogInUser()
        {
            //Validating the given data.
            if (!EmailAddressRegex().IsMatch(EmailAddress) || !PasswordRegex().IsMatch(Password))
            {
                CancellationTokenSource cancellationTokenSource = new();

                var toast = Toast.Make("A megadott adatok formátuma helytelen!", ToastDuration.Long, 14.0);

                await toast.Show(cancellationTokenSource.Token);

                return;
            }
            
            try
            {
                //Filling up the LoginUserDTO with the given data.
                userData = new()
                {
                    Email = EmailAddress,
                    Password = Password
                };

                result = await loginService.CheckUserCredentials(userData);

                //Validating the result got back from the server,
                if (result.UserName == null || result.Email == null || result.Roles.Length == 0)
                {
                    CancellationTokenSource cancellationTokenSource = new();

                    var toast = Toast.Make("A megadott adatokkal nincs regisztrálva felhasználó!", ToastDuration.Long, 14.0);

                    await toast.Show(cancellationTokenSource.Token);

                    return;
                }

                //TODO: a role-oknak megfelelő oldalra kell továbbítani a felhasználót.
                await Shell.Current.GoToAsync(nameof(UserDetailsPage));

                return;
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
