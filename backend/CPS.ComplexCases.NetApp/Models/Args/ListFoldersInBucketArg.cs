namespace CPS.ComplexCases.NetApp.Models.Args
{
    public class ListFoldersInBucketArg
    {
        public required string BucketName { get; set; }
        public string? ContinuationToken { get; set; }
        public string? Prefix { get; set; }
    }
}