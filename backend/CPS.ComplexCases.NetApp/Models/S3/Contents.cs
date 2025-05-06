using System.Xml.Serialization;

namespace CPS.ComplexCases.NetApp.Models.S3;

public class Contents
{
    [XmlElement(ElementName = "ChecksumAlgorithm")]
    public string? ChecksumAlgorithm { get; set; }
    [XmlElement(ElementName = "ChecksumType")]
    public string? ChecksumType { get; set; }
    [XmlElement(ElementName = "ETag")]
    public required string ETag { get; set; }
    [XmlElement(ElementName = "Key")]
    public required string Key { get; set; }
    [XmlElement(ElementName = "LastModified")]
    public DateTime LastModified { get; set; }
    [XmlElement(ElementName = "Owner")]
    public Owner? Owner { get; set; }
    [XmlElement(ElementName = "RestoreStatus")]
    public RestoreStatus? RestoreStatus { get; set; }
    [XmlElement(ElementName = "Size")]
    public long Size { get; set; }
    [XmlElement(ElementName = "StorageClass")]
    public string? StorageClass { get; set; }
}