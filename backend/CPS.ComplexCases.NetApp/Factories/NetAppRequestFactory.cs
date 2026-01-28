using System.Text;
using System.Text.Json;
using System.Web;
using Amazon.S3;
using Amazon.S3.Model;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.NetApp.Constants;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;

namespace CPS.ComplexCases.NetApp.Factories;

public class NetAppRequestFactory : INetAppRequestFactory
{
    public CompleteMultipartUploadRequest CompleteMultipartUploadRequest(CompleteMultipartUploadArg arg)
    {
        return new CompleteMultipartUploadRequest
        {
            BucketName = arg.BucketName,
            Key = arg.ObjectKey,
            UploadId = arg.UploadId,
            PartETags = arg.CompletedParts
        };
    }

    public HttpRequestMessage CreateRegisterUserRequest(RegisterUserArg arg)
    {
        return BuildRequest<object>(HttpMethod.Post, $"api/protocols/s3/services/{arg.S3ServiceUuid}/users/{EncodedValue(arg.Username)}", arg.AccessToken);
    }

    public PutBucketRequest CreateBucketRequest(CreateBucketArg arg)
    {
        return new PutBucketRequest
        {
            BucketName = arg.BucketName,
            UseClientRegion = true
        };
    }

    public PutObjectRequest CreateFolderRequest(CreateFolderArg arg)
    {
        var folderName = arg.FolderKey.EndsWith(S3Constants.Delimiter) ? arg.FolderKey : arg.FolderKey + S3Constants.Delimiter;

        return new PutObjectRequest
        {
            BucketName = arg.BucketName,
            Key = $"{folderName}{S3Constants.TempFileName}",
            InputStream = new MemoryStream([]),
        };
    }

    public InitiateMultipartUploadRequest CreateMultipartUploadRequest(InitiateMultipartUploadArg arg)
    {
        return new InitiateMultipartUploadRequest
        {
            BucketName = arg.BucketName,
            Key = arg.ObjectKey,
        };
    }

    public HttpRequestMessage CreateRegenerateUserKeysRequest(RegenerateUserKeysArg arg)
    {
        var regenerateKeys = new RegenerateKeysDto
        {
            RegenerateKeys = "True",
            KeyTimeToLive = arg.KeyTimeToLive
        };

        return BuildRequest(HttpMethod.Patch, $"api/protocols/s3/services/{arg.S3ServiceUuid}/users/{EncodedValue(arg.Username)}", arg.AccessToken, regenerateKeys);
    }

    public GetObjectAttributesRequest GetObjectAttributesRequest(GetObjectArg arg)
    {
        return new GetObjectAttributesRequest
        {
            BucketName = arg.BucketName,
            Key = arg.ObjectKey,
            ObjectAttributes =
            [
                ObjectAttributes.ETag,
            ]
        };
    }

    public GetObjectRequest GetObjectRequest(GetObjectArg arg)
    {
        return new GetObjectRequest
        {
            BucketName = arg.BucketName,
            Key = arg.ObjectKey,
        };
    }

    public ListBucketsRequest ListBucketsRequest(ListBucketsArg arg)
    {
        return new ListBucketsRequest
        {
            ContinuationToken = arg.ContinuationToken,
            MaxBuckets = arg.MaxBuckets ?? 10000,
            Prefix = arg.Prefix
        };
    }

    public ListObjectsV2Request ListFoldersInBucketRequest(ListFoldersInBucketArg arg)
    {
        return new ListObjectsV2Request
        {
            BucketName = arg.BucketName,
            ContinuationToken = arg.ContinuationToken,
            Delimiter = S3Constants.Delimiter,
            Prefix = arg.Prefix,
        };
    }

    public ListObjectsV2Request ListObjectsInBucketRequest(ListObjectsInBucketArg arg)
    {
        return new ListObjectsV2Request
        {
            BucketName = arg.BucketName,
            ContinuationToken = arg.ContinuationToken,
            MaxKeys = !string.IsNullOrEmpty(arg.MaxKeys) ? int.Parse(arg.MaxKeys) : 1000,
            Delimiter = S3Constants.Delimiter,
            Prefix = arg.Prefix,
        };
    }

    public PutObjectRequest UploadObjectRequest(UploadObjectArg arg)
    {
        return new PutObjectRequest
        {
            BucketName = arg.BucketName,
            DisablePayloadSigning = arg.DisablePayloadSigning,
            Key = arg.ObjectKey,
            InputStream = arg.Stream,
            Headers =
            {
                ContentLength = arg.ContentLength
            }
        };
    }

    public UploadPartRequest UploadPartRequest(UploadPartArg arg)
    {
        return new UploadPartRequest
        {
            BucketName = arg.BucketName,
            Key = arg.ObjectKey,
            PartNumber = arg.PartNumber,
            UploadId = arg.UploadId,
            InputStream = new MemoryStream(arg.PartData)
        };
    }

    public DeleteObjectRequest DeleteObjectRequest(DeleteFileOrFolderArg arg)
    {
        return new DeleteObjectRequest
        {
            BucketName = arg.BucketName,
            Key = arg.Path
        };
    }

    private static HttpRequestMessage BuildRequest<T>(HttpMethod method, string path, string accessToken, T? body = default)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add(HttpHeaderKeys.Authorization, $"Bearer {accessToken}");
        request.Headers.Add(HttpHeaderKeys.Accept, ContentType.ApplicationJson);
        if (body != null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, ContentType.ApplicationJson);
        }
        return request;
    }

    private static string EncodedValue(string value)
    {
        return HttpUtility.UrlEncode(value);
    }
}