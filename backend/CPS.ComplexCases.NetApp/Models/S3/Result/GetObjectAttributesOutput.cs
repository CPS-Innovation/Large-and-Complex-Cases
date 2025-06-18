using System.Xml.Serialization;

namespace CPS.ComplexCases.NetApp.Models.S3.Result;

[XmlRoot(Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
public class GetObjectAttributesOutput
{
    [XmlElement(ElementName = "ETag")]
    public string ETag { get; set; } = string.Empty;
}