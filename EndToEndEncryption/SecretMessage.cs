namespace crypto;

public class SecretMessage
{
    // Cipher text
    public byte[] Cipher { init; get; }

    // Nonce aka Initialization Vector (IV)
    public byte[] Nonce { init; get; }

    // Authentication tag
    public byte[] Tag { init; get; }
}