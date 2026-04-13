namespace ChengYuan.Core.Security;

public sealed class StringEncryptionOptions
{
    public string DefaultPassPhrase { get; set; } = "ChengYuan_Default_Key_DO_NOT_USE_IN_PRODUCTION";

    public byte[]? InitVectorBytes { get; set; }

    public int KeySize { get; set; } = 256;
}
