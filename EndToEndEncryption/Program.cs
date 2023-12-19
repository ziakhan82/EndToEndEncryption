// See https://aka.ms/new-console-template for more information

using ALice;
using crypto;
using System.IO;
using System.Net;



static async Task RunServer()
{
    // Start the server
    using var serverSocket = new SocketWrapper();
    await serverSocket.Listen(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1666));

    Console.WriteLine("Server is running. Waiting for connections...");

    // Accept connections from both Alice and Bob
    using var aliceSocket = await serverSocket.Accept();
    using var bobSocket = await serverSocket.Accept();

    Console.WriteLine("Connections established with Alice and Bob.");

    // Start the key exchange between Alice and Bob
    var alicePubKeyBytes = await aliceSocket.ReceiveBytes();
    await bobSocket.SendBytes(alicePubKeyBytes);

    var bobPubKeyBytes = await bobSocket.ReceiveBytes();
    await aliceSocket.SendBytes(bobPubKeyBytes);

    Console.WriteLine("Public keys exchanged between Alice and Bob.");

    // Receive and forward session key between Alice and Bob
    var aliceSessionKeyJson = await aliceSocket.Receive();
    await bobSocket.Send(aliceSessionKeyJson);

    var bobSessionKeyJson = await bobSocket.Receive();
    await aliceSocket.Send(bobSessionKeyJson);

    Console.WriteLine("Session keys exchanged between Alice and Bob.");

    // Communication loop between Alice and Bob via the server
    while (true)
    {
        var aliceMessage = await aliceSocket.ReceiveBytes();
        await bobSocket.SendBytes(aliceMessage);

        var bobMessage = await bobSocket.ReceiveBytes();
        await aliceSocket.SendBytes(bobMessage);
    }
}
RunServer();


