using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Amazon.S3.Model;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using CPS.ComplexCases.NetApp.Models.S3.Result;

namespace CPS.ComplexCases.NetApp.Client;

public class NetAppMockHttpClient(ILogger<NetAppMockHttpClient> logger, HttpClient httpClient, INetAppMockHttpRequestFactory netAppMockHttpRequestFactory) : INetAppClient
{
    private readonly ILogger<NetAppMockHttpClient> _logger = logger;
    private readonly HttpClient _httpClient = httpClient;
    private readonly INetAppMockHttpRequestFactory _netAppMockHttpRequestFactory = netAppMockHttpRequestFactory;

    public async Task<bool> CreateBucketAsync(CreateBucketArg arg)
    {
        try
        {
            var findExistingBucketArg = new FindBucketArg
            {
                BearerToken = arg.BearerToken,
                BucketName = arg.BucketName
            };
            var existingBucket = await FindBucketAsync(findExistingBucketArg);

            if (existingBucket != null)
            {
                _logger.LogError("A bucket with the name {BucketName} already exists.", arg.BucketName);
                return false;
            }

            var response = await SendRequestAsync(_netAppMockHttpRequestFactory.CreateBucketRequest(arg));
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Bucket '{BucketName}' created successfully.", arg.BucketName);
                return true;
            }
            else
            {
                _logger.LogError("Failed to create bucket. Status Code: {StatusCode}", response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bucket {BucketName} in NetApp.", arg.BucketName);
            throw;
        }
    }

    public async Task<S3Bucket?> FindBucketAsync(FindBucketArg arg)
    {
        var buckets = await ListBucketsAsync(new ListBucketsArg
        {
            BearerToken = arg.BearerToken,
            BucketName = arg.BucketName
        });

        var response = buckets.FirstOrDefault(x => x.BucketName.Equals(arg.BucketName, StringComparison.OrdinalIgnoreCase));
        return response;
    }

    public async Task<S3AccessControlList?> GetACLForBucketAsync(string bucketName)
    {
        var response = await SendRequestAsync<S3AccessControlList>(_netAppMockHttpRequestFactory.GetACLForBucketRequest(bucketName));
        return response;
    }

    public async Task<GetObjectResponse?> GetObjectAsync(GetObjectArg arg)
    {
        var response = await SendRequestAsync(_netAppMockHttpRequestFactory.GetObjectRequest(arg));

        return new GetObjectResponse
        {
            ResponseStream = await response.Content.ReadAsStreamAsync()
        };
    }

    public async Task<IEnumerable<S3Bucket>> ListBucketsAsync(ListBucketsArg arg)
    {
        var list = new List<S3Bucket>();
        var response = await SendRequestAsync<ListAllMyBucketsResult>(_netAppMockHttpRequestFactory.ListBucketsRequest(arg));
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

    public async Task<ListNetAppObjectsDto?> ListFoldersInBucketAsync(ListFoldersInBucketArg arg)
    {
        var response = await SendRequestAsync<ListBucketResult>(_netAppMockHttpRequestFactory.ListFoldersInBucketRequest(arg));

        List<ListNetAppFolderDataDto> folders;

        if (!string.IsNullOrEmpty(arg.OperationName) && string.IsNullOrEmpty(arg.Prefix))
        {
            folders = response.CommonPrefixes?
                .Where(x => x.Prefix.Contains(arg.OperationName) && x.Prefix.Count(p => p == '/') == 1)
                .Select(data => new ListNetAppFolderDataDto
                {
                    Path = data.Prefix
                }).ToList() ?? [];
        }
        else
        {
            folders = response.CommonPrefixes?.Select(data => new ListNetAppFolderDataDto
            {
                Path = data.Prefix
            }).ToList() ?? [];
        }

        var result = new ListNetAppObjectsDto
        {
            Data = new ListNetAppDataDto
            {
                BucketName = arg.BucketName,
                RootPath = arg.Prefix,
                FolderData = folders,
                FileData = [],
            },
            Pagination = new PaginationDto
            {
                ContinuationToken = response.ContinuationToken,
                NextContinuationToken = response.NextContinuationToken,
                MaxKeys = response.MaxKeys,
                KeyCount = response.KeyCount,
            }
        };

        return result;
    }

    public async Task<ListNetAppObjectsDto?> ListObjectsInBucketAsync(ListObjectsInBucketArg arg)
    {
        var response = await SendRequestAsync<ListBucketResult>(_netAppMockHttpRequestFactory.ListObjectsInBucketRequest(arg));

        var folders = response.CommonPrefixes?.Select(data => new ListNetAppFolderDataDto
        {
            Path = data.Prefix
        }).ToList() ?? [];

        var files = response.Contents?.Select(data => new ListNetAppFileDataDto
        {
            Path = data.Key,
            Etag = data.ETag,
            Filesize = data.Size,
            LastModified = data.LastModified
        }).ToList() ?? [];

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
                KeyCount = response.KeyCount,
            }
        };

        return result;
    }

    public Task<bool> UploadObjectAsync(UploadObjectArg arg)
    {
        throw new NotImplementedException();
    }

    public async Task<InitiateMultipartUploadResponse?> InitiateMultipartUploadAsync(InitiateMultipartUploadArg arg)
    {
        var response = await SendRequestAsync<InitiateMultipartUploadResult>(_netAppMockHttpRequestFactory.CreateMultipartUploadRequest(arg));

        return new InitiateMultipartUploadResponse
        {
            UploadId = response.UploadId,
            Key = response.Key,
            BucketName = response.Bucket
        };
    }

    public async Task<UploadPartResponse?> UploadPartAsync(UploadPartArg arg)
    {
        var response = await SendRequestAsync(_netAppMockHttpRequestFactory.UploadPartRequest(arg));

        return new UploadPartResponse
        {
            ETag = response.Headers.ETag?.ToString() ?? string.Empty,
            PartNumber = arg.PartNumber
        };
    }

    public async Task<CompleteMultipartUploadResponse?> CompleteMultipartUploadAsync(CompleteMultipartUploadArg arg)
    {
        var response = await SendRequestAsync<CompleteMultipartUploadResult>(_netAppMockHttpRequestFactory.CompleteMultipartUploadRequest(arg));

        return new CompleteMultipartUploadResponse
        {
            ETag = response.ETag,
            BucketName = response.Bucket,
            Key = response.Key,
            Location = response.Location
        };
    }

    public async Task<bool> DoesObjectExistAsync(GetObjectArg arg)
    {
        try
        {
            var response = await SendRequestAsync<GetObjectAttributesOutput>(_netAppMockHttpRequestFactory.GetObjectAttributesRequest(arg));

            if (response != null && !string.IsNullOrEmpty(response.ETag))
            {
                return true;
            }
            return false;
        }
        catch (NetAppNotFoundException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if object exists in NetApp.");
            throw;
        }
    }

    private async Task<T> SendRequestAsync<T>(HttpRequestMessage request)
    {
        using var response = await SendRequestAsync(request);

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            _logger.LogError("Request failed with status code: {StatusCode}", response.StatusCode);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new NetAppUnauthorizedException();
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new NetAppNotFoundException("The requested resource was not found.");
            }

            throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = DeserializeResponse<T>(responseContent) ?? throw new InvalidOperationException("Deserialization returned null.");
        return result;
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request)
    {
        var response = await _httpClient.SendAsync(request);
        try
        {
            response.EnsureSuccessStatusCode();
            return response;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new NetAppNotFoundException("The requested resource was not found.", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error sending request to NetApp service.");
            throw;
        }
    }

    private static T DeserializeResponse<T>(string responseContent)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StringReader(responseContent);
        return (T)serializer.Deserialize(reader)!;
    }
}