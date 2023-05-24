using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Model;
using GripMobile.Service;
using Microsoft.Maui.Graphics.Text;

namespace GripMobile.ViewModel
{
    public partial class GiveExemptPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private string userId = "";

        [ObservableProperty]
        private string validFrom = "";

        [ObservableProperty]
        private string validTo = "";

        private CreateExemptDTO data;
        
        private ExemptDTO result;

        private readonly GiveExemptService giveExemptService;

        public GiveExemptPageViewModel(GiveExemptService giveExemptService) => this.giveExemptService = giveExemptService;

        /// <value>Property <c>cancellationTokenSource</c> is needed for closing the toast messages.</value>
        private CancellationTokenSource cancellationTokenSource = new();

        /// <value>Property <c>toast</c> represents every toast message in <c>LoginPage</c></value>
        private IToast toast;

        [RelayCommand]
        async void GiveExempt()
        {
            try
            {
                data = new()
                {
                    IssuedToId = int.Parse(UserId),
                    ValidFrom = ValidFrom,
                    ValidTo = ValidTo
                };

                result = await giveExemptService.GiveExempt(data);

                //Checking the result got back from the server.
                if (result == null)
                {
                    toast = Toast.Make("A kikérő elkészítése sikertelen!", ToastDuration.Long, 14.0);

                    await toast.Show(cancellationTokenSource.Token);

                    return;
                }

                toast = Toast.Make("A kikérő sikeresen létrejött!", ToastDuration.Long, 14.0);

                await toast.Show(cancellationTokenSource.Token);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(@"\tERROR {0}", exception.Message);
                await Shell.Current.DisplayAlert("Error!", message: exception.Message, cancel: "OK");
            }
        }
    }
}
