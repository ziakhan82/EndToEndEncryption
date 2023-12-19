using crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ALice
{
    public class ClientProtocol
    {
        public async Task Run()
        {
            using var socket = new SocketWrapper();
            var exchange = new SessionKeyExchange();

            try
            {
                // Connect to the server
                await socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1666));

                // Send own public key to the server
                var ownPubKey = exchange.PublicKey.ExportSubjectPublicKeyInfo();
                await socket.SendBytes(ownPubKey);

                // Receive the other party's public key
                var otherPubKeyBytes = await socket.ReceiveBytes();
                var otherPubKey = SessionKeyExchange.ImportPublicKey(otherPubKeyBytes);

                // Generate a session key and send it to the server
                var sessionKey = exchange.GenerateSessionKey(out var secretMessage, otherPubKey);
                var sessionKeyJson = JsonSerializer.Serialize(secretMessage);
                await socket.Send(sessionKeyJson);

                Console.WriteLine("Session key: " + Convert.ToBase64String(sessionKey));

                // Start communication loop
                while (true)
                {
                    Console.Write("Enter a message to send (or type 'exit' to quit): ");
                    var message = Console.ReadLine();

                    if (message.ToLower() == "exit")
                        break;

                    // Encrypt the message using the session key
                    var encryptedMessage = SymmetricEncryption.Encrypt(Encoding.UTF8.GetBytes(message), sessionKey);
                    // Extract the cipher bytes from the SecretMessage
                    var cipherBytes = encryptedMessage.Cipher;

                    // await socket.SendBytes(encryptedMessage);

                    // Send the cipher bytes to the server
                    await socket.SendBytes(cipherBytes);

                    //Receive the encrypted message from the server
                    var receivedEncryptedMessage = await socket.ReceiveBytes();

                    // Decrypt the message using the session key
                    var decryptedSecretMessage = SymmetricEncryption.Decrypt(new SecretMessage
                    {
                        Cipher = receivedEncryptedMessage
                    }, sessionKey);

                    // Set the Nonce and Tag from the decrypted SecretMessage


                    // Decrypt the message using the session key, nonce, and tag
                    var decryptedMessage = SymmetricEncryption.Decrypt(new SecretMessage
                    {
                        Cipher = decryptedSecretMessage
                    }, sessionKey);

                    // Print the decrypted message
                    Console.WriteLine($"Received message: {Encoding.UTF8.GetString(decryptedMessage)}");
                }
            
            }
            finally
            {
                // Disconnect when done or in case of an exception
                socket.Disconnect();
            }
        }
    }

   
        }
    


