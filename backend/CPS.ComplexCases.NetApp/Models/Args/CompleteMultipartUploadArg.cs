using Amazon.S3.Model;

namespace CPS.ComplexCases.NetApp.Models.Args;

public class CompleteMultipartUploadArg
{
    public required string BucketName { get; set; }
    public required string ObjectKey { get; set; }
    public required string UploadId { get; set; }
    public required List<PartETag> CompletedParts { get; set; }
}