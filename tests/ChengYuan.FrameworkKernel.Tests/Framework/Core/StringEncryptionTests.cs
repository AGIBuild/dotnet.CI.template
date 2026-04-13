using ChengYuan.Core;
using ChengYuan.Core.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class StringEncryptionTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IStringEncryptionService _encryption;

    public StringEncryptionTests()
    {
        var services = new ServiceCollection();
        services.AddOptions<StringEncryptionOptions>().Configure(opts =>
        {
            opts.DefaultPassPhrase = "TestPassPhrase_12345!";
            opts.KeySize = 256;
        });
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<StringEncryptionOptions>>().Value);
        services.AddCoreRuntime();
        _serviceProvider = services.BuildServiceProvider();
        _encryption = _serviceProvider.GetRequiredService<IStringEncryptionService>();
    }

    [Fact]
    public void EncryptDecrypt_RoundTrip_ReturnsOriginal()
    {
        const string plainText = "Hello, ChengYuan!";

        var encrypted = _encryption.Encrypt(plainText);
        var decrypted = _encryption.Decrypt(encrypted);

        encrypted.ShouldNotBe(plainText);
        decrypted.ShouldBe(plainText);
    }

    [Fact]
    public void Encrypt_ProducesDifferentOutput_ForDifferentInputs()
    {
        var encrypted1 = _encryption.Encrypt("text1");
        var encrypted2 = _encryption.Encrypt("text2");

        encrypted1.ShouldNotBe(encrypted2);
    }

    [Fact]
    public void Decrypt_WithWrongPassPhrase_ReturnsNullOrThrows()
    {
        var encrypted = _encryption.Encrypt("secret");

        var services2 = new ServiceCollection();
        services2.AddOptions<StringEncryptionOptions>().Configure(opts =>
        {
            opts.DefaultPassPhrase = "WrongPassPhrase_99999!";
        });
        services2.AddCoreRuntime();
        using var sp2 = services2.BuildServiceProvider();
        var encryption2 = sp2.GetRequiredService<IStringEncryptionService>();

        // Decryption with wrong key should either return null or throw
        Should.Throw<Exception>(() => encryption2.Decrypt(encrypted));
    }

    [Fact]
    public void Encrypt_NullOrEmpty_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _encryption.Encrypt(null!));
        Should.Throw<ArgumentException>(() => _encryption.Encrypt(string.Empty));
    }

    public void Dispose() => _serviceProvider.Dispose();
}
