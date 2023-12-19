// See https://aka.ms/new-console-template for more information
// Alice's public and private key pair
using System.Net;
using System.Text.Json;
using System.Text;
using crypto;

var aliceExchange = new SessionKeyExchange();

// Connect to the server
using var aliceSocket = new SocketWrapper();
await aliceSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1666));

// Send Alice's public key to the server
var alicePubKeyBytes = aliceExchange.PublicKey.ExportSubjectPublicKeyInfo();
await aliceSocket.SendBytes(alicePubKeyBytes);

// Receive Bob's public key from the server
var bobPubKeyBytes = await aliceSocket.ReceiveBytes();
var bobPubKey = SessionKeyExchange.ImportPublicKey(bobPubKeyBytes);

// Generate a session key and send it to the server
var aliceSessionKey = aliceExchange.GenerateSessionKey(out var aliceSessionKeyMessage, bobPubKey);
var aliceSessionKeyJson = JsonSerializer.Serialize(aliceSessionKeyMessage);
await aliceSocket.Send(aliceSessionKeyJson);

Console.WriteLine("Alice's Session key: " + Convert.ToBase64String(aliceSessionKey));

// Start communication loop
while (true)
{
    Console.Write("Alice: Enter a message to send to Bob (or type 'exit' to quit): ");
    var message = Console.ReadLine();

    if (message.ToLower() == "exit")
        break;

    // Encrypt the message using the session key
    var encryptedMessage = SymmetricEncryption.Encrypt(Encoding.UTF8.GetBytes(message), aliceSessionKey);
    var cipherBytes = encryptedMessage.Cipher;

    // Send the encrypted message to the server
    await aliceSocket.SendBytes(cipherBytes);

    Console.WriteLine("Alice: Message sent.");

    // Receive the encrypted message from the server
    var receivedEncryptedMessage = await aliceSocket.ReceiveBytes();

    // Decrypt the message using the session key
    var decryptedMessage = SymmetricEncryption.Decrypt(new SecretMessage
    {
        Cipher = receivedEncryptedMessage
    }, aliceSessionKey);

    // Print the decrypted message
    Console.WriteLine($"Alice: Received message from Bob: {Encoding.UTF8.GetString(decryptedMessage)}");
}

// Disconnect when done
aliceSocket.Disconnect();
        

