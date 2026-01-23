using Amazon.S3.Model;
using CPS.ComplexCases.NetApp.Constants;
using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories;

public class NetAppArgFactory : INetAppArgFactory
{
    public CreateBucketArg CreateCreateBucketArg(string bearerToken, string bucketName)
    {
        return new CreateBucketArg
        {
            BearerToken = bearerToken,
            BucketName = bucketName.ToLowerInvariant(),
        };
    }

    public ListBucketsArg CreateListBucketsArg(string bearerToken, string? continuationToken = null, int? maxBuckets = null, string? prefix = null)
    {
        return new ListBucketsArg
        {
            BearerToken = bearerToken,
            BucketName = "",
            ContinuationToken = continuationToken,
            MaxBuckets = maxBuckets,
            Prefix = prefix
        };
    }

    public FindBucketArg CreateFindBucketArg(string bearerToken, string bucketName)
    {
        return new FindBucketArg
        {
            BearerToken = bearerToken,
            BucketName = bucketName.ToLowerInvariant()
        };
    }

    public CreateFolderArg CreateCreateFolderArg(string bearerToken, string bucketName, string folderKey)
    {
        return new CreateFolderArg
        {
            BearerToken = bearerToken,
            BucketName = bucketName.ToLowerInvariant(),
            FolderKey = folderKey
        };
    }

    public GetObjectArg CreateGetObjectArg(string bearerToken, string bucketName, string objectName)
    {
        return new GetObjectArg
        {
            BearerToken = bearerToken,
            BucketName = bucketName.ToLowerInvariant(),
            ObjectKey = objectName
        };
    }

    public UploadObjectArg CreateUploadObjectArg(string bearerToken, string bucketName, string objectName, Stream stream, long contentLength, bool disablePayloadSigning = true)
    {
        return new UploadObjectArg
        {
            BearerToken = bearerToken,
            BucketName = bucketName.ToLowerInvariant(),
            ObjectKey = objectName,
            Stream = stream,
            ContentLength = contentLength,
            DisablePayloadSigning = disablePayloadSigning
        };
    }

    public ListObjectsInBucketArg CreateListObjectsInBucketArg(string bearerToken, string bucketName, string? continuationToken = null, int? maxKeys = null, string? prefix = null, bool includeDelimiter = false)
    {
        return new ListObjectsInBucketArg
        {
            BearerToken = bearerToken,
            BucketName = bucketName.ToLowerInvariant(),
            ContinuationToken = continuationToken,
            MaxKeys = maxKeys.ToString(),
            Prefix = SetPrefix(prefix),
            IncludeDelimiter = includeDelimiter
        };
    }

    public ListFoldersInBucketArg CreateListFoldersInBucketArg(string bearerToken, string bucketName, string? operationName = null, string? continuationToken = null, int? maxKeys = null, string? prefix = null)
    {
        return new ListFoldersInBucketArg
        {
            BearerToken = bearerToken,
            BucketName = bucketName.ToLowerInvariant(),
            OperationName = operationName?.ToLowerInvariant(),
            ContinuationToken = continuationToken,
            MaxKeys = maxKeys.ToString(),
            Prefix = SetPrefix(prefix)
        };
    }

    public InitiateMultipartUploadArg CreateInitiateMultipartUploadArg(string bearerToken, string bucketName, string objectName)
    {
        return new InitiateMultipartUploadArg
        {
            BearerToken = bearerToken,
            BucketName = bucketName.ToLowerInvariant(),
            ObjectKey = objectName
        };
    }

    public UploadPartArg CreateUploadPartArg(string bearerToken, string bucketName, string objectName, byte[] partData, int partNumber, string uploadId)
    {
        return new UploadPartArg
        {
            BearerToken = bearerToken,
            BucketName = bucketName.ToLowerInvariant(),
            ObjectKey = objectName,
            PartData = partData,
            PartNumber = partNumber,
            UploadId = uploadId
        };
    }

    public CompleteMultipartUploadArg CreateCompleteMultipartUploadArg(string bearerToken, string bucketName, string objectName, string uploadId, Dictionary<int, string> parts)
    {
        return new CompleteMultipartUploadArg
        {
            BearerToken = bearerToken,
            BucketName = bucketName.ToLowerInvariant(),
            ObjectKey = objectName,
            UploadId = uploadId,
            CompletedParts = parts.Select(part => new PartETag
            {
                PartNumber = part.Key,
                ETag = part.Value
            }).ToList()
        };
    }

    public RegisterUserArg CreateRegisterUserArg(string username, string accessToken, Guid s3ServiceUuid)
    {
        return new RegisterUserArg
        {
            Username = username,
            AccessToken = accessToken,
            S3ServiceUuid = s3ServiceUuid
        };
    }

    public RegenerateUserKeysArg CreateRegenerateUserKeysArg(string username, string accessToken, Guid s3ServiceUuid, int sessionDuration)
    {
        return new RegenerateUserKeysArg
        {
            Username = username,
            AccessToken = accessToken,
            S3ServiceUuid = s3ServiceUuid,
            KeyTimeToLive = ConvertToIso8601(sessionDuration)
        };
    }

    private static string SetPrefix(string? prefix)
    {
        if (!string.IsNullOrEmpty(prefix))
        {
            prefix = !prefix.EndsWith(S3Constants.Delimiter) ? $"{prefix}{S3Constants.Delimiter}" : prefix;
        }

        return prefix ?? string.Empty;
    }

    private static string ConvertToIso8601(int durationInSeconds)
    {
        if (durationInSeconds <= 0)
            return "PT0S";

        TimeSpan timeSpan = TimeSpan.FromSeconds(durationInSeconds);

        string duration = "P";

        if (timeSpan.Days > 0)
        {
            duration += $"{timeSpan.Days}D";
        }

        if (timeSpan.Hours > 0 || timeSpan.Minutes > 0 || timeSpan.Seconds > 0)
        {
            duration += "T";

            if (timeSpan.Hours > 0)
            {
                duration += $"{timeSpan.Hours}H";
            }
            if (timeSpan.Minutes > 0)
            {
                duration += $"{timeSpan.Minutes}M";
            }
            if (timeSpan.Seconds > 0)
            {
                duration += $"{timeSpan.Seconds}S";
            }
        }

        return duration;
    }
}