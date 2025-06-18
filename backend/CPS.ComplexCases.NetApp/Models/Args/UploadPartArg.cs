namespace CPS.ComplexCases.NetApp.Models.Args
{
    public class UploadPartArg
    {
        public required string BucketName { get; set; }
        public required string ObjectKey { get; set; }
        public required int PartNumber { get; set; }
        public required string UploadId { get; set; }
        public required byte[] PartData { get; set; }
    }
}