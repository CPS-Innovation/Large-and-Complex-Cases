using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Amazon.S3.Model;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.S3.Result;
using CPS.ComplexCases.NetApp.Models.Dto;
using CPS.ComplexCases.NetApp.Exceptions;

namespace CPS.ComplexCases.NetApp.Client;

public class NetAppMockHttpClient : INetAppClient
{
    private readonly ILogger<NetAppMockHttpClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly INetAppMockRequestFactory _netAppMockRequestFactory;

    public NetAppMockHttpClient(ILogger<NetAppMockHttpClient> logger, HttpClient httpClient, INetAppMockRequestFactory netAppMockRequestFactory)
    {
        _logger = logger;
        _httpClient = httpClient;
        _netAppMockRequestFactory = netAppMockRequestFactory;
    }

    public async Task<bool> CreateBucketAsync(CreateBucketArg arg)
    {
        try
        {
            var findExistingBucketArg = new FindBucketArg { BucketName = arg.BucketName };
            var existingBucket = await FindBucketAsync(findExistingBucketArg);

            if (existingBucket != null)
            {
                _logger.LogError($"A bucket with the name {arg.BucketName} already exists.");
                return false;
            }

            var response = await SendRequestAsync(_netAppMockRequestFactory.CreateBucketRequest(arg));
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Bucket '{arg.BucketName}' created successfully.");
                return true;
            }
            else
            {
                _logger.LogError($"Failed to create bucket. Status Code: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating bucket {arg.BucketName} in NetApp.");
            throw;
        }
    }

    public async Task<S3Bucket?> FindBucketAsync(FindBucketArg arg)
    {
        var buckets = await ListBucketsAsync(new ListBucketsArg());

        var response = buckets.FirstOrDefault(x => x.BucketName.Equals(arg.BucketName, StringComparison.OrdinalIgnoreCase));
        return response;
    }

    public async Task<S3AccessControlList?> GetACLForBucketAsync(string bucketName)
    {
        var response = await SendRequestAsync<S3AccessControlList>(_netAppMockRequestFactory.GetACLForBucketRequest(bucketName));
        return response;
    }

    public async Task<GetObjectResponse?> GetObjectAsync(GetObjectArg arg)
    {
        var response = await SendRequestAsync<GetObjectResponse>(_netAppMockRequestFactory.GetObjectRequest(arg));
        return response;
    }

    public async Task<IEnumerable<S3Bucket>> ListBucketsAsync(ListBucketsArg arg)
    {
        var list = new List<S3Bucket>();
        var response = await SendRequestAsync<ListAllMyBucketsResult>(_netAppMockRequestFactory.ListBucketsRequest(arg));
        if (response.Buckets.Any())
        {
            foreach (var bucket in response.Buckets)
            {
                var s3Bucket = new S3Bucket
                {
                    BucketName = bucket.BucketName,
                    CreationDate = bucket.CreationDate
                };
                list.Add(s3Bucket);
            }
        }
        return list;
    }

    public async Task<ListNetAppFoldersDto?> ListFoldersInBucketAsync(ListFoldersInBucketArg arg)
    {
        var response = await SendRequestAsync<ListBucketResult>(_netAppMockRequestFactory.ListFoldersInBucketRequest(arg));

        var folders = response.CommonPrefixes?.Select(data => new ListNetAppFoldersDataDto
        {
            Path = data.Prefix
        });

        var result = new ListNetAppFoldersDto
        {
            BucketName = arg.BucketName,
            Data = folders,
            DataInfo = new DataInfoDto
            {
                ContinuationToken = response.ContinuationToken,
                NextContinuationToken = response.NextContinuationToken,
                MaxKeys = response.MaxKeys,
                KeyCount = response.KeyCount,
            }
        };

        return result;
    }

    public async Task<ListObjectsV2Response?> ListObjectsInBucketAsync(ListObjectsInBucketArg arg)
    {
        var response = await SendRequestAsync<ListObjectsV2Response>(_netAppMockRequestFactory.ListObjectsInBucketRequest(arg));
        return response;
    }

    public Task<bool> UploadObjectAsync(UploadObjectArg arg)
    {
        throw new NotImplementedException();
    }

    private async Task<T> SendRequestAsync<T>(HttpRequestMessage request)
    {
        using var response = await SendRequestAsync(request);
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            _logger.LogError($"Request failed with status code: {response.StatusCode}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new NetAppUnauthorizedException();
            }

            throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
        }
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = DeserializeResponse<T>(responseContent) ?? throw new InvalidOperationException("Deserialization returned null.");
        return result;
    }

    private static T DeserializeResponse<T>(string responseContent)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StringReader(responseContent);
        return (T)serializer.Deserialize(reader)!;
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request)
    {
        var response = await _httpClient.SendAsync(request);
        try
        {
            response.EnsureSuccessStatusCode();
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error sending request to NetApp service.");
            throw;
        }
    }
}