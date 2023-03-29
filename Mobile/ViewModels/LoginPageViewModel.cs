using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Views;

namespace GripMobile.ViewModels
{
    public partial class LoginPageViewModel: ObservableObject
    {
        [RelayCommand]
        async void NavigateToForgotPasswordPage() => await Shell.Current.GoToAsync(nameof(ForgotPasswordPage));
        
        [RelayCommand]
        async void NavigateToRegisterPage() => await Shell.Current.GoToAsync(nameof(RegisterPage));
    }
}
