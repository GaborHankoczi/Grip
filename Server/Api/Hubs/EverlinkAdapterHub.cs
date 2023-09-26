using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grip.Bll.Services;
using Grip.Bll.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Grip.Api.Hubs
{
    public class EverlinkAdapterHub : Hub<IEverlinkAdapterClient>, IEverlinkAdapterHub
    {
        private ILogger<EverlinkAdapterHub> _logger;
        private IEverlinkAdapterService _everlinkAdapterService;
        public EverlinkAdapterHub(ILogger<EverlinkAdapterHub> logger, IEverlinkAdapterService everlinkAdapterService)
        {
            _logger = logger;
            _everlinkAdapterService = everlinkAdapterService;
        }

        public void SendQuerryResult(byte[] result)
        {
            _everlinkAdapterService.ResponseReceived(result);
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client {Context.ConnectionId} connected to EverlinkAdapterHub");
            _everlinkAdapterService.AdapterConnected();
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogWarning($"Client {Context.ConnectionId} disconnected connected from EverlinkAdapterHub");
            _everlinkAdapterService.AdapterDisconnected();
            return base.OnDisconnectedAsync(exception);
        }
    }
}