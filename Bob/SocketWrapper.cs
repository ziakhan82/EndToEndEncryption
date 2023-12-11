using System.Net;
using System.Net.Sockets;
using System.Text;

namespace crypto;

public class SocketWrapper : IDisposable
{
    private SessionKeyExchange _exchange = new();
    private Socket? _socket;

    public void Dispose()
    {
        _socket?.Dispose();
    }

    public async Task Connect(IPEndPoint ipEndPoint)
    {
        _socket = new Socket(
            ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        Console.WriteLine("Connecting...");

        await _socket.ConnectAsync(ipEndPoint);
    }

    public void Disconnect()
    {
        _socket?.Shutdown(SocketShutdown.Both);
    }

    public async Task Send(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        _ = await _socket!.SendAsync(messageBytes, SocketFlags.None);
        Console.WriteLine($"Socket sent message: \"{message}\"");
    }

    public async Task SendBytes(byte[] message)
    {
        _ = await _socket!.SendAsync(message, SocketFlags.None);
    }

    public async Task<string> Receive()
    {
        var buffer = new byte[1_024];
        var received = await _socket.ReceiveAsync(buffer, SocketFlags.None);
        var message = Encoding.UTF8.GetString(buffer, 0, received);
        Console.WriteLine($"Socket received message: \"{message}\"");
        return message;
    }

    public async Task<byte[]> ReceiveBytes()
    {
        var buffer = new byte[1_024];
        var received = await _socket.ReceiveAsync(buffer, SocketFlags.None);
        return buffer.Take(received).ToArray();
    }
}
