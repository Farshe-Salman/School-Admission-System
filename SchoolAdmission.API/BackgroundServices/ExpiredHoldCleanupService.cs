using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolAdmission.Business.Interfaces.Repositories;

namespace SchoolAdmission.API.BackgroundServices;

public class ExpiredHoldCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ExpiredHoldCleanupService> _logger;

    public ExpiredHoldCleanupService(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<ExpiredHoldCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(5));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireHoldsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while expiring pending holds.");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ExpireHoldsAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var holdRepository = scope.ServiceProvider
            .GetRequiredService<IAdmissionHoldRepository>();

        var nowUtc = _timeProvider.GetUtcNow();

        await holdRepository.ExpireAllPendingHoldsAsync(nowUtc);

        _logger.LogInformation(
            "Expired pending holds at {TimeUtc}.",
            nowUtc);
    }
}