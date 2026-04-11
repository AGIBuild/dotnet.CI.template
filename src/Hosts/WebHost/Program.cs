using ChengYuan.MultiTenancy;
using ChengYuan.WebHost;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWebHostComposition();
builder.Services.AddMultiTenancy();

var app = builder.Build();

app.UseMultiTenancy();
app.MapWebHostEndpoints();

app.Run();

public partial class Program
{
}
