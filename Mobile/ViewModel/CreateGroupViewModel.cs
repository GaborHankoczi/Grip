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
    public partial class CreateGroupViewModel : ObservableObject
    {
        [ObservableProperty]
        private string name;

        private readonly Client api;

        public CreateGroupViewModel(Client api)
        {
            this.api = api;
        }

        [RelayCommand]
        public async void CreateGroup()
        {
            var dto = new GroupDTO
            {
                Name = Name
            };
            await api.GroupPOSTAsync(dto);
            await Toast.Make("Csoport sikeresen létrehozva!", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
        }

    }
}
