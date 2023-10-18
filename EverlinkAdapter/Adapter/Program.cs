using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Adapter;
using Adapter.EverlinkProtocol;
using Microsoft.AspNetCore.SignalR.Client;
using static Adapter.Logger;


var config = JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync("./data/config.json"));
if(config == null){
  Console.WriteLine("Config file not found at ./data/config.json");
  return;
}
var logger = new Logger(config);
var hubConnection = new HubConnectionBuilder()
                .WithUrl(config.SignalRURL,conf=>{conf.HttpMessageHandlerFactory = _ => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };})
                .WithAutomaticReconnect()
                .Build();
hubConnection.Closed += async (error) =>
{

  logger.Log("Disconnected from SignalR",LogLevel.Warning);
  while(true){
    CancellationTokenSource source = new CancellationTokenSource();
    source.CancelAfter(5000);
    logger.Log("Trying to reconnect to SignalR",LogLevel.Info);
    bool reconnected = false;
    try{
      await hubConnection.StartAsync(source.Token);
      reconnected = true;
    }catch(Exception ex){
      if(ex is TaskCanceledException){
        logger.Log("Reconnecting to SignalR timed out",LogLevel.Warning);
      }else{
        logger.Log(ex.Message+": "+ex.StackTrace,LogLevel.Error);
      Thread.Sleep(1000);
    }
    }finally{
      source = new CancellationTokenSource();      
    }
    if(reconnected){
      logger.Log("Reconnected to SignalR",LogLevel.Info);
      break;
    }
  }
  logger.Log("Connected to SignalR", LogLevel.Info);
};

EverlinkConnection connection;
BlockingCollection<string> queryQueue = new BlockingCollection<string>();
hubConnection.On<string>("RunQuerry", async (query) =>{ 
  if(query.ToUpper().Contains("DELETE") || query.ToUpper().Contains("UPDATE") || query.ToUpper().Contains("INSERT"))
    logger.Log("Query rejected, only SELECT allowed",LogLevel.Warning);
  else
    queryQueue.Add(query);
  });
while(true){
  logger.Log("Trying to connect to SignalR",LogLevel.Info);
  try{
    await hubConnection.StartAsync();
    break;
  }catch(Exception ex){
    logger.Log(ex.Message+": "+ex.StackTrace, LogLevel.Error);
    await Task.Delay(1000);
  }
}
hubConnection.Closed += (e)=>
{
  logger.Log($"Disconnected from SignalR with reason: {e?.Message}",LogLevel.Warning);
  return Task.CompletedTask;
};
logger.Log("Connected to SignalR", LogLevel.Info);

while(true){
  try
  {
    connection = new EverlinkConnection(config.EverlinkServer, config.EverlinkPort, config.EverlinkUsername,config.EverlinkPassword,logger);
    await connection.ConnectAsync();
    logger.Log("Connected to Everlink", LogLevel.Info);
    while(true){
      CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
      cancellationTokenSource.CancelAfter(3000);
      try{
        var query = queryQueue.Take(cancellationTokenSource.Token);
        logger.Log($"Received query: {query}", LogLevel.Info);
        var result = await connection.QueryAsync(query);
        await hubConnection.InvokeAsync("SendQuerryResult", result);
      }catch(OperationCanceledException){
        await connection.SendHeartbeat();
      }
      
    }

  }catch(Exception ex)
  {
    logger.Log(ex.Message+": "+ex.StackTrace, LogLevel.Error);
    await Task.Delay(1000);    
    logger.Log("Reconnecting...",LogLevel.Info);
  }
}

//





