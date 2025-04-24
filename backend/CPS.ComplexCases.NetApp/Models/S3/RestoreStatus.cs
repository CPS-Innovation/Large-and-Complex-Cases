using System.Xml.Serialization;

namespace CPS.ComplexCases.NetApp.Models.S3;

public class RestoreStatus
{
    [XmlElement(ElementName = "IsRestoreInProgress")]
    public bool IsRestoreInProgress { get; set; }
    [XmlElement(ElementName = "RestoreExpiryDate")]
    public DateTime RestoreExpiryDate { get; set; }
}