using System.Net;
using System.Net.Sockets;
using System.Text;

var ipEndPoint = new IPEndPoint(IPAddress.Any, 5100);
TcpListener listener = new(ipEndPoint);

async void ConnectionHandler(TcpClient handler){
    try{
        await using NetworkStream stream = handler.GetStream();
        Console.WriteLine("Connected");
        await stream.WriteAsync(Encoding.UTF8.GetBytes("+EverlinkServer\r\n"));
        var buffer = new byte[2048];
        
        var read = await stream.ReadAsync(buffer);

        
        if(buffer.AsSpan(0, read).SequenceEqual(Encoding.UTF8.GetBytes("admin\012345\0")))
        {
            await stream.WriteAsync(Encoding.UTF8.GetBytes("+"));
            Console.WriteLine("Login success");
        }
        else
        {
            await stream.WriteAsync(Encoding.UTF8.GetBytes("-"));
            Console.WriteLine("Login failed");
            throw new Exception("Login failed");
        }

        read = await stream.ReadAsync(buffer);

        if(read!=1||buffer[0]!=0x1b){
            Console.WriteLine("Invalid command");
            throw new Exception("Invalid handshake");
        }
        var randomHandshakeBytes = new byte[]{ 0xdf, 0x05, 0xff, 0x64, 0x67, 0x45, 0x8b, 0x6b };
        await stream.WriteAsync(randomHandshakeBytes);
        while(true)
        {
            read = await stream.ReadAsync(buffer);
            if(read==0)
            {
                Console.WriteLine("Connection closed");
                throw new Exception("Conneection closed");
            }else if(buffer[0]==0x18&&buffer[1]==0x00)
            {
                Console.WriteLine("Query: "+Encoding.UTF8.GetString(buffer, 2, read-3));
                await stream.WriteAsync(Encoding.UTF8.GetBytes("EVERLINK_QUERY_RESULT_CLIENT2049_SQLINDEX0.zip\0"));
                continue;
            }else if(buffer[0]==0x13){
                Console.WriteLine("Download: "+Encoding.UTF8.GetString(buffer, 1, read-2));
                var message = await System.IO.File.ReadAllBytesAsync("message.zip");
                await stream.WriteAsync(new byte[]{0xbc,0x03,0x00,0x00}.Concat(message).ToArray());
                continue;
            }
            
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e.ToString());

    }finally{
        handler.Close();
    }
}

try
{    
    listener.Start();
    while(true){
        Console.WriteLine("Waiting for connection...");
        TcpClient handler = await listener.AcceptTcpClientAsync();
        Console.WriteLine("Client connected");
        // Start a new thread to handle the connection
        Thread clientThread = new Thread(() => ConnectionHandler(handler));
        clientThread.Start();
    }
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
finally
{
    listener.Stop();
}