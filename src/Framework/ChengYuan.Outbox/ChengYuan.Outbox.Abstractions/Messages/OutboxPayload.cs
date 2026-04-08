namespace ChengYuan.Outbox;

public sealed record OutboxPayload(byte[] Content, string TypeName);
