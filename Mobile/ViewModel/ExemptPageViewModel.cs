using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Model;
using GripMobile.Service;
using System.Collections.ObjectModel;
using System.Threading;

namespace GripMobile.ViewModel
{
    public partial class ExemptPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        public ObservableCollection<ExemptDTO> currentObservableExempts;

        private readonly ExemptGetService exemptGetService;

        public ExemptPageViewModel(ExemptGetService exemptGetService) 
        { 
            this.exemptGetService = exemptGetService;
            UpdateExempts();
        }

        /// <value>Property <c>cancellationTokenSource</c> is needed for closing the toast messages.</value>
        private CancellationTokenSource cancellationTokenSource = new();

        /// <value>Property <c>toast</c> represents every toast message in <c>LoginPage</c></value>
        private IToast toast;

        [RelayCommand]
        public async void UpdateExempts()
        {
            IsRefreshing = true;

            try
            {
                List<ExemptDTO> exempts = await exemptGetService.GetExempts();

                if (exempts == null || exempts.Count == 0)
                {
                    toast = Toast.Make("Nincsenek aktív kikérők!", ToastDuration.Long, 14.0);

                    await toast.Show(cancellationTokenSource.Token);

                    IsRefreshing = false;

                    return;
                }

                CurrentObservableExempts = new ObservableCollection<ExemptDTO>(exempts);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(@"\tERROR {0}", exception.Message);
                await Shell.Current.DisplayAlert("Error!", message: exception.Message, cancel: "OK");
            }

            IsRefreshing = false;
        }
    }
}
