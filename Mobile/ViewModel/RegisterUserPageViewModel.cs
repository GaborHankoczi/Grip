using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Model;
using GripMobile.Service;
using System.Text.RegularExpressions;

namespace GripMobile.ViewModel
{
    public partial class RegisterUserPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string emailAddress = "";

        private RegisterUserDTO data;

        private UserDTO result;

        private readonly RegisterUserService registerUserService;

        public RegisterUserPageViewModel(RegisterUserService registerUserService) => this.registerUserService = registerUserService;

        [GeneratedRegex("^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$")]
        private partial Regex EmailAddressRegex();

        /// <value>Property <c>cancellationTokenSource</c> is needed for closing the toast messages.</value>
        private CancellationTokenSource cancellationTokenSource = new();

        /// <value>Property <c>toast</c> represents every toast message in <c>LoginPage</c></value>
        private IToast toast;

        [RelayCommand]
        async void RegisterUser()
        {
            //Validating the given data.
            if (!EmailAddressRegex().IsMatch(EmailAddress) || Name.Equals(""))
            {
                toast = Toast.Make("A megadott adatok formátuma helytelen!", ToastDuration.Long, 14.0);

                await toast.Show(cancellationTokenSource.Token);

                return;
            }

            try
            {
                data = new()
                {
                    Email = EmailAddress,
                    Name = Name,  
                };

                result = await registerUserService.RegisterUser(data);

                //Checking the result got back from the server.
                if (result == null)
                {
                    toast = Toast.Make("A regisztráció nem sikerült!", ToastDuration.Long, 14.0);

                    await toast.Show(cancellationTokenSource.Token);

                    return;
                }
               
                toast = Toast.Make("A regisztráció sikeresen megtörtént!", ToastDuration.Long, 14.0);

                await toast.Show(cancellationTokenSource.Token);

                Name = "";
                EmailAddress = "";                
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(@"\tERROR {0}", exception.Message);
                await Shell.Current.DisplayAlert("Error!", message: exception.Message, cancel: "OK");
            }
        }
    }
}
