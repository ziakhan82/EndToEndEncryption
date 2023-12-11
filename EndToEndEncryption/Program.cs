// See https://aka.ms/new-console-template for more information

using ALice;
using crypto;
using System.IO;
using System.Net;


     private static SessionKeyExchange serverSessionKeyExchange = new SessionKeyExchange();
static async Task RunServer()
{
    var serverSocket = new SocketWrapper();
    await serverSocket.Listen(new IPEndPoint(IPAddress.Any, 1666));

    // Accept connections from Alice and Bob
    var aliceConnection = await serverSocket.AcceptClient();
    var bobConnection = await serverSocket.AcceptClient();

    // Exchange keys with Alice
    var alicePublicKeyBytes = await serverSocket.ReceiveBytes();
    var alicePublicKey = SessionKeyExchange.ImportPublicKey(alicePublicKeyBytes);
    var serverSessionKeyAlice = serverSessionKeyExchange.GenerateSessionKey(alicePublicKey, out var serverSessionKeyMessageAlice);
    await serverSocket.SendBytes(serverSessionKeyMessageAlice);

    // Exchange keys with Bob
    var bobPublicKeyBytes = await bobConnection.ReceiveBytes();
    var bobPublicKey = SessionKeyExchange.ImportPublicKey(bobPublicKeyBytes);
    var serverSessionKeyBob = serverSessionKeyExchange.GenerateSessionKey(bobPublicKey, out var serverSessionKeyMessageBob);
    await serverSocket.SendBytes(serverSessionKeyMessageBob);
}

static async Task HandleCommunication(SocketWrapper clientConnection, string clientName)
{
    Console.WriteLine($"Communication with {clientName} started.");

    // For example, receiving an encrypted message from the client
    var receivedBuffer = new byte[1024];
    var bytesRead = await stream.ReadAsync(receivedBuffer, 0, receivedBuffer.Length);
    Console.WriteLine($"{clientName} received encrypted message.");

    // Forward the received encrypted message to the other participant (Bob)
    await ForwardEncryptedMessageToOtherParticipant(receivedBuffer, bytesRead);

    Console.WriteLine($"Communication with {clientName} completed.");
}

static async Task ForwardEncryptedMessageToOtherParticipant(byte[] encryptedMessage, int length)
{
    // Forward the encrypted message to the other participant (Bob)
    // For example, you can send the encrypted message to Bob using another stream or a different communication channel
    // You may need to maintain a reference to Bob's connection (stream) in the server
    // Replace the following line with the appropriate logic to forward the message to Bob
    await BobStream.WriteAsync(encryptedMessage, 0, length);

    Console.WriteLine("Server forwarded encrypted message to Bob.");
}
    
