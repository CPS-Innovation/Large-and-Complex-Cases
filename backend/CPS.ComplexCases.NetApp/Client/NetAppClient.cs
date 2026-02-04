using Microsoft.Extensions.Logging;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using CPS.ComplexCases.NetApp.Wrappers;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace CPS.ComplexCases.NetApp.Client;

public class NetAppClient(
    ILogger<NetAppClient> logger,
    IAmazonS3UtilsWrapper amazonS3UtilsWrapper,
    INetAppRequestFactory netAppRequestFactory,
    IS3ClientFactory s3ClientFactory) : INetAppClient
{
    private readonly ILogger<NetAppClient> _logger = logger;
    private readonly IAmazonS3UtilsWrapper _amazonS3UtilsWrapper = amazonS3UtilsWrapper;
    private readonly INetAppRequestFactory _netAppRequestFactory = netAppRequestFactory;
    private readonly IS3ClientFactory _s3ClientFactory = s3ClientFactory;

    public async Task<bool> CreateBucketAsync(CreateBucketArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);

        try
        {
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
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, ex.Message, "Failed to create bucket with name {BucketName}", arg.BucketName);
            return false;
        }
    }

    public async Task<IEnumerable<S3Bucket>> ListBucketsAsync(ListBucketsArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);

        try
        {
            var response = await s3Client.ListBucketsAsync(_netAppRequestFactory.ListBucketsRequest(arg));
            return response.Buckets;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, ex.Message, "Failed to list buckets.");
            return [];
        }
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

    public async Task<bool> CreateFolderAsync(CreateFolderArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);

        try
        {
            // S3 does not have a native folder concept. Folders are represented by creating a zero-byte object with a key that ends with a "/".
            // However, doing so results in an rate-limiting issue with NetApp.
            // As a workaround, to create a folder, we create an empty object and then delete it immediately after to simulate folder creation.
            var request = _netAppRequestFactory.CreateFolderRequest(arg);
            var response = await s3Client.PutObjectAsync(request);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogWarning("Failed to create folder {FolderKey} in bucket {BucketName}. HTTP Status Code: {StatusCode}", arg.FolderKey, arg.BucketName, response.HttpStatusCode);
                return false;
            }
            var retryPolicy = GetDeleteFileRetryPolicy(request.Key, arg.BucketName);

            var deleteResponse = await retryPolicy.ExecuteAsync(async () =>
                await s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = arg.BucketName,
                    Key = request.Key
                }));

            return deleteResponse.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, ex.Message, "Failed to create folder {FolderKey} in bucket {BucketName}.", arg.FolderKey, arg.BucketName);
            throw;
        }
    }

    public async Task<GetObjectResponse?> GetObjectAsync(GetObjectArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);

        try
        {
            var response = await s3Client.GetObjectAsync(_netAppRequestFactory.GetObjectRequest(arg));

            return response;
        }
        catch (AmazonS3Exception ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Object {ObjectKey} not found in bucket {BucketName}.", arg.ObjectKey, arg.BucketName);
                throw new FileNotFoundException($"Object {arg.ObjectKey} not found in bucket {arg.BucketName}.", ex);
            }

            _logger.LogError(ex, ex.Message, "Failed to get file {ObjectKey} from bucket {BucketName}.", arg.ObjectKey, arg.BucketName);
            throw;
        }
    }

    public async Task<bool> UploadObjectAsync(UploadObjectArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);

        try
        {
            var response = await s3Client.PutObjectAsync(_netAppRequestFactory.UploadObjectRequest(arg));
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, ex.Message, "Failed to upload file {ObjectKey} to bucket {BucketName}.", arg.ObjectKey, arg.BucketName);
            throw;
        }
    }

    public async Task<ListNetAppObjectsDto?> ListObjectsInBucketAsync(ListObjectsInBucketArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);

        try
        {
            var request = _netAppRequestFactory.ListObjectsInBucketRequest(arg);
            var response = await s3Client.ListObjectsV2Async(request);

            var folders = response.CommonPrefixes.Select(data => new ListNetAppFolderDataDto
            {
                Path = data
            });

            var files = response.S3Objects.Select(data => new ListNetAppFileDataDto
            {
                Path = data.Key,
                Etag = data.ETag,
                Filesize = data.Size,
                LastModified = data.LastModified
            });

            var result = new ListNetAppObjectsDto
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
                    KeyCount = response.KeyCount
                }
            };

            return result;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to list objects in bucket {BucketName}.", arg.BucketName);
            return null;
        }
    }

    public async Task<ListNetAppObjectsDto?> ListFoldersInBucketAsync(ListFoldersInBucketArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);
        try
        {
            var response = await s3Client.ListObjectsV2Async(_netAppRequestFactory.ListFoldersInBucketRequest(arg));
            var folders = response.CommonPrefixes.Select(data => new ListNetAppFolderDataDto
            {
                Path = data
            });

            var result = new ListNetAppObjectsDto
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
                    KeyCount = response.KeyCount
                }
            };

            return result;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to list objects in bucket {BucketName}.", arg.BucketName);
            return null;
        }
    }

    public async Task<InitiateMultipartUploadResponse?> InitiateMultipartUploadAsync(InitiateMultipartUploadArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);
        try
        {
            return await s3Client.InitiateMultipartUploadAsync(_netAppRequestFactory.CreateMultipartUploadRequest(arg));
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate multipart upload for file {ObjectKey}.", arg.ObjectKey);
            return null;
        }
    }

    public async Task<UploadPartResponse?> UploadPartAsync(UploadPartArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);
        try
        {
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
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to upload part {PartNumber} for file {ObjectKey}.", arg.PartNumber, arg.ObjectKey);
            throw;
        }
    }

    public async Task<CompleteMultipartUploadResponse?> CompleteMultipartUploadAsync(CompleteMultipartUploadArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);
        try
        {
            return await s3Client.CompleteMultipartUploadAsync(_netAppRequestFactory.CompleteMultipartUploadRequest(arg));
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to complete multipart upload {UploadId} for file {ObjectKey}.", arg.UploadId, arg.ObjectKey);
            throw;
        }
    }

    public async Task<bool> DoesObjectExistAsync(GetObjectArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);
        try
        {
            var response = await s3Client.GetObjectAsync(_netAppRequestFactory.GetObjectRequest(arg));
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to check if object {ObjectKey} exists in bucket {BucketName}.", arg.ObjectKey, arg.BucketName);
            return false;
        }
    }

    public async Task<string> DeleteFileOrFolderAsync(DeleteFileOrFolderArg arg)
    {
        var s3Client = await _s3ClientFactory.GetS3ClientAsync(arg.BearerToken);
        try
        {
            if (Path.HasExtension(arg.Path))
            {
                var request = _netAppRequestFactory.DeleteObjectRequest(arg);
                var response = await s3Client.DeleteObjectAsync(request);
                return $"Successfully deleted file {arg.Path} from bucket {arg.BucketName}.";
            }
            else
            {
                var filesToDelete = await ListAllObjectKeysForDeletionAsync(arg.BucketName, arg.Path, arg.BearerToken);

                var deleteObjectsRequest = new DeleteObjectsRequest
                {
                    BucketName = arg.BucketName,
                    Objects = filesToDelete.Select(path => new KeyVersion { Key = path }).ToList()
                };

                var response = await s3Client.DeleteObjectsAsync(deleteObjectsRequest);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK && response.DeleteErrors.Count == 0)
                {
                    return $"Successfully deleted folder {arg.Path} and its contents from bucket {arg.BucketName}.";
                }

                foreach (var error in response.DeleteErrors)
                {
                    _logger.LogError("Failed to delete object {ObjectKey} from bucket {BucketName}. Code: {Code}, Message: {Message}",
                        error.Key, arg.BucketName, error.Code, error.Message);
                }

                var successfulDeletionsCount = response.DeletedObjects.Count;
                var failedDeletionsCount = response.DeleteErrors.Count;

                return $"Successfully deleted {successfulDeletionsCount} files from bucket {arg.BucketName}. Deletion failed for {failedDeletionsCount} files. ";
            }
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file or folder {Path} from bucket {BucketName}.", arg.Path, arg.BucketName);
            throw;
        }
    }

    private async Task<IEnumerable<string>> ListAllObjectKeysForDeletionAsync(string bucketName, string prefix, string bearerToken)
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
                ContinuationToken = continuationToken
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
                        var subObjectKeys = await ListAllObjectKeysForDeletionAsync(bucketName, folder.Path, bearerToken);
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

    private static async Task<SessionAWSCredentials> GetTemporaryCredentialsAsync(string accessKey, string secretKey)
    {
        using var stsClient = new AmazonSecurityTokenServiceClient(accessKey, secretKey);
        var getSessionTokenRequest = new GetSessionTokenRequest
        {
            DurationSeconds = 7200
        };

        GetSessionTokenResponse sessionTokenResponse =
                      await stsClient.GetSessionTokenAsync(getSessionTokenRequest);

        Credentials credentials = sessionTokenResponse.Credentials;

        var sessionCredentials =
            new SessionAWSCredentials(credentials.AccessKeyId,
                                      credentials.SecretAccessKey,
                                      credentials.SessionToken);
        return sessionCredentials;
    }

    private Polly.Retry.AsyncRetryPolicy<DeleteObjectResponse> GetDeleteFileRetryPolicy(string objectKey, string bucketName)
    {
        return Policy
                .HandleResult<DeleteObjectResponse>(r => r.HttpStatusCode != System.Net.HttpStatusCode.NoContent)
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
}