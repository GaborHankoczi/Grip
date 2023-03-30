using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using GripMobile.Model;
using GripMobile.Services;
using GripMobile.Views;
using CommunityToolkit.Maui.Core;

namespace GripMobile.ViewModels
{
    public partial class LoginPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private string idCardNumber = "";

        [ObservableProperty]
        private string password = "";
        
        private User user = new();

        private LoginService loginService;

        public LoginPageViewModel(LoginService loginService) => this.loginService = loginService; 

        [RelayCommand]
        async void LogInUser()
        {
            //Validáció
            if (IdCardNumber == "" || Password == "" || IdCardNumber.Length < 8 || Password.Length < 8)
            {
                CancellationTokenSource cancellationTokenSource = new();

                var toast = Toast.Make("A megadott adatok formátuma helytelen!", ToastDuration.Long, 14.0);

                await toast.Show(cancellationTokenSource.Token);

                return;
            }
            
            try
            {
                user = await loginService.CheckUserCredentials(IdCardNumber, Password);

                if (user.Id == -1)
                {
                    CancellationTokenSource cancellationTokenSource = new();

                    var toast = Toast.Make("A megadott adatokkal nincs regisztrálva felhasználó!", ToastDuration.Long, 14.0);

                    await toast.Show(cancellationTokenSource.Token);

                    return;
                }

                await Shell.Current.GoToAsync(nameof(UserDetailsPage));
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(@"\tERROR {0}", exception.Message);
                await Shell.Current.DisplayAlert("Error!", exception.Message, "OK");
            }
        }
        
        [RelayCommand]
        async void NavigateToForgotPasswordPage() => await Shell.Current.GoToAsync(nameof(ForgotPasswordPage));
        
        [RelayCommand]
        async void NavigateToRegisterPage() => await Shell.Current.GoToAsync(nameof(RegisterPage));
    }
}
