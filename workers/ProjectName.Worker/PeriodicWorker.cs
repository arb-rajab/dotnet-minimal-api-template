using Microsoft.Extensions.Options;

namespace ProjectName.Worker;

/// <summary>The unit of work run on every tick.</summary>
public interface ITickHandler
{
    Task HandleAsync(CancellationToken cancellationToken);
}

public sealed class LoggingTickHandler(ILogger<LoggingTickHandler> logger) : ITickHandler
{
    public Task HandleAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Worker tick at {Time:O}", DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Runs <see cref="ITickHandler"/> on a fixed interval until the host stops. A
/// failing tick is logged and the loop continues, so one bad iteration does not
/// take the process down.
/// </summary>
public sealed class PeriodicWorker(
    ITickHandler handler,
    IOptions<WorkerOptions> options,
    ILogger<PeriodicWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.Interval);
        do
        {
            try
            {
                await handler.HandleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Worker tick failed");
            }
        }
        while (await WaitForNextTickAsync(timer, stoppingToken));
    }

    private static async Task<bool> WaitForNextTickAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try
        {
            return await timer.WaitForNextTickAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
