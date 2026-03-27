using System.Net;

namespace CPS.ComplexCases.NetApp.Exceptions;

public class NetAppAccessDeniedException : Exception
{
    public string BucketName { get; }

    public NetAppAccessDeniedException(string bucketName, Exception innerException)
        : base($"Access denied to NetApp bucket: {bucketName}", innerException)
    {
        BucketName = bucketName;
    }

    public static HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
}