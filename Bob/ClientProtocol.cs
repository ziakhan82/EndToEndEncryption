using crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ALice
{
    public class ClientProtocol
    {
        public async Task Run()
        {
            var exchange = new SessionKeyExchange();

            using var socket = new SocketWrapper();
            await socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1666));

            // The client sends its public key bytes to the server using the SendBytes method.
            var ownPubKey = exchange.PublicKey.ExportSubjectPublicKeyInfo();
            await socket.SendBytes(ownPubKey);

            // The client waits to receive the public key bytes from the server using the ReceiveBytes method.
            var otherPubKeyBytes = await socket.ReceiveBytes();

            // The received public key bytes are imported using the ImportPublicKey method.
            var otherPubKey = SessionKeyExchange.ImportPublicKey(otherPubKeyBytes);

            // The client generates a session key and sends it as a JSON string to the server using the Send method.
            var sessionKey = exchange.GenerateSessionKey(out var encryptedSessionKey, otherPubKey);
            var sessionKeyJson = JsonSerializer.Serialize(encryptedSessionKey);
            await socket.Send(sessionKeyJson);

            Console.WriteLine("Session key sent to the server.");
        }
    }
}

