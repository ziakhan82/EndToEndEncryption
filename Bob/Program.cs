// See https://aka.ms/new-console-template for more information
// Bob's public and private key pair
using System.Net;
using System.Text.Json;
using System.Text;
using crypto;

var bobExchange = new SessionKeyExchange();

// Connect to the server
using var bobSocket = new SocketWrapper();
await bobSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1666));

// Send Bob's public key to the server
var bobPubKeyBytes = bobExchange.PublicKey.ExportSubjectPublicKeyInfo();
await bobSocket.SendBytes(bobPubKeyBytes);

// Receive Alice's public key from the server
var alicePubKeyBytes = await bobSocket.ReceiveBytes();
var alicePubKey = SessionKeyExchange.ImportPublicKey(alicePubKeyBytes);

// Generate a session key and send it to the server
var bobSessionKey = bobExchange.GenerateSessionKey(out var bobSessionKeyMessage, alicePubKey);
var bobSessionKeyJson = JsonSerializer.Serialize(bobSessionKeyMessage);
await bobSocket.Send(bobSessionKeyJson);

Console.WriteLine("Bob's Session key: " + Convert.ToBase64String(bobSessionKey));

// Start communication loop
while (true)
{
    Console.Write("Bob: Enter a message to send to Alice (or type 'exit' to quit): ");
    var message = Console.ReadLine();

    if (message.ToLower() == "exit")
        break;

    // Encrypt the message using the session key
    var encryptedMessage = SymmetricEncryption.Encrypt(Encoding.UTF8.GetBytes(message), bobSessionKey);
    var cipherBytes = encryptedMessage.Cipher;

    // Send the encrypted message to the server
    await bobSocket.SendBytes(cipherBytes);

    Console.WriteLine("Bob: Message sent.");

    // Receive the encrypted message from the server
    var receivedEncryptedMessage = await bobSocket.ReceiveBytes();

    // Decrypt the message using the session key
    var decryptedMessage = SymmetricEncryption.Decrypt(new SecretMessage
    {
        Cipher = receivedEncryptedMessage
    }, bobSessionKey);

    // Print the decrypted message
    Console.WriteLine($"Bob: Received message from Alice: {Encoding.UTF8.GetString(decryptedMessage)}");
}

// Disconnect when done
bobSocket.Disconnect();
 
