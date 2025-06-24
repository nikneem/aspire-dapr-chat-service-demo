using HexMaster.Chat.Members.Api.Services;

namespace HexMaster.Chat.Members.Api.BackgroundServices;

public class MemberCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MemberCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(15);

    public MemberCleanupService(
        IServiceProvider serviceProvider,
        ILogger<MemberCleanupService> logger)
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
                var memberService = scope.ServiceProvider.GetRequiredService<IMemberService>();

                _logger.LogInformation("Starting member cleanup process");
                await memberService.RemoveInactiveMembersAsync();
                _logger.LogInformation("Member cleanup process completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during member cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }
}
