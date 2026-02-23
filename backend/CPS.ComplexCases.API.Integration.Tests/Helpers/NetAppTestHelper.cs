using CPS.ComplexCases.API.Integration.Tests.Fixtures;
using CPS.ComplexCases.NetApp.Models.Dto;
using Polly;
using Polly.Retry;

namespace CPS.ComplexCases.API.Integration.Tests.Helpers;

/// <summary>
/// Simple metadata record for NetApp object information.
/// </summary>
public record NetAppObjectMetadata(long ContentLength);

/// <summary>
/// Helper class for NetApp integration test operations.
/// </summary>
public static class NetAppTestHelper
{
    private static readonly ResiliencePipeline<bool> ExistsRetryPipeline = new ResiliencePipelineBuilder<bool>()
        .AddRetry(new RetryStrategyOptions<bool>
        {
            ShouldHandle = new PredicateBuilder<bool>().HandleResult(exists => !exists),
            MaxRetryAttempts = 10,
            Delay = TimeSpan.FromMilliseconds(100),
            BackoffType = DelayBackoffType.Exponential,
            MaxDelay = TimeSpan.FromSeconds(2)
        })
        .Build();

    /// <summary>
    /// Waits for an object to exist in NetApp storage using retry with exponential backoff.
    /// </summary>
    /// <param name="fixture">The integration test fixture containing NetApp client and configuration.</param>
    /// <param name="bearerToken">The bearer token for authentication.</param>
    /// <param name="objectKey">The full object key to check for existence.</param>
    /// <returns>True if the object exists; otherwise, false after all retries are exhausted.</returns>
    public static async Task<bool> WaitForObjectExistsAsync(
        IntegrationTestFixture fixture,
        string bearerToken,
        string objectKey)
    {
        return await ExistsRetryPipeline.ExecuteAsync(
            async _ => await ObjectExistsViaHeadObjectAsync(fixture, bearerToken, objectKey));
    }

    /// <summary>
    /// Waits until a condition is met using retry with exponential backoff.
    /// </summary>
    /// <param name="condition">The async condition to evaluate.</param>
    /// <returns>True if the condition was met; otherwise, false after all retries are exhausted.</returns>
    public static async Task<bool> WaitUntilAsync(Func<Task<bool>> condition)
    {
        return await ExistsRetryPipeline.ExecuteAsync(async _ => await condition());
    }

    /// <summary>
    /// Checks if an object exists in NetApp storage by sending a HEAD request for the object.
    /// </summary>
    /// <param name="fixture">The integration test fixture containing NetApp client and configuration.</param>
    /// <param name="bearerToken">The bearer token for authentication.</param>
    /// <param name="objectKey">The full object key to check for existence.</param>
    /// <returns>True if the object exists; otherwise, false.</returns>
    public static async Task<bool> ObjectExistsViaHeadObjectAsync(
        IntegrationTestFixture fixture,
        string bearerToken,
        string objectKey)
    {
        var headArg = fixture.NetAppArgFactory!.CreateGetObjectArg(
            bearerToken,
            fixture.NetAppBucketName!,
            objectKey);

        return await fixture.NetAppClient!.DoesObjectExistAsync(headArg);
    }

    /// <summary>
    /// Gets metadata for an object in NetApp storage by sending a HEAD request for the object.
    /// </summary>
    /// <param name="fixture">The integration test fixture containing NetApp client and configuration.</param>
    /// <param name="bearerToken">The bearer token for authentication.</param>
    /// <param name="objectKey">The full object key to get metadata for.</param>
    /// <returns>Metadata about the object including content length.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the object is not found.</exception>
    public static async Task<HeadObjectResponseDto> GetObjectMetadataAsync(
        IntegrationTestFixture fixture,
        string bearerToken,
        string objectKey)
    {
        var headArg = fixture.NetAppArgFactory!.CreateGetObjectArg(
           bearerToken,
           fixture.NetAppBucketName!,
           objectKey);

        var result = await fixture.NetAppClient!.GetHeadObjectMetadataAsync(headArg);

        if (result == null || result.StatusCode == System.Net.HttpStatusCode.NotFound)
            throw new FileNotFoundException($"Object not found: {objectKey}");

        return result;
    }
}
