using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Model;
using GripMobile.Service;
using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using System.Net;
using GripMobile.View;

namespace GripMobile.ViewModel
{
   /// <summary>
   /// Class <c>ForgotPasswordPopupViewModel</c> implements the logic behind every element in <c>ForgotPasswordPopup</c>.
   /// </summary>
    public partial class ForgotPasswordPopupViewModel: ObservableObject
    {
        /// <value>Property <c>userData</c> contains the validated user input.</value>
        private ForgotPasswordDTO userData;

        [ObservableProperty]
        private string emailAddress = "";

        private readonly ForgotPasswordService forgotPasswordService;

        public ForgotPasswordPopupViewModel(ForgotPasswordService forgotPasswordService) => this.forgotPasswordService = forgotPasswordService;

        [GeneratedRegex("^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$")]
        private partial Regex EmailAddressRegex();

        /// <value>Property <c>cancellationTokenSource</c> is needed for closing the toast messages.</value>
        private CancellationTokenSource cancellationTokenSource = new();

        /// <value>Property <c>toast</c> represents every toast message in <c>LoginPage</c></value>
        private IToast toast;

        /// <summary>
        /// Method <c>GetToken</c> implements the logic behind the "Ok" button. Validatesthe user input, and gives them to the <c>ForgotPasswordService</c> to handle the forgot password process.
        /// </summary>
        [RelayCommand]
        public async void GetToken()
        {
            //Validating the user's input.
            if(!EmailAddressRegex().IsMatch(EmailAddress))
            {
                toast = Toast.Make("A megadott e-mail cím formátuma helytelen!", ToastDuration.Long, 14.0);

                await toast.Show(cancellationTokenSource.Token);

                return;
            }

            try
            {
                userData = new()
                {
                    Email = EmailAddress
                };

                var response = await forgotPasswordService.SendToken(userData);

                if (response == HttpStatusCode.OK)
                {
                    //We need to pass the userData to the ResetPasswordPage, because the ResetPasswordDTO needs it.
                    await Shell.Current.GoToAsync($"{nameof(ResetPasswordPage)}", new Dictionary<string, object> { ["userEmail"] = userData});
                }
                else if (response == HttpStatusCode.NotFound)
                {
                    toast = Toast.Make("A megadott e-mail címmel nincs regisztrálva felhasználó a rendszerben!", ToastDuration.Long, 14.0);

                    await toast.Show(cancellationTokenSource.Token);

                    return;
                }
                else
                {
                    toast = Toast.Make("Hiba lépett fel. Próbálja újra!", ToastDuration.Long, 14.0);

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
