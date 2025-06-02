using System.Xml.Serialization;

namespace CPS.ComplexCases.NetApp.Models.S3.Result;

[XmlRoot(Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
public class CompleteMultipartUploadResult
{
    [XmlElement(ElementName = "ETag")]
    public string? ETag { get; set; }
    [XmlElement(ElementName = "Bucket")]
    public string? Bucket { get; set; }
    [XmlElement(ElementName = "ChecksumType")]
    public string? ChecksumType { get; set; }
    [XmlElement(ElementName = "Key")]
    public string? Key { get; set; }
    [XmlElement(ElementName = "Location")]
    public string? Location { get; set; }
}