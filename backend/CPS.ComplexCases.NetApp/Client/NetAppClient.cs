using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Namespace.CPS.ComplexCases.NetApp.Constants;

namespace CPS.ComplexCases.NetApp.Client;

public class NetAppClient : INetAppClient
{
    private readonly ILogger<NetAppClient> _logger;
    private readonly NetAppOptions _options;
    private IAmazonS3 _client;
    private readonly IAmazonS3UtilsWrapper _amazonS3UtilsWrapper;
    private readonly INetAppArgFactory _netAppArgFactory;

    public NetAppClient(ILogger<NetAppClient> logger, IOptions<NetAppOptions> options, IAmazonS3 client, IAmazonS3UtilsWrapper amazonS3UtilsWrapper, INetAppArgFactory netAppArgFactory)
    {
        _logger = logger;
        _options = options.Value;
        _client = client;
        _amazonS3UtilsWrapper = amazonS3UtilsWrapper;
        _netAppArgFactory = netAppArgFactory;
    }

    public async Task<bool> CreateBucketAsync(CreateBucketArg arg)
    {
        try
        {
            var bucketExists = await _amazonS3UtilsWrapper.DoesS3BucketExistV2Async(_client, arg.BucketName);
            if (bucketExists)
            {
                _logger.LogInformation($"Bucket with name {arg.BucketName} already exists.");
                return false;
            }

            var request = new PutBucketRequest
            {
                BucketName = arg.BucketName,
                UseClientRegion = true
            };

            var response = await _client.PutBucketAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex.Message, $"Failed to create bucket with name {arg.BucketName}");
            return false;
        }
    }

    public async Task<IEnumerable<S3Bucket>> ListBucketsAsync(ListBucketsArg arg)
    {
        try
        {
            var response = await _client.ListBucketsAsync(new ListBucketsRequest
            {
                ContinuationToken = arg.ContinuationToken,
                MaxBuckets = arg.MaxBuckets ?? 10000,
                Prefix = arg.Prefix
            });
            return response.Buckets;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex.Message, $"Failed to list buckets.");
            return [];
        }
    }

    public async Task<S3Bucket?> FindBucketAsync(FindBucketArg arg)
    {
        try
        {
            var buckets = await ListBucketsAsync(_netAppArgFactory.CreateListBucketsArg());

            return buckets?.SingleOrDefault(x => x.BucketName == arg.BucketName);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex.Message, $"Failed to find bucket {arg.BucketName}.");
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
            _logger.LogError(ex.Message, $"Failed to get ACL for bucket {bucketName}");
            return null;
        }
    }

    public async Task<GetObjectResponse?> GetObjectAsync(GetObjectArg arg)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = arg.BucketName,
                Key = arg.ObjectKey,
            };

            var response = await _client.GetObjectAsync(request);

            var stream = response.ResponseStream;

            return new GetObjectResponse
            {
                BucketName = arg.BucketName,
                Key = arg.ObjectKey,
            };
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex.Message, $"Failed to get file {arg.ObjectKey} from bucket {arg.BucketName}.");
            return null;
        }
    }

    public async Task<bool> UploadObjectAsync(UploadObjectArg arg)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = arg.BucketName,
                Key = arg.ObjectKey,
                InputStream = arg.Stream,
            };

            var response = await _client.PutObjectAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex.Message, $"Failed to upload file {arg.ObjectKey} to bucket {arg.BucketName}.");
            return false;
        }
    }

    public async Task<ListObjectsV2Response?> ListObjectsInBucketAsync(ListObjectsInBucketArg arg)
    {
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = arg.BucketName,
                ContinuationToken = arg.ContinuationToken
            };

            return await _client.ListObjectsV2Async(request);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex.Message, $"Failed to list objects in bucket {arg.BucketName}.");
            return null;
        }
    }

    public async Task<IEnumerable<string>> ListFoldersInBucketAsync(ListFoldersInBucketArg arg)
    {
        string? prefix = null;
        if (!string.IsNullOrEmpty(arg.Prefix))
        {
            prefix = !arg.Prefix.EndsWith(S3Constants.Delimiter) ? $"{arg.Prefix}{S3Constants.Delimiter}" : arg.Prefix;
        }

        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = arg.BucketName,
                ContinuationToken = arg.ContinuationToken,
                Delimiter = S3Constants.Delimiter,
                Prefix = prefix
            };

            var response = await _client.ListObjectsV2Async(request);
            return response.CommonPrefixes;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex.Message, $"Failed to list objects in bucket {arg.BucketName}.");
            return [];
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