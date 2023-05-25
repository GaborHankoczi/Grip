using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Model.Api;
using GripMobile.Service;
using System.Collections.ObjectModel;

namespace GripMobile.ViewModel
{
    public partial class UserListPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        public ObservableCollection<UserInfoDTO> currentObservableUsers;

        private readonly UserListService userListService;

        private Client api;

        public UserListPageViewModel(UserListService userListService, Client client)
        {
            this.userListService = userListService;
            api = client;
            GetUsers();
        }

        [RelayCommand]
        public async void GetUsers()
        {
            IsRefreshing = true;

            try
            {
                ICollection<UserInfoDTO> users = await api.SearchAsync("",null);

                CurrentObservableUsers = new ObservableCollection<UserInfoDTO>(users);
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
