namespace CPS.ComplexCases.NetApp.Models
{
    public class NetAppOptions
    {
        public required string Url { get; set; }
        public required string AccessKey { get; set; }
        public required string SecretKey { get; set; }
        public required string RegionName { get; set; }
    }
}