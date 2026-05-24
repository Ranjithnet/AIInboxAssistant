using AIInboxAssistant.Infrastructure.Data;
using AIInboxAssistant.Worker.Services;
using Microsoft.EntityFrameworkCore;

namespace AIInboxAssistant.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IServiceScopeFactory _scopeFactory;

        public Worker(
            ILogger<Worker> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;

            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var dbContext =
                scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var gmailReader = new GmailReaderService();

            await gmailReader.ReadEmailsAsync(dbContext);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Worker running at: {time}",
                    DateTimeOffset.Now);

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
