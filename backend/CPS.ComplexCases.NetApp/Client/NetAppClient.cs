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

public class NetAppClient(ILogger<NetAppClient> logger, IAmazonS3 client, IAmazonS3UtilsWrapper amazonS3UtilsWrapper, INetAppRequestFactory netAppRequestFactory) : INetAppClient
{
    private readonly ILogger<NetAppClient> _logger = logger;
    private readonly IAmazonS3 _client = client;
    private readonly IAmazonS3UtilsWrapper _amazonS3UtilsWrapper = amazonS3UtilsWrapper;
    private readonly INetAppRequestFactory _netAppRequestFactory = netAppRequestFactory;

    public async Task<bool> CreateBucketAsync(CreateBucketArg arg)
    {
        try
        {
            var bucketExists = await _amazonS3UtilsWrapper.DoesS3BucketExistV2Async(_client, arg.BucketName);
            if (bucketExists)
            {
                _logger.LogInformation("Bucket with name {BucketName} already exists.", arg.BucketName);
                return false;
            }

            var response = await _client.PutBucketAsync(_netAppRequestFactory.CreateBucketRequest(arg));
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
        try
        {
            var response = await _client.ListBucketsAsync(_netAppRequestFactory.ListBucketsRequest(arg));
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

    public async Task<S3AccessControlList?> GetACLForBucketAsync(string bucketName)
    {
        try
        {
            var response = await _client.GetACLAsync(new GetACLRequest
            {
                BucketName = bucketName
            });

            return response.AccessControlList;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, ex.Message, "Failed to get ACL for bucket {BucketName}", bucketName);
            return null;
        }
    }

    public async Task<GetObjectResponse?> GetObjectAsync(GetObjectArg arg)
    {
        try
        {
            var response = await _client.GetObjectAsync(_netAppRequestFactory.GetObjectRequest(arg));

            return response;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, ex.Message, "Failed to get file {ObjectKey} from bucket {BucketName}.", arg.ObjectKey, arg.BucketName);
            throw;
        }
    }

    public async Task<bool> UploadObjectAsync(UploadObjectArg arg)
    {
        try
        {
            var response = await _client.PutObjectAsync(_netAppRequestFactory.UploadObjectRequest(arg));
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
        try
        {
            var request = _netAppRequestFactory.ListObjectsInBucketRequest(arg);
            var response = await _client.ListObjectsV2Async(request);

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
        try
        {
            var response = await _client.ListObjectsV2Async(_netAppRequestFactory.ListFoldersInBucketRequest(arg));
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
        try
        {
            return await _client.InitiateMultipartUploadAsync(_netAppRequestFactory.CreateMultipartUploadRequest(arg));
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate multipart upload for file {ObjectKey}.", arg.ObjectKey);
            return null;
        }
    }

    public async Task<UploadPartResponse?> UploadPartAsync(UploadPartArg arg)
    {
        try
        {
            return await _client.UploadPartAsync(_netAppRequestFactory.UploadPartRequest(arg));
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to upload part {PartNumber} for file {ObjectKey}.", arg.PartNumber, arg.ObjectKey);
            throw;
        }
    }

    public async Task<CompleteMultipartUploadResponse?> CompleteMultipartUploadAsync(CompleteMultipartUploadArg arg)
    {
        try
        {
            return await _client.CompleteMultipartUploadAsync(_netAppRequestFactory.CompleteMultipartUploadRequest(arg));
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to complete multipart upload {UploadId} for file {ObjectKey}.", arg.UploadId, arg.ObjectKey);
            throw;
        }
    }

    public async Task<bool> DoesObjectExistAsync(GetObjectArg arg)
    {
        try
        {
            var response = await _client.GetObjectAttributesAsync(_netAppRequestFactory.GetObjectAttributesRequest(arg));
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
        using (var stsClient = new AmazonSecurityTokenServiceClient(accessKey, secretKey))
        {
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
}