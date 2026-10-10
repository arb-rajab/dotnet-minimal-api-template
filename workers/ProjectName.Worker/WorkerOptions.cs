namespace ProjectName.Worker;

public sealed class WorkerOptions
{
    public const string SectionName = "Worker";

    /// <summary>Delay between ticks.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);
}
