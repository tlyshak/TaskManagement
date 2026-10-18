namespace TaskManagement.Tests.Infrastructure;

public sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
}
