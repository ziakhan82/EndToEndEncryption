using System.Security.Cryptography;

namespace crypto;

public static class SymmetricEncryption
{
    public static SecretMessage Encrypt(byte[] plaintext, byte[] key)
    {
        //create a new instance of AesGcm using the provided key.
        using var aes = new AesGcm(key);
        // MaxSize = 12 bytes / 96 bits and this size should always be used.

        //Generates a new nonce using a secure random number generator.
        // A new nonce is generated with every encryption operation in line with the AES GCM security model
        var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);

        // Tag for authenticated encryption
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        // Ciphertext will be same length in bytes as plaintext 
        var cipher = new byte[plaintext.Length];

        // Perform the actual encryption
        aes.Encrypt(nonce, plaintext, cipher, tag);
        return new SecretMessage { Cipher = cipher, Nonce = nonce, Tag = tag };
    }

    public static byte[] Decrypt(SecretMessage secretMessage, byte[] key)
    {
        using var aes = new AesGcm(key);

        // Plaintext will be same length in bytes as Ciphertext 
        var plaintextBytes = new byte[secretMessage.Cipher.Length];

        // Perform the actual decryption
        aes.Decrypt(secretMessage.Nonce, secretMessage.Cipher, secretMessage.Tag, plaintextBytes);

        return plaintextBytes;
    }
}