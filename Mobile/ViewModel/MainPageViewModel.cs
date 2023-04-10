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
        /// <summary>
        /// Method <c>NavigateToLoginPage</c> implements the logic behind the "Bejelentkezés" button. Displays the <c>LoginPage</c>.
        /// </summary>
        [RelayCommand]
        async void NavigateToLoginPage() => await Shell.Current.GoToAsync(nameof(LoginPage));

        /// <summary>
        /// Method <c>NavigateToFirstLoginPage</c> implements the logic behind the "Először lépek be" button. Displays the <c>FirstLoginPage</c>.
        /// </summary>
        [RelayCommand]
        async void NavigateToFirstLoginPage() => await Shell.Current.GoToAsync(nameof(FirstLoginPage));
    }
}
