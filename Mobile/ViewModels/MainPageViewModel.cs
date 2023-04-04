using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Views;

namespace GripMobile.ViewModels
{
    public partial class MainPageViewModel: ObservableObject
    {
        [RelayCommand]
        async void NavigateToLoginPage() => await Shell.Current.GoToAsync(nameof(LoginPage));
    }
}
