using System.Net;
using System.Net.Sockets;
using System.Text;

namespace crypto;

public class SocketWrapper : IDisposable
{
    private TcpListener? _listener;
    private TcpClient? _tcpClient;

    public NetworkStream? AliceStream { get; set; }
    public NetworkStream? BobStream { get; set; }

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

    public void SetAliceStream(NetworkStream stream)
    {
        AliceStream = stream;
    }

    public void SetBobStream(NetworkStream stream)
    {
        BobStream = stream;
    }

    public async Task HandleCommunication(NetworkStream stream, string participantName)
    {
        Console.WriteLine($"Communication with {participantName} started.");

        if (participantName == "Alice")
        {
            SetAliceStream(stream);
        }
        else if (participantName == "Bob")
        {
            SetBobStream(stream);
        }

        // For example, receiving an encrypted message from the client
        var receivedBuffer = await ReceiveBytes();
        Console.WriteLine($"{participantName} received encrypted message.");

        // Forward the received encrypted message to the other participant
        if (participantName == "Alice")
        {
            await ForwardEncryptedMessageTo("Bob", receivedBuffer);
        }
        else if (participantName == "Bob")
        {
            await ForwardEncryptedMessageTo("Alice", receivedBuffer);
        }

        Console.WriteLine($"Communication with {participantName} completed.");
    }

    public async Task ForwardEncryptedMessageTo(string recipient, byte[] encryptedMessage)
    {
        // Forward the encrypted message to the specified recipient (Alice or Bob)
        // Replace the following lines with the appropriate logic to forward the message to the recipient
        var recipientStream = recipient == "Alice" ? AliceStream : BobStream;
        await recipientStream?.WriteAsync(encryptedMessage, 0, encryptedMessage.Length);
        Console.WriteLine($"Server forwarded encrypted message to {recipient}.");
    }
    public async Task<SocketWrapper> Accept()
    {
        var acceptedTcpClient = await _listener!.AcceptTcpClientAsync();
        var acceptedSocketWrapper = new SocketWrapper { _tcpClient = acceptedTcpClient };
        return acceptedSocketWrapper;
    }
}
