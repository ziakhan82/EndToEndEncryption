using System.Net;
using System.Text.Json;

namespace crypto;

public class ServerProtocol : Protocol
{
    public async Task Run()
    {
        var exchange = new SessionKeyExchange();

        using var socket = new SocketWrapper();
        await socket.Listen(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1666));

        //The server waits to receive the public key bytes from the client using the ReceiveBytes method.
        var otherPubKeyBytes = await socket.ReceiveBytes();

        //The received public key bytes are imported using the ImportPublicKey method
        var otherPubKey = SessionKeyExchange.ImportPublicKey(otherPubKeyBytes);

        // The server's own public key is obtained from the exchange instance and exported as bytes using ExportSubjectPublicKeyInfo.
        var ownPubKey = exchange.PublicKey.ExportSubjectPublicKeyInfo();

        //The server sends its own public key bytes to the client using the SendBytes method.
        await socket.SendBytes(ownPubKey);

        //The server waits to receive the session key as a JSON string from the client using the Receive method.
        var sessionKeyJson = await socket.Receive();

        //The received JSON string is deserialized into a SecretMessage object using the JsonSerializer.
        var secretMessage = JsonSerializer.Deserialize<SecretMessage>(sessionKeyJson)!;

        // decrypt the session key from the received SecretMessage using the other party's public key.
        var sessionKey = exchange.DecryptSessionKey(secretMessage, otherPubKey);
        
        Console.WriteLine("Session key: " + Convert.ToBase64String(sessionKey)); 
    }
}