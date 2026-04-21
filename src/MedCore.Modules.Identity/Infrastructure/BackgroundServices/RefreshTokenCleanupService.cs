namespace MedCore.Modules.Identity.Infrastructure.BackgroundServices;

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
        _logger.LogInformation(
            "Refresh token cleanup service started. Interval: every {IntervalInHours} hour(s).",
            _settings.IntervalInHours);
        
        while (!ct.IsCancellationRequested)
        {
            await CleanupExpiredTokensAsync(ct);
            await Task.Delay(TimeSpan.FromHours(_settings.IntervalInHours), ct);
        }
    }

    private async Task CleanupExpiredTokensAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
            var deleted = await repository.DeleteExpiredAsync(ct);
            
            _logger.LogInformation(
                "Expired refresh token cleanup complete: {DeletedCount} token(s) deleted.",
                deleted);
        }
        catch (OperationCanceledException)
        {
            // App is shutting down. Exit gracefully.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Expired Refresh tokens cleanup failed.");
        }
    }
}