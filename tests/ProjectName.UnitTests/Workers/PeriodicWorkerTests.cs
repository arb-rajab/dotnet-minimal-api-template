using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ProjectName.Worker;

namespace ProjectName.UnitTests.Workers;

public class PeriodicWorkerTests
{
    private sealed class CountingHandler(int failFirst = 0) : ITickHandler
    {
        private int _calls;

        public int Calls => Volatile.Read(ref _calls);

        public Task HandleAsync(CancellationToken cancellationToken)
        {
            var n = Interlocked.Increment(ref _calls);
            if (n <= failFirst)
            {
                throw new InvalidOperationException("boom");
            }

            return Task.CompletedTask;
        }
    }

    private static PeriodicWorker CreateWorker(ITickHandler handler) =>
        new(handler,
            Options.Create(new WorkerOptions { Interval = TimeSpan.FromMilliseconds(10) }),
            NullLogger<PeriodicWorker>.Instance);

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }
    }

    [Fact]
    public async Task RunsHandlerRepeatedlyUntilStopped()
    {
        var handler = new CountingHandler();
        var worker = CreateWorker(handler);

        await worker.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => handler.Calls >= 3);
        await worker.StopAsync(CancellationToken.None);

        Assert.True(handler.Calls >= 3);
        var callsAtStop = handler.Calls;
        await Task.Delay(50);
        Assert.Equal(callsAtStop, handler.Calls);
    }

    [Fact]
    public async Task FailingTick_DoesNotStopTheLoop()
    {
        var handler = new CountingHandler(failFirst: 2);
        var worker = CreateWorker(handler);

        await worker.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => handler.Calls >= 4);
        await worker.StopAsync(CancellationToken.None);

        Assert.True(handler.Calls >= 4);
    }
}
