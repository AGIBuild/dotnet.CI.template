namespace ChengYuan.Core.Security;

public interface IStringEncryptionService
{
    string Encrypt(string plainText);

    string Decrypt(string cipherText);
}
