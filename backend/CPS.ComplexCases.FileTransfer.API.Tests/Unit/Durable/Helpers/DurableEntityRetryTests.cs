using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Helpers;

public class DurableEntityRetryTests
{
    [Fact]
    public async Task ExecuteUntilNotNullAsync_ReturnsImmediately_WhenFirstAttemptSucceeds()
    {
        var attempts = 0;

        var result = await DurableEntityRetry.ExecuteUntilNotNullAsync(
            "GetEntityAsync",
            () =>
            {
                attempts++;
                return Task.FromResult<string?>("entity");
            },
            NullLogger.Instance,
            delayAsync: (_, _) => Task.CompletedTask);

        Assert.Equal("entity", result);
        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task ExecuteUntilNotNullAsync_RetriesUntilValueIsVisible()
    {
        var attempts = 0;

        var result = await DurableEntityRetry.ExecuteUntilNotNullAsync(
            "GetEntityAsync",
            () =>
            {
                attempts++;
                return Task.FromResult(attempts >= 3 ? "entity" : null);
            },
            NullLogger.Instance,
            delayAsync: (_, _) => Task.CompletedTask);

        Assert.Equal("entity", result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task ExecuteUntilNotNullAsync_ReturnsNull_AfterAllAttempts()
    {
        var attempts = 0;

        var result = await DurableEntityRetry.ExecuteUntilNotNullAsync(
            "GetEntityAsync",
            () =>
            {
                attempts++;
                return Task.FromResult<string?>(null);
            },
            NullLogger.Instance,
            cancellationToken: default,
            maxAttempts: 5,
            retryDelay: TimeSpan.Zero,
            delayAsync: (_, _) => Task.CompletedTask);

        Assert.Null(result);
        Assert.Equal(5, attempts);
    }
}
