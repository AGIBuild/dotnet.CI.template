using ChengYuan.BackgroundJobs;
using ChengYuan.BlobStoring;
using ChengYuan.Caching;
using ChengYuan.Core.Modularity;
using ChengYuan.DistributedLocking;
using ChengYuan.Emailing;
using ChengYuan.EventBus;
using ChengYuan.ExceptionHandling;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using ChengYuan.Notifications;
using ChengYuan.ObjectMapping;
using ChengYuan.Sms;
using ChengYuan.TextTemplating;

namespace ChengYuan.WebHost;

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MultiTenancyModule))]
[DependsOn(typeof(MemoryCachingModule))]
[DependsOn(typeof(EventBusModule))]
[DependsOn(typeof(ExceptionHandlingModule))]
[DependsOn(typeof(DistributedLockingModule))]
[DependsOn(typeof(BackgroundJobsModule))]
[DependsOn(typeof(BlobStoringModule))]
[DependsOn(typeof(EmailingModule))]
[DependsOn(typeof(SmsModule))]
[DependsOn(typeof(ObjectMappingModule))]
[DependsOn(typeof(TextTemplatingModule))]
[DependsOn(typeof(NotificationsModule))]
internal sealed class WebHostFrameworkCompositionModule : HostModule
{
}
