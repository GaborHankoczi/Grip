using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Adapter.EverlinkProtocol.Messages;

namespace Adapter.EverlinkProtocol
{
    public class EverlinkConnection
    {
        public bool IsConnected { get; private set; }
        public bool IsLoggedIn { get; private set; }
        public bool IsHandshakeComplete { get; private set; }
        public bool IsReady { get; private set;}

        public string Host { get; private set; }
        public int Port { get; private set; }
        public string Username { get; private set; }

        private TcpClient _client;
        private string password;

        private Logger logger;

        private object _keepAliveLock = new object();


        private long _lastKeepAliveBinary;
        private DateTime _lastKeepAlive {get => DateTime.FromBinary(Interlocked.CompareExchange(ref _lastKeepAliveBinary,0,0)); set { Interlocked.Exchange(ref _lastKeepAliveBinary,value.ToBinary());}}

        private SemaphoreSlim _keepAliveSemaphore = new SemaphoreSlim(1);

        private Thread KeepAliveThread;

        private async void KeepAlive(){
            while(true){
                await Task.Delay(1000);
                if((DateTime.Now - _lastKeepAlive).TotalSeconds > 3){
                    _lastKeepAlive = DateTime.Now;
                    await SendMessageAndAwaitResponse<BinaryMessage>(BinaryMessage.KEEPALIVE);
                }
            }
        }
        
        public EverlinkConnection(string host, int port, string username, string password, Logger logger)
        {
            Host = host;
            Port = port;
            Username = username;
            this.password = password;
            this.logger = logger;
            _client = new TcpClient();
            KeepAliveThread = new Thread(KeepAlive);
        }

        private async Task<byte[]> AwaitMessage(){
            var stream = _client.GetStream();
            var buffer = new byte[2048];
            var read = await stream.ReadAsync(buffer);
            logger.Log("Receiving message",Logger.LogLevel.Debug);
            var outputBuffer = new ArraySegment<byte>(buffer,0,read).ToArray();
            logger.LogHex(outputBuffer,Logger.LogLevel.Dump);
            return outputBuffer;
        }

        private async Task SendMessage(MessageBase message){
            var stream = _client.GetStream();
            byte[] messageData = message.Compile();
            logger.Log("Sending message",Logger.LogLevel.Debug);
            logger.LogHex(messageData,Logger.LogLevel.Dump);
            await stream.WriteAsync(messageData);
            _lastKeepAlive = DateTime.Now;
        }

        private async Task<T> SendMessageAndAwaitResponse<T>(MessageBase message) where T : MessageBase{
            await _keepAliveSemaphore.WaitAsync();
            try{
                await SendMessage(message);
                var responseMessage = Activator.CreateInstance(typeof(T),await AwaitMessage());
                if(responseMessage is null)
                    throw new Exception("Unable to parse response message");
                _keepAliveSemaphore.Release();
                return (T)responseMessage;
            }finally{
                _keepAliveSemaphore.Release();
            }
        }

        public async Task ConnectAsync(){
            await _client.ConnectAsync(Host, Port);
            IsConnected = true;
            new ServerGreetMessage(await AwaitMessage());
            var loginResult = await SendMessageAndAwaitResponse<LoginResultMessage>(new LoginMessage(Username, password));
            if(!loginResult.IsSuccess){
                throw new Exception("Login failed");
            }
            IsLoggedIn = true;
            var handshakeResult = await SendMessageAndAwaitResponse<BinaryMessage>(BinaryMessage.HANDSHAKE);
            // TODO check handshake result
            IsHandshakeComplete = true;
            IsReady = true;
            KeepAliveThread.Start();
        }

        public async Task<byte[]> QueryAsync(string query){
            if(!IsHandshakeComplete)
                throw new Exception("Handshake not complete");
            if(!IsReady)
                throw new Exception("Connection not ready");
            IsReady = false;
            var result = await SendMessageAndAwaitResponse<QueryResponseMessage>(new QueryMessage(query));
            var response = new QueryResponse();
            FileResponseMessage fileResponse;
            do
            {
                fileResponse = await SendMessageAndAwaitResponse<FileResponseMessage>(new FileRequestMessage(result.ResponseFileName));
                if(!fileResponse.IsReady)
                    await Task.Delay(200);
            }while(!fileResponse.IsReady);
            // Save first response and skip first 4 bytes
            response.Append(fileResponse.FileContent.Skip(4).ToArray());
            while(!response.IsComplete){
                response.Append((new FileResponseMessage(await AwaitMessage()).FileContent));
            }
            IsReady = true;
            return response.Data;
        }

    }
}