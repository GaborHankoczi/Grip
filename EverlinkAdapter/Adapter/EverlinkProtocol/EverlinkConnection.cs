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
        
        public EverlinkConnection(string host, int port, string username, string password)
        {
            Host = host;
            Port = port;
            Username = username;
            this.password = password;
            _client = new TcpClient();
        }

        private async Task<byte[]> AwaitMessage(){
            var stream = _client.GetStream();
            var buffer = new byte[2048];
            var read = await stream.ReadAsync(buffer);
            return buffer;
        }

        private async Task SendMessage(MessageBase message){
            var stream = _client.GetStream();
            await stream.WriteAsync(message.Compile());
        }

        private async Task<T> SendMessageAndAwaitResponse<T>(MessageBase message) where T : MessageBase{
            await SendMessage(message);
            var responseMessage = Activator.CreateInstance(typeof(T),await AwaitMessage());
            if(responseMessage is null)
                throw new Exception("Unable to parse response message");
            return (T)responseMessage;
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
            response.Append(fileResponse.FileContent);
            while(!response.IsComplete){
                response.Append((new FileResponseMessage(await AwaitMessage()).FileContent));
            }
            IsReady = true;
            return response.Data;
        }

    }
}