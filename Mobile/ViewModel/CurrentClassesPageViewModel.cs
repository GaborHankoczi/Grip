using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Model;
using GripMobile.Model.Api;
using GripMobile.Service;
using System.Collections.ObjectModel;

namespace GripMobile.ViewModel
{
    public partial class CurrentClassesPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        public ObservableCollection<AttendanceDTO> currentObservableClasses;

        private readonly Client api;

        /// <value>Property <c>cancellationTokenSource</c> is needed for closing the toast messages.</value>
        private CancellationTokenSource cancellationTokenSource = new();

        /// <value>Property <c>toast</c> represents every toast message in <c>LoginPage</c></value>
        private IToast toast;
        
        public CurrentClassesPageViewModel(Client api) 
        {
            this.api = api;
            UpdateClasses();
        }

        [RelayCommand]
        public async void UpdateClasses()
        {
            IsRefreshing = true;
            
            try
            {
                ICollection<AttendanceDTO> currentClasses = await api.AttendanceAllAsync(DateTime.Now);

                if (currentClasses == null || currentClasses.Count == 0)
                {
                    toast = Toast.Make("A mai napon nincsnek órák!", ToastDuration.Long, 14.0);

                    await toast.Show(cancellationTokenSource.Token);

                    IsRefreshing = false;

                    return;
                }

                CurrentObservableClasses = new ObservableCollection<AttendanceDTO>(currentClasses);
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
