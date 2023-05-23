using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Model;
using GripMobile.Service;
using System.Collections.ObjectModel;

namespace GripMobile.ViewModel
{
    public partial class UserListPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        public ObservableCollection<UserDTO> currentObservableUsers;

        private readonly UserListService userListService;

        public UserListPageViewModel(UserListService userListService) => this.userListService = userListService;

        [RelayCommand]
        public async void GetUsers()
        {
            IsRefreshing = true;

            try
            {
                List<UserDTO> users = await userListService.GetUsers();

                CurrentObservableUsers = new ObservableCollection<UserDTO>(users);
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
