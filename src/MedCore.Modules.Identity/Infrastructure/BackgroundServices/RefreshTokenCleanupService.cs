namespace MedCore.Modules.Identity.Infrastructure.BackgroundServices;

using MedCore.Modules.Identity.Application.Logging;
using MedCore.Modules.Identity.Configuration;
using MedCore.Modules.Identity.Domain.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal sealed class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenCleanupService> _logger;
    private readonly RefreshTokenCleanupSettings _settings;

    public RefreshTokenCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<RefreshTokenCleanupService> logger,
        IOptions<RefreshTokenCleanupSettings> settings)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = settings.Value;
    }
    
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        AuthLogMessages.RefreshTokenCleanupStarted(_logger, _settings.IntervalInHours, null);
        
        using var timer = new PeriodicTimer(TimeSpan.FromHours(_settings.IntervalInHours));
        
        while (await timer.WaitForNextTickAsync(ct))
        {
            await CleanupExpiredTokensAsync(ct);
        }
    }

    private async Task CleanupExpiredTokensAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
            var deleted = await repository.DeleteExpiredAsync(ct);
            
            AuthLogMessages.RefreshTokenCleanupSucceeded(
                _logger,
                deleted,
                DateTimeOffset.UtcNow.AddHours(_settings.IntervalInHours),
                null);
        }
        catch (OperationCanceledException)
        {
            // App is shutting down. Exit gracefully.
        }
        catch (Exception ex)
        {
            AuthLogMessages.RefreshTokenCleanupFailed(_logger, ex);
        }
    }
}