using System.Xml.Serialization;

namespace CPS.ComplexCases.NetApp.Models.S3;

public class Bucket
{
    [XmlElement(ElementName = "Name")]
    public string? BucketName { get; set; }
    [XmlElement(ElementName = "CreationDate")]
    public DateTime CreationDate { get; set; }
}