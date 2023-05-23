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
    /// Class <c>FirstLoginPageViewModel</c> validates and forwards the <c>FirstLoginPage</c>'s inputs and implements the logic behind every view element.
    /// </summary>
    public partial class FirstLoginPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private string emailAddress = "";

        [ObservableProperty]
        private string givenToken = "";

        [ObservableProperty]
        private string newPassword = "";

        [ObservableProperty]
        private string newPasswordAgain = "";

        /// <value>Property <c>userData</c> contains all <c>ObservableProperty</c>'s data (despite the <c>newPasswordAgain</c>).</value>
        private ConfirmEmailDTO userData;

        private readonly FirstLoginService firstLoginService;

        public FirstLoginPageViewModel(FirstLoginService firstLoginService) => this.firstLoginService = firstLoginService;

        [GeneratedRegex("^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$")]
        private partial Regex EmailAddressRegex();

        [GeneratedRegex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$")]
        private partial Regex PasswordRegex();

        /// <value>Property <c>cancellationTokenSource</c> is needed for closing the toast messages.</value>
        private CancellationTokenSource cancellationTokenSource = new();

        /// <value>Property <c>toast</c> represents every toast message in <c>FirstLoginPage</c></value>
        private IToast toast;

        /// <summary>
        /// Method <c>ConfirmEmail</c> authenticates the view's input and communicates with an instance of the <c>FirstLoginService</c> class.
        /// This method represents the logic behind the "Küldés" button.
        /// </summary>
        [RelayCommand]
        async void ConfirmEmail()
        {
            //Check if the given data has the correct format.
            if(!EmailAddressRegex().IsMatch(EmailAddress) || GivenToken.Length != 6 || !PasswordRegex().IsMatch(NewPassword) || !PasswordRegex().IsMatch(NewPasswordAgain))
            {
                toast = Toast.Make("A megadott adatok formátuma helytelen!", ToastDuration.Long, 14.0);

                await toast.Show(cancellationTokenSource.Token);

                return;
            }

            //Check if the user has entered the same password twice or not.
            if (!NewPassword.Equals(NewPasswordAgain))
            {
                toast = Toast.Make("A megadott két jelszó nem egyezik!", ToastDuration.Long, 14.0);

                await toast.Show(cancellationTokenSource.Token);

                return;
            }

            try
            {
                userData = new()
                {
                    Email = EmailAddress,
                    Token = GivenToken,
                    Password = NewPassword
                };

                HttpStatusCode result = await firstLoginService.ConfirmEmail(userData);

                if (result == HttpStatusCode.OK)
                {
                    await Shell.Current.GoToAsync(nameof(LoginPage));

                    return;
                }
                else if (result == HttpStatusCode.NotFound)
                {
                    toast = Toast.Make("A megadott e-mail címmel nincs regisztrálva felhasználó a rendszerben.", ToastDuration.Long, 14.0);

                    await toast.Show(cancellationTokenSource.Token);

                    return;
                }
                else
                {
                    toast = Toast.Make($"Az e-mail cím megerősítése sikertelen. Próbálja újra! {result}", ToastDuration.Long, 14.0);

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
        /// Method <c>BackToMainPage</c> implements the logic behind the back button and changes the view from <c>FirstLoginPage</c> to <c>MainPage</c>.
        /// </summary>
        [RelayCommand]
        async void BackToMainPage() => await Shell.Current.GoToAsync("///MainPage");
    }
}
