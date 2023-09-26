using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Adapter.EverlinkProtocol;
using Microsoft.AspNetCore.SignalR.Client;

var hubConnection = new HubConnectionBuilder()
                .WithUrl("https://nloc.duckdns.org:8030/hubs/everlink",conf=>{conf.HttpMessageHandlerFactory = _ => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };})
                .WithAutomaticReconnect()
                .Build();
hubConnection.Closed += async (error) =>
{

  Console.WriteLine("Disconnected from SignalR");
  await Task.Delay(1000);    
  Console.WriteLine("Reconnecting");
  await hubConnection.StartAsync();
};

EverlinkConnection connection;
BlockingCollection<string> queryQueue = new BlockingCollection<string>();
hubConnection.On<string>("RunQuerry", async (query) =>{ 
  Console.WriteLine($"Received query: {query}");
  queryQueue.Add(query);
  });

await hubConnection.StartAsync();
Console.WriteLine("Connected to SignalR");
while(true){
  try
  {
    connection = new EverlinkConnection("localhost", 5100, "admin","12345");
    await connection.ConnectAsync();
    Console.WriteLine("Connected to Everlink");
    while(true){
      var query = queryQueue.Take();
      Console.WriteLine($"Received query: {query}");
      var result = await connection.QueryAsync(query);
      await hubConnection.InvokeAsync("SendQuerryResult", result);
    }

  }catch(Exception ex)
  {
    Console.WriteLine(ex.Message);
    await Task.Delay(1000);    
    Console.WriteLine("Reconnecting...");
  }
}
return;

//





