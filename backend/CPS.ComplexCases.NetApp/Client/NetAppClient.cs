using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using CPS.ComplexCases.NetApp.Models.Args;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.NetApp.Client
{
    public class NetAppClient : INetAppClient
    {
        private readonly IAmazonS3 _client;
        private readonly ILogger<NetAppClient> _logger;

        public NetAppClient(ILogger<NetAppClient> logger, IAmazonS3 amazonS3Client)
        {
            _client = amazonS3Client;
            _logger = logger;
        }

        public async Task<bool> CreateBucketAsync(CreateBucketArg arg)
        {
            try
            {
                var bucketExists = AmazonS3Util.DoesS3BucketExistV2Async(_client, arg.BucketName).Result;
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

        public async Task<S3Bucket?> FindBucketAsync(FindBucketArg arg)
        {
            try
            {
                var response = await _client.ListBucketsAsync();

                return response.Buckets.SingleOrDefault(x => x.BucketName == arg.BucketName);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex.Message, $"Failed to find bucket {arg.BucketName}.");
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
                    Key = arg.ObjectName,
                };

                var response = await _client.GetObjectAsync(request);

                var stream = response.ResponseStream;

                return new GetObjectResponse
                {
                    BucketName = arg.BucketName,
                    Key = arg.ObjectName,
                };
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex.Message, $"Failed to get file {arg.ObjectName} from bucket {arg.BucketName}.");
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
                    Key = arg.ObjectName,
                    InputStream = arg.Stream,
                };

                var response = await _client.PutObjectAsync(request);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex.Message, $"Failed to upload file {arg.ObjectName} to bucket {arg.BucketName}.");
                return false;
            }
        }

        public async Task<ListObjectsV2Response?> ListObjectsInBucketAsync(ListObjectsInBucketArg arg)
        {
            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = arg.BucketName
                };

                return await _client.ListObjectsV2Async(request);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex.Message, $"Failed to list objects in bucket {arg.BucketName}.");
                return null;
            }
        }
    }
}