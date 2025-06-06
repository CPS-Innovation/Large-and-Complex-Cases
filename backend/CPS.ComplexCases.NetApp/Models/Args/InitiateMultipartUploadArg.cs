namespace CPS.ComplexCases.NetApp.Models.Args
{
    public class InitiateMultipartUploadArg
    {
        public required string BucketName { get; set; }
        public required string ObjectKey { get; set; }
    }
}