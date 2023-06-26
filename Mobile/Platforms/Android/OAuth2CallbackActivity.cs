using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidOs = Android;

namespace GripMobile.Platforms.Android;

[Activity(Label = "OAuth2CallbackActivity", NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { AndroidOs.Content.Intent.ActionView },
              Categories = new[] { AndroidOs.Content.Intent.CategoryDefault, AndroidOs.Content.Intent.CategoryBrowsable },
              DataScheme = CALLBACK_SCHEME)]
public class OAuth2CallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{

    const string CALLBACK_SCHEME = "grip";
}