using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GripMobile.ViewModel
{
    public partial  class CreateExemptViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<UserInfoDTO> students;

        [ObservableProperty]
        private UserInfoDTO selectedStudent;

        [ObservableProperty]
        private DateTime fromDate;

        [ObservableProperty]
        private TimeSpan fromTime;

        [ObservableProperty]
        private DateTime validToDate;

        [ObservableProperty]
        private TimeSpan validToTime;

        private readonly Client api;

        public CreateExemptViewModel(Client api)
        {
            this.api = api;
            LoadPickers();
        }

        private async void LoadPickers()
        {
            Students = (await api.SearchAsync("",null)).ToList();
            SelectedStudent = Students[0];
        }

        [RelayCommand]
        public async void CreateExempt()
        {
            var dto = new CreateExemptDTO
            {
                IssuedToId = SelectedStudent.Id,
                ValidFrom = new DateTime(FromDate.Year, FromDate.Month, FromDate.Day, FromTime.Hours, FromTime.Minutes, 0,DateTimeKind.Utc),
                ValidTo = new DateTime(ValidToDate.Year, ValidToDate.Month, ValidToDate.Day, ValidToTime.Hours, ValidToTime.Minutes, 0, DateTimeKind.Utc),
            };
            var result = await api.ExemptPOSTAsync(dto);
            await Toast.Make("Igazolás sikeresen létregozva!", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
        }
    }
}
