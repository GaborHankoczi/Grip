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
    public partial class CreateClassViewModel : ObservableObject
    {
        [ObservableProperty]
        private DateTime startDate;

        [ObservableProperty]
        private TimeSpan startTime;

        [ObservableProperty]
        private List<GroupDTO> groups;

        [ObservableProperty]
        private GroupDTO selectedGroup;

        [ObservableProperty]
        private List<UserDTO> teachers;

        [ObservableProperty]
        private UserDTO selectedTeacher;

        [ObservableProperty]
        private string className;

        [ObservableProperty]
        private string stationId;

        private readonly Client api;

        public CreateClassViewModel(Client api)
        {
            this.api = api;
            LoadPickers();
        }

        private async void LoadPickers()
        {
            Groups = (await api.GroupAllAsync()).ToList();
            SelectedGroup = Groups[0];
            Teachers = (await api.UserAllAsync()).ToList();
            SelectedTeacher = Teachers[0];
        }

        [RelayCommand]
        public async void CreateClass()
        {
            var result = await api.ClassPOSTAsync(new CreateClassDTO
            {
                GroupId = SelectedGroup.Id,
                StartDateTime = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, StartTime.Hours, StartTime.Minutes, 0),
                Name = ClassName,
                TeacherId = SelectedTeacher.Id,
                StationId = Convert.ToInt32(StationId)
            });
            await Toast.Make("Osztály sikeresen létregozva!", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
        }

    }
}
