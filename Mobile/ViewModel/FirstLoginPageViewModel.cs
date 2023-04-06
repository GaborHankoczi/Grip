using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Model;
using GripMobile.Service;
using GripMobile.View;
using System.Text.RegularExpressions;

namespace GripMobile.ViewModel
{
    public partial class FirstLoginPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private string emailAddress = "";

        [ObservableProperty]
        private string token = "";

        [ObservableProperty]
        private string newPassword = "";

        [ObservableProperty]
        private string newPasswordAgain = "";

        private ConfirmEmailDTO userData;

        private readonly FirstLoginService firstLoginService;

        public FirstLoginPageViewModel(FirstLoginService firstLoginService) => this.firstLoginService = firstLoginService;

        [GeneratedRegex("^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$")]
        private partial Regex EmailAddressRegex();

        [GeneratedRegex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$")]
        private partial Regex PasswordRegex();

        [RelayCommand]
        async void ConfirmEmail()
        {
            if(!EmailAddressRegex().IsMatch(EmailAddress) || Token.Length != 6 || !PasswordRegex().IsMatch(NewPassword) || !PasswordRegex().IsMatch(NewPasswordAgain))
            {
                CancellationTokenSource cancellationTokenSource = new();

                var toast = Toast.Make("A megadott adatok formátuma helytelen!", ToastDuration.Long, 14.0);

                await toast.Show(cancellationTokenSource.Token);

                return;
            }

            if (!NewPassword.Equals(NewPasswordAgain))
            {
                CancellationTokenSource cancellationTokenSource = new();

                var toast = Toast.Make("A megadott két jelszó nem egyezik!", ToastDuration.Long, 14.0);

                await toast.Show(cancellationTokenSource.Token);

                return;
            }

            try
            {
                userData = new()
                {
                    Email = EmailAddress,
                    Token = Token,
                    Password = NewPassword
                };

                bool result = await firstLoginService.ConfirmEmail(userData);

                if (result)
                {
                    await Shell.Current.GoToAsync(nameof(LoginPage));

                    return;
                }
                else
                {
                    CancellationTokenSource cancellationTokenSource = new();

                    var toast = Toast.Make("Az e-mail cím megerősítése sikertelen. Próbálja újra!", ToastDuration.Long, 14.0);

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
    }
}
