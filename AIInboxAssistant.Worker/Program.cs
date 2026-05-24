using AIInboxAssistant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIInboxAssistant.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();

            builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

            var host = builder.Build();
            host.Run();
        }
    }
}