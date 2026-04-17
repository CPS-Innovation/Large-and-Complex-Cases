using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Amazon.S3;
using Amazon.S3.Model;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.NetApp.Constants;
using CPS.ComplexCases.NetApp.Enums;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using CPS.ComplexCases.NetApp.Wrappers;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace CPS.ComplexCases.NetApp.Client;

public class NetAppClient(
    ILogger<NetAppClient> logger,
    IOptions<NetAppOptions> options,
    IAmazonS3UtilsWrapper amazonS3UtilsWrapper,
    INetAppRequestFactory netAppRequestFactory,
    INetAppArgFactory netAppArgFactory,
    IS3ClientFactory s3ClientFactory,
    INetAppS3HttpClient netAppS3HttpClient,
    INetAppS3HttpArgFactory netAppS3HttpArgFactory) : INetAppClient
{
    private readonly ILogger<NetAppClient> _logger = logger;
    private readonly NetAppOptions _options = options.Value;
    private readonly IAmazonS3UtilsWrapper _amazonS3UtilsWrapper = amazonS3UtilsWrapper;
    private readonly INetAppRequestFactory _netAppRequestFactory = netAppRequestFactory;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly IS3ClientFactory _s3ClientFactory = s3ClientFactory;
    private readonly INetAppS3HttpClient _netAppS3HttpClient = netAppS3HttpClient;
    private readonly INetAppS3HttpArgFactory _netAppS3HttpArgFactory = netAppS3HttpArgFactory;

    public async Task<bool> CreateBucketAsync(CreateBucketArg arg)
    {
        try
        {
            return await CreateBucketCoreAsync(arg);
        }
        catch (AmazonS3Exception ex) when (IsCredentialError(ex))
        {
            _logger.LogWarning(ex,
                "Credential error in CreateBucketAsync (ErrorCode={ErrorCode}) - invalidating and retrying once",
                ex.ErrorCode);
            await _s3ClientFactory.InvalidateClientAsync();
            try
            {
                return await CreateBucketCoreAsync(arg);
            }
            catch (AmazonS3Exception retryEx) when (retryEx.ErrorCode == S3ErrorCodes.AccessDenied)
            {
                throw new NetAppAccessDeniedException(arg.BucketName, retryEx);
            }
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, ex.Message, "Failed to create bucket with name {BucketName}", arg.BucketName);
            return false;
        }
    }

    private async Task<bool> CreateBucketCoreAsync(CreateBucketArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);

        if (string.IsNullOrEmpty(arg.BucketName))
        {
            _logger.LogWarning("Bucket name is null or empty.");
            return false;
        }

        var bucketExists = await _amazonS3UtilsWrapper.DoesS3BucketExistV2Async(s3Client, arg.BucketName);
        if (bucketExists)
        {
            _logger.LogInformation("Bucket with name {BucketName} already exists.", arg.BucketName);
            return false;
        }

        var response = await s3Client.PutBucketAsync(_netAppRequestFactory.CreateBucketRequest(arg));
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<IEnumerable<S3Bucket>> ListBucketsAsync(ListBucketsArg arg)
    {
        try
        {
            return await ListBucketsCoreAsync(arg);
        }
        catch (AmazonS3Exception ex) when (IsCredentialError(ex))
        {
            _logger.LogWarning(ex,
                "Credential error in ListBucketsAsync (ErrorCode={ErrorCode}) - invalidating and retrying once",
                ex.ErrorCode);
            await _s3ClientFactory.InvalidateClientAsync();
            try
            {
                return await ListBucketsCoreAsync(arg);
            }
            catch (AmazonS3Exception retryEx) when (retryEx.ErrorCode == S3ErrorCodes.AccessDenied)
            {
                throw new NetAppAccessDeniedException(arg.BucketName, retryEx);
            }
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, ex.Message, "Failed to list buckets.");
            return [];
        }
    }

    private async Task<IEnumerable<S3Bucket>> ListBucketsCoreAsync(ListBucketsArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);
        var response = await s3Client.ListBucketsAsync(_netAppRequestFactory.ListBucketsRequest(arg));
        return response.Buckets ?? [];
    }

    public async Task<S3Bucket?> FindBucketAsync(FindBucketArg arg)
    {
        try
        {
            var buckets = await ListBucketsAsync(new ListBucketsArg
            {
                BearerToken = arg.BearerToken,
                BucketName = arg.BucketName
            });

            return buckets?.SingleOrDefault(x => x.BucketName == arg.BucketName);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, ex.Message, "Failed to find bucket {BucketName}.", arg.BucketName);
            return null;
        }
    }

    public Task<bool> CreateFolderAsync(CreateFolderArg arg)
        => ExecuteWithCredentialRetryAsync(() => CreateFolderCoreAsync(arg), nameof(CreateFolderAsync), arg.BucketName);

    private async Task<bool> CreateFolderCoreAsync(CreateFolderArg arg)
    {
        var success = await _netAppS3HttpClient.PutFolderAsync(arg);
        if (!success)
        {
            _logger.LogWarning(
                "Failed to create folder {FolderKey} in bucket {BucketName}.",
                arg.FolderKey, arg.BucketName);
        }
        return success;
    }

    public Task<GetObjectResponse?> GetObjectAsync(GetObjectArg arg)
        => ExecuteWithCredentialRetryAsync(() => GetObjectCoreAsync(arg), nameof(GetObjectAsync), arg.BucketName);

    private async Task<GetObjectResponse?> GetObjectCoreAsync(GetObjectArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);

        try
        {
            var response = await s3Client.GetObjectAsync(_netAppRequestFactory.GetObjectRequest(arg));

            return response;
        }
        catch (AmazonS3Exception ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Object {ObjectKey} not found in bucket {BucketName}.", arg.ObjectKey,
                    arg.BucketName);
                throw new FileNotFoundException($"Object {arg.ObjectKey} not found in bucket {arg.BucketName}.", ex);
            }

            if (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                _logger.LogWarning("ETag mismatch for object {ObjectKey} in bucket {BucketName}.", arg.ObjectKey,
                    arg.BucketName);
                return null;
            }

            _logger.LogError(ex, ex.Message, "Failed to get file {ObjectKey} from bucket {BucketName}.", arg.ObjectKey,
                arg.BucketName);
            throw;
        }
    }

    public Task<bool> UploadObjectAsync(UploadObjectArg arg)
        => ExecuteWithCredentialRetryAsync(() => UploadObjectCoreAsync(arg), nameof(UploadObjectAsync), arg.BucketName);

    private async Task<bool> UploadObjectCoreAsync(UploadObjectArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);
        try
        {
            var response = await s3Client.PutObjectAsync(_netAppRequestFactory.UploadObjectRequest(arg));
            return response.HttpStatusCode == HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, ex.Message, "Failed to upload file {ObjectKey} to bucket {BucketName}.", arg.ObjectKey,
                arg.BucketName);
            throw;
        }
    }

    public async Task<ListNetAppObjectsDto?> ListObjectsInBucketAsync(ListObjectsInBucketArg arg)
    {
        try
        {
            return await ListObjectsInBucketCoreAsync(arg);
        }
        catch (AmazonS3Exception ex) when (IsCredentialError(ex))
        {
            _logger.LogWarning(ex,
                "Credential error in ListObjectsInBucketAsync (ErrorCode={ErrorCode}) - invalidating and retrying once",
                ex.ErrorCode);
            await _s3ClientFactory.InvalidateClientAsync();
            try
            {
                return await ListObjectsInBucketCoreAsync(arg);
            }
            catch (AmazonS3Exception retryEx) when (retryEx.ErrorCode == S3ErrorCodes.AccessDenied)
            {
                throw new NetAppAccessDeniedException(arg.BucketName, retryEx);
            }
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to list objects in bucket {BucketName}.", arg.BucketName);
            return null;
        }
    }

    private async Task<ListNetAppObjectsDto?> ListObjectsInBucketCoreAsync(ListObjectsInBucketArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);
        var request = _netAppRequestFactory.ListObjectsInBucketRequest(arg);
        var response = await s3Client.ListObjectsV2Async(request);

        var folders = (response.CommonPrefixes ?? []).Select(data => new ListNetAppFolderDataDto
        {
            Path = data
        });

        var files = (response.S3Objects ?? []).Select(data => new ListNetAppFileDataDto
        {
            Path = data.Key,
            Etag = data.ETag,
            Filesize = data.Size ?? 0,
            LastModified = data.LastModified ?? DateTime.MinValue
        });

        return new ListNetAppObjectsDto
        {
            Data = new ListNetAppDataDto
            {
                BucketName = arg.BucketName,
                RootPath = arg.Prefix,
                FolderData = folders,
                FileData = files
            },
            Pagination = new PaginationDto
            {
                ContinuationToken = response.ContinuationToken,
                NextContinuationToken = response.NextContinuationToken,
                MaxKeys = response.MaxKeys,
                KeyCount = response.KeyCount ?? 0
            }
        };
    }

    public async Task<ListNetAppObjectsDto?> ListFoldersInBucketAsync(ListFoldersInBucketArg arg)
    {
        try
        {
            return await ListFoldersInBucketCoreAsync(arg);
        }
        catch (AmazonS3Exception ex) when (IsCredentialError(ex))
        {
            _logger.LogWarning(ex,
                "Credential error in ListFoldersInBucketAsync (ErrorCode={ErrorCode}) - invalidating and retrying once",
                ex.ErrorCode);
            await _s3ClientFactory.InvalidateClientAsync();
            try
            {
                return await ListFoldersInBucketCoreAsync(arg);
            }
            catch (AmazonS3Exception retryEx) when (retryEx.ErrorCode == S3ErrorCodes.AccessDenied)
            {
                throw new NetAppAccessDeniedException(arg.BucketName, retryEx);
            }
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to list objects in bucket {BucketName}.", arg.BucketName);
            return null;
        }
    }

    private async Task<ListNetAppObjectsDto?> ListFoldersInBucketCoreAsync(ListFoldersInBucketArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);
        var response = await s3Client.ListObjectsV2Async(_netAppRequestFactory.ListFoldersInBucketRequest(arg));
        var folders = (response.CommonPrefixes ?? []).Select(data => new ListNetAppFolderDataDto
        {
            Path = data
        });

        return new ListNetAppObjectsDto
        {
            Data = new ListNetAppDataDto
            {
                BucketName = arg.BucketName,
                RootPath = arg.Prefix,
                FolderData = folders,
                FileData = []
            },
            Pagination = new PaginationDto
            {
                ContinuationToken = response.ContinuationToken,
                NextContinuationToken = response.NextContinuationToken,
                MaxKeys = response.MaxKeys,
                KeyCount = response.KeyCount ?? 0
            }
        };
    }

    public async Task<InitiateMultipartUploadResponse?> InitiateMultipartUploadAsync(InitiateMultipartUploadArg arg)
    {
        try
        {
            return await InitiateMultipartUploadCoreAsync(arg);
        }
        catch (AmazonS3Exception ex) when (IsCredentialError(ex))
        {
            _logger.LogWarning(ex,
                "Credential error in InitiateMultipartUploadAsync (ErrorCode={ErrorCode}) - invalidating and retrying once",
                ex.ErrorCode);
            await _s3ClientFactory.InvalidateClientAsync();
            try
            {
                return await InitiateMultipartUploadCoreAsync(arg);
            }
            catch (AmazonS3Exception retryEx) when (retryEx.ErrorCode == S3ErrorCodes.AccessDenied)
            {
                throw new NetAppAccessDeniedException(arg.BucketName, retryEx);
            }
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate multipart upload for file {ObjectKey}.", arg.ObjectKey);
            return null;
        }
    }

    private async Task<InitiateMultipartUploadResponse?> InitiateMultipartUploadCoreAsync(
        InitiateMultipartUploadArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);
        return await s3Client.InitiateMultipartUploadAsync(_netAppRequestFactory.CreateMultipartUploadRequest(arg));
    }

    public async Task<UploadPartResponse?> UploadPartAsync(UploadPartArg arg)
    {
        var retryPolicy = GetUploadPartRetryPolicy(arg.PartNumber, arg.ObjectKey);
        try
        {
            return await retryPolicy.ExecuteAsync(async () =>
            {
                // Re-resolve the S3 client on every attempt so that a credential
                // rotation triggered by a sibling task or another environment is picked up.
                var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);
                using var partStream = new MemoryStream(arg.PartData, writable: false);

                var request = new UploadPartRequest
                {
                    BucketName = arg.BucketName,
                    Key = arg.ObjectKey,
                    UploadId = arg.UploadId,
                    PartNumber = arg.PartNumber,
                    PartSize = arg.PartData.Length,
                    InputStream = partStream,
                    DisablePayloadSigning = true
                };
                return await s3Client.UploadPartAsync(request);
            });
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == S3ErrorCodes.AccessDenied)
        {
            throw new NetAppAccessDeniedException(arg.BucketName, ex);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex,
                "Failed to upload part {PartNumber} for file {ObjectKey} after all retry attempts. StatusCode={StatusCode}, ErrorCode={ErrorCode}",
                arg.PartNumber, arg.ObjectKey, ex.StatusCode, ex.ErrorCode);
            throw;
        }
    }

    public async Task<CompleteMultipartUploadResponse?> CompleteMultipartUploadAsync(CompleteMultipartUploadArg arg,
        CancellationToken cancellationToken = default)
    {
        var retryPolicy = GetCompleteMultipartUploadRetryPolicy(arg.UploadId, arg.ObjectKey);
        try
        {
            return await retryPolicy.ExecuteAsync(async ct =>
            {
                var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);
                return await s3Client.CompleteMultipartUploadAsync(
                    _netAppRequestFactory.CompleteMultipartUploadRequest(arg), ct);
            }, cancellationToken);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == S3ErrorCodes.AccessDenied)
        {
            throw new NetAppAccessDeniedException(arg.BucketName, ex);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex,
                "Failed to complete multipart upload {UploadId} for file {ObjectKey} after all retry attempts.",
                arg.UploadId, arg.ObjectKey);
            throw;
        }
    }

    public async Task AbortMultipartUploadAsync(AbortMultipartUploadArg arg)
    {
        try
        {
            await AbortMultipartUploadCoreAsync(arg);
        }
        catch (AmazonS3Exception ex) when (IsCredentialError(ex))
        {
            _logger.LogWarning(ex,
                "Credential error in AbortMultipartUploadAsync (ErrorCode={ErrorCode}) - invalidating and retrying once",
                ex.ErrorCode);
            await _s3ClientFactory.InvalidateClientAsync();
            try
            {
                await AbortMultipartUploadCoreAsync(arg);
            }
            catch (AmazonS3Exception retryEx)
            {
                _logger.LogError(retryEx,
                    "Failed to abort multipart upload {UploadId} for {ObjectKey} after credential refresh.",
                    arg.UploadId, arg.ObjectKey);
            }
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex,
                "Failed to abort multipart upload {UploadId} for {ObjectKey}.",
                arg.UploadId, arg.ObjectKey);
        }
    }

    private async Task AbortMultipartUploadCoreAsync(AbortMultipartUploadArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);
        await s3Client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest
        {
            BucketName = arg.BucketName,
            Key = arg.ObjectKey,
            UploadId = arg.UploadId
        });
    }

    public async Task<bool> DoesObjectExistAsync(GetObjectArg arg)
    {
        var response = await GetHeadObjectMetadataAsync(arg);
        return response.StatusCode == HttpStatusCode.OK;
    }

    public Task<DeleteNetAppResult> DeleteFileOrFolderAsync(DeleteFileOrFolderArg arg)
        => ExecuteWithCredentialRetryAsync(() => DeleteFileOrFolderCoreAsync(arg), nameof(DeleteFileOrFolderAsync), arg.BucketName);

    private async Task<DeleteNetAppResult> DeleteFileOrFolderCoreAsync(DeleteFileOrFolderArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);
        try
        {
            if (!arg.IsFolder)
            {
                var request = _netAppRequestFactory.DeleteObjectRequest(arg);
                await s3Client.DeleteObjectAsync(request);
                return new DeleteNetAppResult(true, 1, null, null);
            }
            else
            {
                var filesToDelete = (await ListAllObjectKeysForDeletionAsync(arg.BucketName, arg.Path, arg.BearerToken)).ToList();

                // Re-resolve the S3 client after listing. The listing phase calls
                // ListObjectsInBucketAsync internally, which has its own credential
                // retry wrapper. If that wrapper detects a credential error and
                // regenerates keys, the s3Client captured above now holds dead keys.
                s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);

                var totalDeleted = 0;
                var allErrors = new List<DeleteError>();

                foreach (var chunk in filesToDelete.Chunk(1000))
                {
                    var deleteObjectsRequest = new DeleteObjectsRequest
                    {
                        BucketName = arg.BucketName,
                        Objects = chunk.Select(path => new KeyVersion { Key = path }).ToList()
                    };

                    var response = await s3Client.DeleteObjectsAsync(deleteObjectsRequest);

                    totalDeleted += (response.DeletedObjects ?? []).Count;

                    var chunkErrors = response.DeleteErrors ?? [];
                    foreach (var error in chunkErrors)
                    {
                        _logger.LogError(
                            "Failed to delete object {ObjectKey} from bucket {BucketName}. Code: {Code}, Message: {Message}",
                            error.Key, arg.BucketName, error.Code, error.Message);
                    }
                    allErrors.AddRange(chunkErrors);
                }

                if (allErrors.Count > 0)
                {
                    return new DeleteNetAppResult(false, totalDeleted,
                        $"Deletion failed for {allErrors.Count} object(s) in folder {arg.Path}.", null);
                }

                return new DeleteNetAppResult(true, totalDeleted, null, null);
            }
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file or folder {Path} from bucket {BucketName}.", arg.Path,
                arg.BucketName);
            throw;
        }
    }

    public async Task<HeadObjectResponseDto> GetHeadObjectMetadataAsync(GetObjectArg arg)
    {
        try
        {
            var headObjectArg =
                _netAppS3HttpArgFactory.CreateGetHeadObjectArg(arg.BearerToken, arg.BucketName, arg.ObjectKey);
            return await _netAppS3HttpClient.GetHeadObjectAsync(headObjectArg);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "HTTP request failed while getting head object metadata for {ObjectKey} in bucket {BucketName}.",
                arg.ObjectKey, arg.BucketName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get head object metadata for {ObjectKey} in bucket {BucketName}.",
                arg.ObjectKey,
                arg.BucketName);
            throw;
        }
    }

    public async Task<SearchResultsDto?> SearchObjectsInBucketAsync(SearchArg arg)
    {
        try
        {
            var searchTerm = arg.OperationName.EnsureTrailingSlash();

            if (arg.Mode == SearchModes.Prefix)
            {
                return await SearchPrefixAsync(arg, searchTerm);
            }

            return await SearchSubstringAsync(arg, searchTerm);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while searching objects in bucket {BucketName}.", arg.BucketName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search objects in bucket {BucketName}.", arg.BucketName);
            throw;
        }
    }

    private async Task<SearchResultsDto> SearchPrefixAsync(SearchArg arg, string searchTerm)
    {
        if (!string.IsNullOrEmpty(arg.Query))
        {
            searchTerm += arg.Query;
        }

        var listObjectsArg = new ListObjectsInBucketArg
        {
            BearerToken = arg.BearerToken,
            BucketName = arg.BucketName,
            MaxKeys = arg.MaxResults.ToString(),
            Prefix = searchTerm,
            IncludeDelimiter = false
        };
        var response = await ListObjectsInBucketAsync(listObjectsArg);

        if (response == null)
        {
            return new SearchResultsDto { Data = [], Truncated = false, TotalScanned = 0 };
        }

        var searchResults = MapToSearchResultItems(response);

        // Search again with Delimiter=true to ensure folders (CommonPrefixes)
        // are included alongside the file matches returned by the no-delimiter call.
        var listObjectsWithDelimiterArg = new ListObjectsInBucketArg
        {
            BearerToken = arg.BearerToken,
            BucketName = arg.BucketName,
            MaxKeys = arg.MaxResults.ToString(),
            Prefix = searchTerm,
            IncludeDelimiter = true
        };

        var responseWithDelimiter = await ListObjectsInBucketAsync(listObjectsWithDelimiterArg);

        if (responseWithDelimiter != null)
        {
            var existingKeys = searchResults.Select(x => x.Key).ToHashSet(StringComparer.Ordinal);
            var searchResultsWithDelimiter = MapToSearchResultItems(responseWithDelimiter);
            searchResults.AddRange(searchResultsWithDelimiter.Where(x => !existingKeys.Contains(x.Key)));
        }

        var merged = searchResults;

        if (merged.Count > arg.MaxResults)
        {
            var folders = merged
                .Where(x => x.Type == S3SearchResultTypes.Folder)
                .ToList();

            var files = merged
                .Where(x => x.Type == S3SearchResultTypes.File)
                .ToList();

            var capped = new List<SearchResultItemDto>(arg.MaxResults);

            capped.AddRange(folders.Take(arg.MaxResults));

            if (capped.Count < arg.MaxResults)
            {
                capped.AddRange(files.Take(arg.MaxResults - capped.Count));
            }

            searchResults = capped;
        }

        var mergeExceedsMax = merged.Count > arg.MaxResults;

        return new SearchResultsDto
        {
            Data = searchResults,
            Truncated = mergeExceedsMax
                        || response.Pagination.NextContinuationToken != null
                        || responseWithDelimiter?.Pagination.NextContinuationToken != null,
            TotalScanned = 0,
        };
    }

    private async Task<SearchResultsDto> SearchSubstringAsync(SearchArg arg, string searchTerm)
    {
        var matchingResults = new List<SearchResultItemDto>();
        var keysInResults = new HashSet<string>(StringComparer.Ordinal);
        var totalScanned = 0;
        var truncated = false;
        string? continuationToken = null;

        do
        {
            var remaining = _options.SearchMaxSubstringScanItems - totalScanned;
            var pageSize = Math.Min(arg.MaxResults, remaining);

            var listObjectsArg = _netAppArgFactory.CreateListObjectsInBucketArg(
                arg.BearerToken, arg.BucketName, continuationToken, pageSize, arg.OperationName, includeDelimiter: false);
            var response = await ListObjectsInBucketAsync(listObjectsArg);

            if (response == null)
            {
                break;
            }

            totalScanned += response.Pagination.KeyCount;
            continuationToken = response.Pagination.NextContinuationToken;

            var pageItems = string.IsNullOrEmpty(arg.Query)
                ? MapToSearchResultItems(response)
                : CollectSubstringSegmentMatches(response, arg.OperationName, arg.Query);

            foreach (var item in pageItems)
            {
                if (!keysInResults.Add(item.Key))
                    continue;

                if (matchingResults.Count >= arg.MaxResults)
                {
                    truncated = true;
                    break;
                }

                matchingResults.Add(item);
            }

            if (truncated)
                break;

            if ((matchingResults.Count >= arg.MaxResults || totalScanned >= _options.SearchMaxSubstringScanItems)
                && continuationToken != null)
            {
                truncated = true;
                break;
            }
        }
        while (continuationToken != null);

        return new SearchResultsDto
        {
            Data = matchingResults,
            Truncated = truncated,
            TotalScanned = totalScanned,
        };
    }

    private static List<SearchResultItemDto> CollectSubstringSegmentMatches(
        ListNetAppObjectsDto response,
        string operationName,
        string query)
    {
        var rootPrefix = NormalizeOperationNamePrefix(operationName);
        var comparer = StringComparison.OrdinalIgnoreCase;
        var dedupe = new HashSet<string>(StringComparer.Ordinal);
        var ordered = new List<SearchResultItemDto>();

        foreach (var item in MapToSearchResultItems(response))
        {
            if (item.Type == S3SearchResultTypes.Folder)
            {
                if (!FolderKeySegmentMatches(item.Key, query, comparer) || !dedupe.Add(item.Key))
                    continue;

                ordered.Add(item);
                continue;
            }

            if (FileBasenameMatches(item.Key, query, comparer) && dedupe.Add(item.Key))
                ordered.Add(item);

            foreach (var folder in EnumerateDerivedParentFolders(item.Key, rootPrefix, query, comparer))
            {
                if (dedupe.Add(folder.Key))
                    ordered.Add(folder);
            }
        }

        return ordered
            .OrderBy(x => x.Type == S3SearchResultTypes.File ? 1 : 0)
            .ThenBy(x => x.Key, StringComparer.Ordinal)
            .ToList();
    }

    private static string NormalizeOperationNamePrefix(string operationName)
    {
        if (string.IsNullOrEmpty(operationName))
            return string.Empty;

        return operationName.EndsWith(S3Constants.Delimiter, StringComparison.Ordinal)
            ? operationName
            : operationName + S3Constants.Delimiter;
    }

    private static bool FolderKeySegmentMatches(string folderKey, string query, StringComparison comparer)
    {
        var name = GetFolderSegmentName(folderKey);
        return !string.IsNullOrEmpty(name) && name.Contains(query, comparer);
    }

    private static string GetFolderSegmentName(string folderKey)
    {
        var trimmed = folderKey.TrimEnd('/');
        if (string.IsNullOrEmpty(trimmed))
            return string.Empty;

        var lastSlash = trimmed.LastIndexOf(S3Constants.Delimiter, StringComparison.Ordinal);
        return lastSlash < 0 ? trimmed : trimmed[(lastSlash + 1)..];
    }

    private static bool FileBasenameMatches(string fileKey, string query, StringComparison comparer)
    {
        if (string.IsNullOrEmpty(fileKey) || fileKey.EndsWith(S3Constants.Delimiter, StringComparison.Ordinal))
            return false;

        var lastSlash = fileKey.LastIndexOf(S3Constants.Delimiter, StringComparison.Ordinal);
        var basename = lastSlash < 0 ? fileKey : fileKey[(lastSlash + 1)..];
        return basename.Contains(query, comparer);
    }

    private static IEnumerable<SearchResultItemDto> EnumerateDerivedParentFolders(
        string fileKey,
        string rootPrefix,
        string query,
        StringComparison comparer)
    {
        if (string.IsNullOrEmpty(fileKey) || fileKey.EndsWith(S3Constants.Delimiter, StringComparison.Ordinal))
            yield break;

        if (string.IsNullOrEmpty(rootPrefix) || !fileKey.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            yield break;

        var relative = fileKey[rootPrefix.Length..];
        var parts = relative.Split(S3Constants.Delimiter, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            yield break;

        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (!parts[i].Contains(query, comparer))
                continue;

            var folderRel = string.Join(S3Constants.Delimiter, parts.Take(i + 1));
            var folderKey = $"{rootPrefix}{folderRel}{S3Constants.Delimiter}";

            yield return new SearchResultItemDto
            {
                Key = folderKey,
                Type = S3SearchResultTypes.Folder,
                LastModified = null,
                Size = null
            };
        }
    }

    private static List<SearchResultItemDto> MapToSearchResultItems(ListNetAppObjectsDto response)
    {
        var results = response.Data.FileData.Select(x =>
        {
            var isFolder = x.Path.EndsWith('/');
            return new SearchResultItemDto
            {
                Key = x.Path,
                Type = isFolder ? S3SearchResultTypes.Folder : S3SearchResultTypes.File,
                LastModified = isFolder ? null : x.LastModified,
                Size = isFolder ? null : x.Filesize
            };
        }).ToList();

        results.AddRange(response.Data.FolderData
            .Where(x => !string.IsNullOrEmpty(x.Path))
            .Select(x => new SearchResultItemDto
            {
                Key = x.Path!,
                Type = S3SearchResultTypes.Folder,
                LastModified = null,
                Size = null
            }));

        return results;
    }

    private async Task<IEnumerable<string>> ListAllObjectKeysForDeletionAsync(string bucketName, string prefix,
        string bearerToken)
    {
        var objectKeys = new List<string>();

        string? continuationToken = null;
        do
        {
            var listArg = new ListObjectsInBucketArg
            {
                BearerToken = bearerToken,
                BucketName = bucketName,
                Prefix = prefix.EndsWith('/') ? prefix : prefix + "/",
                ContinuationToken = continuationToken,
                IncludeDelimiter = true
            };

            var listResponse = await ListObjectsInBucketAsync(listArg);
            if (listResponse?.Data.FileData != null)
            {
                foreach (var file in listResponse.Data.FileData)
                {
                    objectKeys.Add(file.Path);
                }
            }

            if (listResponse?.Data.FolderData != null)
            {
                foreach (var folder in listResponse.Data.FolderData)
                {
                    if (!string.IsNullOrEmpty(folder.Path))
                    {
                        var subObjectKeys =
                            await ListAllObjectKeysForDeletionAsync(bucketName, folder.Path, bearerToken);
                        objectKeys.AddRange(subObjectKeys);
                        objectKeys.Add(folder.Path);
                    }
                }
            }

            continuationToken = listResponse?.Pagination.NextContinuationToken;
        } while (!string.IsNullOrEmpty(continuationToken));

        objectKeys.Add(prefix);

        return objectKeys;
    }

    private Polly.Retry.AsyncRetryPolicy<DeleteObjectResponse> GetDeleteFileRetryPolicy(string objectKey,
        string bucketName)
    {
        return Policy
            .HandleResult<DeleteObjectResponse>(r => r.HttpStatusCode != HttpStatusCode.NoContent)
            .WaitAndRetryAsync(
                Backoff.DecorrelatedJitterBackoffV2(
                    medianFirstRetryDelay: TimeSpan.FromSeconds(1),
                    retryCount: 3),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Delete object retry attempt {RetryCount} for key {ObjectKey} in bucket {BucketName}. Status: {StatusCode}. Waiting {DelayMs}ms before next retry.",
                        retryCount,
                        objectKey,
                        bucketName,
                        outcome.Result?.HttpStatusCode,
                        timespan.TotalMilliseconds);
                });
    }

    private Polly.Retry.AsyncRetryPolicy<UploadPartResponse?> GetUploadPartRetryPolicy(int partNumber, string objectKey)
    {
        // Retry when NetApp rejects the access key because credentials were rotated
        // by a concurrently running part upload or another environment sharing the same Key Vault.
        // On retry, InvalidateClientAsync + GetS3ClientAsync will force-regenerate fresh credentials.
        return Policy
            .HandleResult<UploadPartResponse?>(r => false)
            .Or<AmazonS3Exception>(IsCredentialError)
            .WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(retryAttempt * 3),
                onRetryAsync: async (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(outcome.Exception,
                        "Credential error uploading part {PartNumber} for {ObjectKey} - credentials likely rotated mid-transfer (StatusCode={StatusCode}, ErrorCode={ErrorCode}). Refreshing and retrying (attempt {RetryCount}/2). Waiting {DelayMs}ms.",
                        partNumber, objectKey,
                        (outcome.Exception as AmazonS3Exception)?.StatusCode,
                        (outcome.Exception as AmazonS3Exception)?.ErrorCode,
                        retryCount, timespan.TotalMilliseconds);

                    // Invalidate the cached client ONLY on retry (not the first attempt)
                    // so that GetS3ClientAsync regenerates credentials on the next call.
                    await _s3ClientFactory.InvalidateClientAsync();
                });
    }

    private Polly.Retry.AsyncRetryPolicy GetCompleteMultipartUploadRetryPolicy(string uploadId, string objectKey)
    {
        return Policy
            .Handle<AmazonS3Exception>(ex => (int)ex.StatusCode >= 500
                                             || ex.StatusCode == HttpStatusCode.RequestTimeout
                                             || IsCredentialError(ex))
            .WaitAndRetryAsync(
                Backoff.DecorrelatedJitterBackoffV2(
                    medianFirstRetryDelay: TimeSpan.FromSeconds(3),
                    retryCount: 5),
                onRetryAsync: async (exception, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(exception,
                        "CompleteMultipartUpload retry attempt {RetryCount} for upload {UploadId} ({ObjectKey}). Waiting {DelayMs}ms.",
                        retryCount, uploadId, objectKey, timespan.TotalMilliseconds);

                    // Force credential regeneration on retry so the next attempt
                    // gets fresh keys from NetApp instead of reusing the dead cache.
                    if (exception is AmazonS3Exception s3Ex && IsCredentialError(s3Ex))
                    {
                        await _s3ClientFactory.InvalidateClientAsync();
                    }
                });
    }

    private static bool IsCredentialError(AmazonS3Exception ex)
        => ex.ErrorCode is S3ErrorCodes.InvalidAccessKeyId or S3ErrorCodes.ExpiredToken
               or S3ErrorCodes.InvalidClientTokenId or S3ErrorCodes.AccessDenied
           || ex.Message.Contains("does not exist in our records", StringComparison.OrdinalIgnoreCase)
           || ex.Message.Contains("token has expired", StringComparison.OrdinalIgnoreCase);

    private async Task<T> ExecuteWithCredentialRetryAsync<T>(
        Func<Task<T>> operation, string operationName, string bucketName)
    {
        try
        {
            return await operation();
        }
        catch (AmazonS3Exception ex) when (IsCredentialError(ex))
        {
            _logger.LogWarning(ex,
                "Credential error in {Operation} (ErrorCode={ErrorCode}) - invalidating S3 client and retrying once with fresh credentials",
                operationName, ex.ErrorCode);
            await _s3ClientFactory.InvalidateClientAsync();
            try
            {
                return await operation();
            }
            catch (AmazonS3Exception retryEx) when (retryEx.ErrorCode == S3ErrorCodes.AccessDenied)
            {
                throw new NetAppAccessDeniedException(bucketName, retryEx);
            }
        }
        catch (S3CredentialException ex)
        {
            _logger.LogWarning(ex,
                "HTTP credential error in {Operation} - invalidating S3 client and retrying once with fresh credentials",
                operationName);
            await _s3ClientFactory.InvalidateClientAsync();
            try
            {
                return await operation();
            }
            catch (S3CredentialException retryEx)
            {
                throw new NetAppAccessDeniedException(bucketName, retryEx);
            }
        }
    }
}