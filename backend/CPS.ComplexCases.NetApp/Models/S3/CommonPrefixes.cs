using System.Xml.Serialization;

namespace CPS.ComplexCases.NetApp.Models.S3;

public class CommonPrefixes
{
    [XmlElement(ElementName = "Prefix")]
    public required string Prefix { get; set; }
}