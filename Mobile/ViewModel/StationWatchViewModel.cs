using CommunityToolkit.Mvvm.ComponentModel;
using GripMobile.Model.Api;
using GripMobile.Model.Api.HubDTO;
using GripMobile.Model.Api.Hubs;
using GripMobile.Service.Auth;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GripMobile.ViewModel
{
    public partial class StationWatchViewModel : ObservableObject, IStationClient
    {
        [ObservableProperty]
        private ObservableCollection<StationScanDTO> scans;

        [ObservableProperty]
        private StationScanDTO selectedScan;

        private readonly HubConnection connection;

        private readonly ApiClient api;

        public StationWatchViewModel(ApiClient api)
        {
            this.api = api;
            Scans = new ObservableCollection<StationScanDTO>();
            var accessToken = Preferences.Get(OidcConsts.AccessTokenKeyName,null);
            if (accessToken != null) { 
                connection = new HubConnectionBuilder()
                   .WithUrl(api.BaseUrl + "/hubs/station",
                        options => {
                            options.AccessTokenProvider = () => Task.FromResult(accessToken);
                        })
                   .Build();
                StartSignalR();
                connection.On<StationScanDTO>("ReceiveScan", ReceiveScan);
            }
            else
            {
                Debugger.Break();
            }
        }

        private async void StartSignalR()
        {
            try { 
                await connection.StartAsync();
                await connection.SendAsync("SelectStation", 1);
            }catch(Exception e)
            {
                Debugger.Break();
                Debug.WriteLine("Error connectiong to station hub");
            }
        }

        public Task ReceiveScan(StationScanDTO scanDTO)
        {
            Debug.WriteLine("Received data from station hub");
            Scans.Add(scanDTO);
            return Task.CompletedTask;
        }
    }
}
