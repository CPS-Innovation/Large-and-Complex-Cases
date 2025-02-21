namespace CPS.ComplexCases.NetApp.Models.Args
{
    public class ListObjectsInBucketArg
    {
        public required string BucketName { get; set; }
        public string? ContinuationToken { get; set; }
    }
}