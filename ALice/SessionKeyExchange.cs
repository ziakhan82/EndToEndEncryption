using System.Security.Cryptography;

namespace crypto;

public class SessionKeyExchange
{
    // initializes an instance of ECDiffieHellman for key exchange.
    private readonly ECDiffieHellman _ecdh = ECDiffieHellman.Create();

    //exposes the public key of the current instance of ECDiffieHellman. The public key is used for sharing with the other party during key exchange.
    public ECDiffieHellmanPublicKey PublicKey => _ecdh.PublicKey;


    // This method generates a session key, 
    public byte[] GenerateSessionKey(out SecretMessage encryptedSessionKey, ECDiffieHellmanPublicKey partnerPublicKey)
    {

        var commonSecret = DeriveCommonSecret(partnerPublicKey);

        // Use a cryptographically secure pseudorandom number generator to generate 256 bits to use as session key
        var sessionKey = RandomNumberGenerator.GetBytes(32);

        // Encrypt session key using common secret
        encryptedSessionKey = SymmetricEncryption.Encrypt(sessionKey, commonSecret);

        return sessionKey;
    }

    public byte[] DecryptSessionKey(SecretMessage sessionKey, ECDiffieHellmanPublicKey partnerPublicKey)
    {
        var commonSecret = DeriveCommonSecret(partnerPublicKey);
        return SymmetricEncryption.Decrypt(sessionKey, commonSecret);
    }

    private byte[] DeriveCommonSecret(ECDiffieHellmanPublicKey partnerPublicKey)
    {
        // Derive the secret based on this side's private key and the other side's public key 
        var secret = _ecdh.DeriveKeyMaterial(partnerPublicKey);

        var aesKey = new byte[32]; // 256-bit AES key
        Array.Copy(secret, 0, aesKey, 0, 32); // Copy first 32 bytes as the key

        return aesKey;
    }

    public static ECDiffieHellmanPublicKey ImportPublicKey(byte[] bytes)
    {
        // creates an ECDiffieHellman instance and imports a public key from the provided byte array.
        var ecdh = ECDiffieHellman.Create();
        ecdh.ImportSubjectPublicKeyInfo(bytes, out var bytesRead);
        return ecdh.PublicKey;
    }
}