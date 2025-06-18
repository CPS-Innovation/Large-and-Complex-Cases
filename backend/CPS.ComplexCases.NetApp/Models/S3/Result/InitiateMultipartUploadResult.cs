using System.Xml.Serialization;

namespace CPS.ComplexCases.NetApp.Models.S3.Result;

[XmlRoot(Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
public class InitiateMultipartUploadResult
{
    [XmlElement(ElementName = "UploadId")]
    public required string UploadId { get; set; }
    [XmlElement(ElementName = "Key")]
    public required string Key { get; set; }
    [XmlElement(ElementName = "Bucket")]
    public required string Bucket { get; set; }
}