using ChengYuan.Core.Modularity;

namespace ChengYuan.Sms;

public sealed class SmsModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSms();
    }
}
