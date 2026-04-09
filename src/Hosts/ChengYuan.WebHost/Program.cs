using ChengYuan.WebHost;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWebHostComposition();

var app = builder.Build();

app.MapWebHostEndpoints();

app.Run();

public partial class Program
{
}
