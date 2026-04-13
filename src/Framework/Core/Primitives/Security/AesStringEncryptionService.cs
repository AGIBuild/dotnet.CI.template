using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace ChengYuan.Core.Security;

internal sealed class AesStringEncryptionService : IStringEncryptionService
{
    private static readonly byte[] DefaultIv = "ChengYuan_IV1234"u8.ToArray();
    private readonly StringEncryptionOptions _options;

    public AesStringEncryptionService(IOptions<StringEncryptionOptions> options)
    {
        _options = options.Value;
    }

    public string Encrypt(string plainText)
    {
        ArgumentException.ThrowIfNullOrEmpty(plainText);

        using var aes = CreateAes();
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = PerformCryptography(encryptor, plainBytes);

        return Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string cipherText)
    {
        ArgumentException.ThrowIfNullOrEmpty(cipherText);

        using var aes = CreateAes();
        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = PerformCryptography(decryptor, cipherBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }

    private Aes CreateAes()
    {
        var aes = Aes.Create();
        aes.KeySize = _options.KeySize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.IV = _options.InitVectorBytes ?? DefaultIv;
        aes.Key = DeriveKey(_options.DefaultPassPhrase, aes.KeySize / 8);
        return aes;
    }

    private static byte[] DeriveKey(string passPhrase, int keyBytes)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            passPhrase,
            "ChengYuan_Salt"u8,
            iterations: 100_000,
            HashAlgorithmName.SHA256,
            keyBytes);
    }

    private static byte[] PerformCryptography(ICryptoTransform transform, byte[] data)
    {
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
        {
            cs.Write(data, 0, data.Length);
        }

        return ms.ToArray();
    }
}
