using HexMaster.Chat.Messages.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HexMaster.Chat.Messages.BackgroundServices;

public class MessageCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MessageCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

    public MessageCleanupService(
        IServiceProvider serviceProvider,
        ILogger<MessageCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();

                _logger.LogInformation("Starting message cleanup process");
                await messageService.RemoveExpiredMessagesAsync();
                _logger.LogInformation("Message cleanup process completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during message cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }
}
