using System.Xml.Serialization;

namespace CPS.ComplexCases.NetApp.Models.S3;

public class Owner
{
    [XmlElement(ElementName = "ID")]
    public string? Id { get; set; }
    [XmlElement(ElementName = "DisplayName")]
    public string? DisplayName { get; set; }
}