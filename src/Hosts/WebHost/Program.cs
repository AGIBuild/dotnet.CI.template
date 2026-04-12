using ChengYuan.EntityFrameworkCore;
using ChengYuan.WebHost;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=chengyuan-webhost.db";
builder.Services.UseSqlite(connectionString);
builder.Services.AddWebHostComposition();

var app = builder.Build();

app.UseWebHostComposition();

app.Run();

public partial class Program
{
}
