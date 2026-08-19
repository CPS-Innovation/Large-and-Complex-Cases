using CPS.ComplexCases.API.Domain.Models;

namespace CPS.ComplexCases.API.Services;

public interface IUserBucketAccessService
{
    Task<IReadOnlyList<SecurityGroup>> GetEntitledBucketsAsync(string bearerToken);

    Task<SecurityGroup> EnsureBucketAllowedAsync(string bearerToken, string bucketName);

    /// <summary>
    /// Resolves the bucket to work against, preferring the value persisted on the case, then any
    /// bucket supplied by the caller. Both are validated against the user's entitlements. When
    /// neither is available the first entitlement is used, preserving pre-existing behaviour.
    /// </summary>
    Task<SecurityGroup> ResolveBucketAsync(string bearerToken, string? persistedBucketName, string? requestedBucketName);
}
