using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Model;
using GripMobile.Service;
using System.Collections.ObjectModel;

namespace GripMobile.ViewModel
{
    public partial class CurrentClassesPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        public ObservableCollection<ClassDTO> currentObservableClasses;

        private readonly CurrentClassesService currentClassesService;

        /// <value>Property <c>cancellationTokenSource</c> is needed for closing the toast messages.</value>
        private CancellationTokenSource cancellationTokenSource = new();

        /// <value>Property <c>toast</c> represents every toast message in <c>LoginPage</c></value>
        private IToast toast;

        public CurrentClassesPageViewModel(CurrentClassesService currentClassesService) => this.currentClassesService = currentClassesService;

        [RelayCommand]
        public async void UpdateClasses()
        {
            IsRefreshing = true;
            
            try
            {
                List<ClassDTO> currentClasses = await currentClassesService.GetCurrentClasses(DateTime.Now.ToString("yyyy-MM-dd"));

                if (currentClasses == null || currentClasses.Count == 0)
                {
                    toast = Toast.Make("A mai napon nincsnek órák!", ToastDuration.Long, 14.0);

                    await toast.Show(cancellationTokenSource.Token);

                    IsRefreshing = false;

                    return;
                }

                CurrentObservableClasses = new ObservableCollection<ClassDTO>(currentClasses);
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
