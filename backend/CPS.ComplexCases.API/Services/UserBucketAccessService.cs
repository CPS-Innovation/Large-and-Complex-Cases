using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Exceptions;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Services;

public class UserBucketAccessService(
    ILogger<UserBucketAccessService> logger,
    ISecurityGroupMetadataService securityGroupMetadataService) : IUserBucketAccessService
{
    private readonly ILogger<UserBucketAccessService> _logger = logger;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;

    public async Task<IReadOnlyList<SecurityGroup>> GetEntitledBucketsAsync(string bearerToken)
    {
        // Throws MissingSecurityGroupException when the user matches no mapping, which the
        // exception middleware surfaces as a 403.
        return await _securityGroupMetadataService.GetUserSecurityGroupsAsync(bearerToken);
    }

    public async Task<SecurityGroup> EnsureBucketAllowedAsync(string bearerToken, string bucketName)
    {
        var entitledBuckets = await GetEntitledBucketsAsync(bearerToken);

        return FindEntitledBucket(entitledBuckets, bucketName)
            ?? throw BucketNotAllowed(bucketName);
    }

    public async Task<SecurityGroup> ResolveBucketAsync(string bearerToken, string? persistedBucketName, string? requestedBucketName)
    {
        var entitledBuckets = await GetEntitledBucketsAsync(bearerToken);

        if (!string.IsNullOrEmpty(persistedBucketName))
        {
            return FindEntitledBucket(entitledBuckets, persistedBucketName)
                ?? throw BucketNotAllowed(persistedBucketName);
        }

        if (!string.IsNullOrEmpty(requestedBucketName))
        {
            return FindEntitledBucket(entitledBuckets, requestedBucketName)
                ?? throw BucketNotAllowed(requestedBucketName);
        }

        var fallback = entitledBuckets[0];

        if (entitledBuckets.Count > 1)
        {
            _logger.LogWarning(
                "No bucket was persisted or requested and the user is entitled to {BucketCount} buckets. Defaulting to {BucketName}.",
                entitledBuckets.Count, fallback.BucketName);
        }

        return fallback;
    }

    private static SecurityGroup? FindEntitledBucket(IReadOnlyList<SecurityGroup> entitledBuckets, string bucketName) =>
        entitledBuckets.FirstOrDefault(x => string.Equals(x.BucketName, bucketName, StringComparison.OrdinalIgnoreCase));

    private MissingSecurityGroupException BucketNotAllowed(string bucketName)
    {
        _logger.LogWarning("User is not entitled to bucket {BucketName}.", bucketName);
        return new MissingSecurityGroupException($"User is not entitled to bucket '{bucketName}'.");
    }
}
