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

namespace CPS.ComplexCases.NetApp.Client;

public class NetAppClient(ILogger<NetAppClient> logger, IAmazonS3UtilsWrapper amazonS3UtilsWrapper, INetAppRequestFactory netAppRequestFactory, IS3ClientFactory s3ClientFactory) : INetAppClient
{
    private readonly ILogger<NetAppClient> _logger = logger;
    private readonly IAmazonS3UtilsWrapper _amazonS3UtilsWrapper = amazonS3UtilsWrapper;
    private readonly INetAppRequestFactory _netAppRequestFactory = netAppRequestFactory;
    private readonly IS3ClientFactory _s3ClientFactory = s3ClientFactory;

    public async Task<bool> CreateBucketAsync(CreateBucketArg arg)
    {
        var s3Client = await _s3ClientFactory.CreateS3ClientAsync();

        try
        {
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
        var s3Client = await _s3ClientFactory.CreateS3ClientAsync();

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
            var buckets = await ListBucketsAsync(new ListBucketsArg());

            return buckets?.SingleOrDefault(x => x.BucketName == arg.BucketName);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, ex.Message, "Failed to find bucket {BucketName}.", arg.BucketName);
            return null;
        }
    }

    public async Task<GetObjectResponse?> GetObjectAsync(GetObjectArg arg)
    {
        var s3Client = await _s3ClientFactory.CreateS3ClientAsync();

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
        var s3Client = await _s3ClientFactory.CreateS3ClientAsync();

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
        var s3Client = await _s3ClientFactory.CreateS3ClientAsync();

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
        var s3Client = await _s3ClientFactory.CreateS3ClientAsync();
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
        var s3Client = await _s3ClientFactory.CreateS3ClientAsync();
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
        var s3Client = await _s3ClientFactory.CreateS3ClientAsync();
        try
        {
            return await s3Client.UploadPartAsync(_netAppRequestFactory.UploadPartRequest(arg));
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to upload part {PartNumber} for file {ObjectKey}.", arg.PartNumber, arg.ObjectKey);
            throw;
        }
    }

    public async Task<CompleteMultipartUploadResponse?> CompleteMultipartUploadAsync(CompleteMultipartUploadArg arg)
    {
        var s3Client = await _s3ClientFactory.CreateS3ClientAsync();
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
        var s3Client = await _s3ClientFactory.CreateS3ClientAsync();
        try
        {
            var response = await s3Client.GetObjectAttributesAsync(_netAppRequestFactory.GetObjectAttributesRequest(arg));
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
}