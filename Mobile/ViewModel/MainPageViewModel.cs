using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.View;

namespace GripMobile.ViewModel
{
    /// <summary>
    /// Class <c>MainPageViewModel</c> implements the logic for the Main Page.
    /// </summary>
    public partial class MainPageViewModel: ObservableObject
    {
        [RelayCommand]
        async void NavigateToLoginPage() => await Shell.Current.GoToAsync(nameof(LoginPage));

        [RelayCommand]
        async void NavigateToFirstLoginPage() => await Shell.Current.GoToAsync(nameof(FirstLoginPage));
    }
}
