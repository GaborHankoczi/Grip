using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grip.Bll.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Grip.Api.Hubs;
using System.Data;
using Grip.Bll.Everlink;
using AutoMapper;
using Grip.Bll.DTO.Everlink;

namespace Grip.Bll.Services
{
    public class EverlinkAdapterService : IEverlinkAdapterService
    {
        private int connectedCount = 0;
        private TaskCompletionSource<byte[]> dataReceivedTaskCompletionSource = new TaskCompletionSource<byte[]>();
        private readonly ILogger<EverlinkAdapterService> _logger;
        private readonly IHubContext<EverlinkAdapterHub, IEverlinkAdapterClient> _hubContext;
        private readonly IMapper _mapper;
        public EverlinkAdapterService(ILogger<EverlinkAdapterService> logger, IHubContext<EverlinkAdapterHub, IEverlinkAdapterClient> hubContext, IMapper mapper)
        {
            _logger = logger;
            _hubContext = hubContext;
            _mapper = mapper;
        }
        public void AdapterConnected()
        {
            connectedCount++;
        }

        public void AdapterDisconnected()
        {
            connectedCount--;
        }

        public bool IsConnected()
        {
            return connectedCount > 0;
        }

        public void ResponseReceived(byte[] response)
        {
            dataReceivedTaskCompletionSource.SetResult(response);
        }


        public async Task<TableDTO> SendQuery(string query)
        {
            if(!IsConnected())
                throw new Exception("Everlink adapter is not connected" );
            await _hubContext.Clients.All.RunQuerry(query);
            var result = await AwaitResponse();
            var table = new Table();
            await table.LoadFromZip(result);
            var tableDTO =  _mapper.Map<TableDTO>(table);
            _logger.LogInformation($"Querry {query} executed successfully");
            return tableDTO;
        }

        public async Task<byte[]> SendQueryZip(string query)
        {
            if(!IsConnected())
                throw new Exception("Everlink adapter is not connected" );
            await _hubContext.Clients.All.RunQuerry(query);
            var result = await AwaitResponse();
            _logger.LogInformation($"Querry {query} executed successfully");
            return result;
        }
        private async Task<byte[]> AwaitResponse(TimeSpan? timeout = null)
        {
            if(timeout == null)
                timeout = TimeSpan.FromSeconds(10);
            _logger.LogInformation("Awaiting response");
            var dataTask = dataReceivedTaskCompletionSource.Task;
            var timeoutTask = Task.Delay(timeout.Value);
            var completedTask = await Task.WhenAny(dataReceivedTaskCompletionSource.Task, timeoutTask);

            if (completedTask == dataTask)
            {
                _logger.LogInformation("Response received");
                dataReceivedTaskCompletionSource = new TaskCompletionSource<byte[]>();
                return await dataTask; // Data received before timeout
            }
            else
            {
                dataReceivedTaskCompletionSource = new TaskCompletionSource<byte[]>();
                throw new TimeoutException("Timeout waiting for data.");
            }
        }
    }
}