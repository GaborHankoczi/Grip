using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Service.Auth;
using GripMobile.View;
using IdentityModel.OidcClient;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace GripMobile.ViewModel
{
    /// <summary>
    /// Class <c>MainPageViewModel</c> implements the logic for the Main Page.
    /// </summary>
    public partial class MainPageViewModel: ObservableObject
    {
        private readonly OidcClient OidcClient;
        public MainPageViewModel(OidcClient oidcClient)
        {
            this.OidcClient = oidcClient;
        }

        /// <summary>
        /// Method <c>NavigateToLoginPage</c> implements the logic behind the "Bejelentkezés" button. Displays the <c>LoginPage</c>.
        /// </summary>
        [RelayCommand]
        async void NavigateToLoginPage() 
        {
            try
            {
                var loginResult = await OidcClient.LoginAsync(new LoginRequest());
                if (loginResult.IsError)
                {
                    //await DisplayAlert("Error", loginResult.Error, "OK");
                    Debug.WriteLine("Authentication failed: " + loginResult.Error);
                }
                else
                {
                    Preferences.Set(OidcConsts.AccessTokenKeyName, loginResult.AccessToken);
                    Preferences.Set(OidcConsts.IdTokenKeyName, loginResult.IdentityToken);
                    Preferences.Set(OidcConsts.RefreshTokenKeyName, loginResult.RefreshToken);
                    
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(loginResult.AccessToken);
                    var roleClaim = jwt.Claims.First(claim => claim.Type == "role").Value;


                    
                    if (roleClaim.Contains("Student"))
                    {
                        await Shell.Current.GoToAsync("//StudentInterface");
                    }
                    else if (roleClaim.Contains("Admin"))
                    {
                        await Shell.Current.GoToAsync("//AdminInterface");
                    }
                    else if(roleClaim.Contains("Teacher"))
                    {
                        await Shell.Current.GoToAsync("//TeacherInterface"); 
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Authentication failed: " + ex.Message);
            }
            //await Shell.Current.GoToAsync(nameof(LoginPage)); 
        }

        /// <summary>
        /// Method <c>NavigateToFirstLoginPage</c> implements the logic behind the "Először lépek be" button. Displays the <c>FirstLoginPage</c>.
        /// </summary>
        [RelayCommand]
        async void NavigateToFirstLoginPage() => await Shell.Current.GoToAsync(nameof(FirstLoginPage));
    }
}
