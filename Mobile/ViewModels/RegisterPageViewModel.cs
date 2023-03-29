using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Views;

namespace GripMobile.ViewModels
{
    public partial class RegisterPageViewModel: ObservableObject
    {
        [RelayCommand]
        async void NavigateToLoginPage() => await Shell.Current.GoToAsync(nameof(LoginPage));
    }
}
