using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Model;
using GripMobile.Service;
using GripMobile.View;
using System.Net;
using System.Text.RegularExpressions;

namespace GripMobile.ViewModel
{
    /// <summary>
    /// Class <c>ResetPasswordPageViewModel</c> implements the logic behind every component in <c>ResetPasswordPage</c>.
    /// </summary>
    [QueryProperty(nameof(userEmail), "userEmail")]
    public partial class ResetPasswordPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private string token = "";

        [ObservableProperty]
        private string newPassword = "";

        [ObservableProperty]
        private string newPasswordAgain = "";

        ///<value>Property <c>userEmail</c> holds the data that has been sent from <c>ForgotPasswordPageViewModel</c> via <c>QueryProperty</c>.</value>
        private ForgotPasswordDTO userEmail;

        [GeneratedRegex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$")]
        private partial Regex PasswordRegex();

        /// <value>Property <c>userData</c> contains every <c>ObservableProperty</c>'s value (except <c>newPasswordAgain</c>) and the <c>QueryProperty</c>'s <c>Email</c> value.</value>
        private ResetPasswordDTO userData;

        private readonly ResetPasswordService resetPasswordService;

        public ResetPasswordPageViewModel(ResetPasswordService resetPasswordService) => this.resetPasswordService = resetPasswordService;

        /// <value>Property <c>cancellationTokenSource</c> is needed for closing the toast messages.</value>
        private CancellationTokenSource cancellationTokenSource = new();

        /// <value>Property <c>toast</c> represents every toast message in <c>LoginPage</c></value>
        private IToast toast;

        /// <summary>
        /// Method <c>ResetPassword</c> implements the logic behind the "Beállítás" button. If every user data is valid, then it handles the reset password process.
        /// </summary>
        [RelayCommand]
        public async void ResetPassword()
        {
            //Validating the user input.
            if (Token.Length < 6 || !PasswordRegex().IsMatch(NewPassword) || !PasswordRegex().IsMatch(NewPasswordAgain))
            {
                toast = Toast.Make("A megadott adatok formátuma helytelen!", ToastDuration.Long, 14.0);

                await toast.Show(cancellationTokenSource.Token);

                return;
            }

            //Check if the two given passwords are equal or not.
            if (!NewPassword.Equals(NewPasswordAgain))
            {
                toast = Toast.Make("A megadott jelszavak nem egyeznek!", ToastDuration.Long, 14.0);

                await toast.Show(cancellationTokenSource.Token);

                return;
            }

            try
            {
                //Filling up the Data Trasnfer Object needed by the ResetPasswordService.
                userData = new()
                {
                    Email = userEmail.Email,
                    Token = Token,
                    Password = NewPassword
                };

                var response = await resetPasswordService.ResetPassword(userData);

                if (response == HttpStatusCode.OK)
                {
                    await Shell.Current.GoToAsync(nameof(LoginPage));
                }
                else
                {
                    toast = Toast.Make("Hiba a jelszó megváltoztatása során. Próbálja újra!", ToastDuration.Long, 14.0);

                    await toast.Show(cancellationTokenSource.Token);

                    return;
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(@"\tERROR {0}", exception.Message);
                await Shell.Current.DisplayAlert("Error!", message: exception.Message, cancel: "OK");
            }
        }

        /// <summary>
        /// Method <c>NavigateToLoginPage</c> implements the logic behind the "Mégse" button. Navigates the user back to the <c>LoginPage</c>.
        /// </summary>
        [RelayCommand]
        public async void NavigateToLoginPage() => await Shell.Current.GoToAsync(nameof(LoginPage));
    }
}
