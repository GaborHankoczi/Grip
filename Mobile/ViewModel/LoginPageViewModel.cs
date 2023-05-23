using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using GripMobile.Model;
using GripMobile.Service;
using GripMobile.View;
using CommunityToolkit.Maui.Core;
using System.Text.RegularExpressions;

namespace GripMobile.ViewModel
{
    /// <summary>
    /// Class <c>LoginPageViewModel</c> validates and forwards the <c>LoginPage</c>'s inputs and implements the logic behind every view element.
    /// </summary>
    public partial class LoginPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private string emailAddress = "";

        [ObservableProperty]
        private string password = "";

        /// <value>Property <c>userData</c> contains all <c>ObservableProperty</c>'s data.</value>
        private LoginUserDTO userData;

        /// <value>Property <c>result</c> contains the data got back from the server.</value>
        private LoginResultDTO result;

        private readonly LoginService loginService;

        public LoginPageViewModel(LoginService loginService) => this.loginService = loginService;

        [GeneratedRegex("^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$")]
        private partial Regex EmailAddressRegex();

        [GeneratedRegex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$")]
        private partial Regex PasswordRegex();

        /// <value>Property <c>cancellationTokenSource</c> is needed for closing the toast messages.</value>
        private CancellationTokenSource cancellationTokenSource = new();

        /// <value>Property <c>toast</c> represents every toast message in <c>LoginPage</c></value>
        private IToast toast;

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
                toast = Toast.Make("A megadott adatok formátuma helytelen!", ToastDuration.Long, 14.0);

                await toast.Show(cancellationTokenSource.Token);

                return;
            }
            
            try
            {
                //Filling up the LoginUserDTO with the given valid data.
                userData = new()
                {
                    Email = EmailAddress,
                    Password = Password
                };

                result = await loginService.CheckUserCredentials(userData);

                //Checking the result got back from the server.
                if (result.UserName == null || result.Email == null || result.Roles.Length == 0)
                {
                    toast = Toast.Make("A megadott adatokkal nincs regisztrálva felhasználó!", ToastDuration.Long, 14.0);

                    await toast.Show(cancellationTokenSource.Token);

                    return;
                }

                if (result.Roles.Contains("Student"))
                {
                    await Shell.Current.GoToAsync("//StudentInterface");
                }
                else if (result.Roles.Contains("Admin"))
                {
                    await Shell.Current.GoToAsync("//AdminInterface");
                }
                else { await Shell.Current.GoToAsync("//TeacherInterface"); }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(@"\tERROR {0}", exception.Message);
                await Shell.Current.DisplayAlert("Error!", message: exception.Message, cancel: "OK");
            }
        }

        /// <summary>
        /// Method <c>DisplayForgotPasswordPopup</c> implements the logic behind the "Elfelejtettem a jelszavam" button.
        /// Displays a popup window where the user can start the process to get a new password.
        /// </summary>
        [RelayCommand]
        public async void DisplayForgotPasswordPopup()
        {
            var popup = new ForgotPasswordPopup(new ForgotPasswordPopupViewModel(new ForgotPasswordService()));

            await Shell.Current.CurrentPage.ShowPopupAsync(popup);
        }

        /// <summary>
        /// Method <c>BackToMainPage</c> implements the logic behind the back button. Navigates back to the <c>MainPage</c>.
        /// </summary>
        [RelayCommand]
        async void BackToMainPage() => await Shell.Current.GoToAsync("///MainPage");
    }
}
