using System.Net;
using System.Net.Sockets;
using System.Text;

namespace crypto;

public class SocketWrapper : IDisposable
{
    private SessionKeyExchange _exchange = new();
    private TcpListener? _listener;
    private TcpClient? _tcpClient;

    public void Dispose()
    {
        _listener?.Stop();
        _tcpClient?.Close();
    }

    public async Task Connect(IPEndPoint ipEndPoint)
    {
        _tcpClient = new TcpClient();
        Console.WriteLine("Connecting...");

        await _tcpClient.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port);
    }

    public void Disconnect()
    {
        _tcpClient?.Close();
    }

    public async Task Listen(IPEndPoint ipEndPoint)
    {
        _listener = new TcpListener(ipEndPoint);
        _listener.Start();

        Console.WriteLine("Listening...");

        _tcpClient = await _listener.AcceptTcpClientAsync();
    }

    public async Task Send(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        await _tcpClient!.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
        Console.WriteLine($"Socket sent message: \"{message}\"");
    }

    public async Task SendBytes(byte[] message)
    {
        await _tcpClient!.GetStream().WriteAsync(message, 0, message.Length);
    }

    public async Task<string> Receive()
    {
        var buffer = new byte[1_024];
        var bytesRead = await _tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);
        var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine($"Socket received message: \"{message}\"");
        return message;
    }

    public async Task<byte[]> ReceiveBytes()
    {
        var buffer = new byte[1_024];
        var bytesRead = await _tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);
        return buffer[..bytesRead];
    }
    public async Task<TcpClient> AcceptClient()
    {
        return await _listener.AcceptTcpClientAsync();
    }
}
