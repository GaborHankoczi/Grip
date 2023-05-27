using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GripMobile.ViewModel
{
    public partial class AssignToGroupViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<UserInfoDTO> students;

        [ObservableProperty]
        private UserInfoDTO selectedStudent;

        [ObservableProperty]
        private List<GroupDTO> groups;

        [ObservableProperty]
        private GroupDTO selectedGroup;

        private readonly Client api;

        public AssignToGroupViewModel(Client api)
        {
            this.api = api;
            LoadPickers();
        }

        private async void LoadPickers()
        {
            Students = (await api.SearchAsync("", null)).ToList();
            SelectedStudent = Students[0];
            Groups = (await api.GroupAllAsync()).ToList();
            SelectedGroup = Groups[0];
        }

        [RelayCommand]
        public async void AssignToGroup()
        {
            await api.AddUserAsync( SelectedGroup.Id, SelectedStudent.Id);
            await Toast.Make("Sikeresen hozzárendelve!", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
        }

    }
}
