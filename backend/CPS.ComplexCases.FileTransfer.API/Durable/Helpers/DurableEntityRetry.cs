using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Helpers;

/// <summary>
/// Retries transient gRPC failures when calling the Durable Task management API from inside
/// activities. Targets <see cref="StatusCode.Unavailable"/> RpcExceptions and the underlying
/// socket failures (e.g. the WSAEACCES "An attempt was made to access a socket in a way
/// forbidden by its access permissions" error seen under local port pressure on Windows hosts).
/// </summary>
public static class DurableEntityRetry
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(1);

    public static async Task<T> ExecuteAsync<T>(
        string operationName,
        Func<Task<T>> action,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        for (var attempt = 1; attempt < MaxAttempts; attempt++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                logger.LogWarning(
                    ex,
                    "Transient gRPC failure on Durable entity operation {Operation} (attempt {Attempt}/{MaxAttempts}). Retrying in {DelaySeconds}s.",
                    operationName, attempt, MaxAttempts, Delay.TotalSeconds);

                await Task.Delay(Delay, cancellationToken);
            }
        }

        // Final attempt: let any failure (transient or not) propagate to the caller.
        return await action();
    }

    public static Task ExecuteAsync(
        string operationName,
        Func<Task> action,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            operationName,
            async () =>
            {
                await action();
                return true;
            },
            logger,
            cancellationToken);
    }

    private static bool IsTransient(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current is RpcException rpc && rpc.StatusCode == StatusCode.Unavailable)
            {
                return true;
            }

            if (current is System.Net.Sockets.SocketException)
            {
                return true;
            }
        }

        return false;
    }
}