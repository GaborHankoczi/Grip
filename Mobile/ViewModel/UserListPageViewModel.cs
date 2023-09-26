using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Model.Api;
using GripMobile.Service;
using GripMobile.View;
using System.Collections.ObjectModel;

namespace GripMobile.ViewModel
{
    public partial class UserListPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        public ObservableCollection<UserInfoDTO> currentObservableUsers;

        [ObservableProperty]
        private UserInfoDTO selectedUser;

        private readonly UserListService userListService;

        private ApiClient api;

        public UserListPageViewModel(UserListService userListService, ApiClient client)
        {
            this.userListService = userListService;
            api = client;
            GetUsers();
        }

        partial void OnSelectedUserChanged(UserInfoDTO value)
        {
            ShowDetails(value);
        }
        
        private async void ShowDetails(UserInfoDTO value)
        {
            var details = await api.StudentAsync(value.Id);
            await Shell.Current.ShowPopupAsync(new StudentInfoPopup(new StudentInfoViewModel(details)));
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
