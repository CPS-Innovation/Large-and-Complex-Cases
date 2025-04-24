using System.Xml.Serialization;

namespace CPS.ComplexCases.NetApp.Models.S3.Result;

[XmlRoot(Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
public class ListBucketResult
{
    [XmlElement(ElementName = "IsTruncated")]
    public bool IsTruncated { get; set; }
    [XmlElement(ElementName = "Contents")]
    public Contents? Contents { get; set; }
    [XmlElement(ElementName = "Name")]
    public string? Name { get; set; }
    [XmlElement(ElementName = "Prefix")]
    public string? Prefix { get; set; }
    [XmlElement(ElementName = "Delimiter")]
    public string? Delimiter { get; set; }
    [XmlElement(ElementName = "MaxKeys")]
    public int? MaxKeys { get; set; }
    [XmlElement(ElementName = "CommonPrefixes")]
    public List<CommonPrefixes>? CommonPrefixes { get; set; }
    [XmlElement(ElementName = "ContinuationToken")]
    public string? ContinuationToken { get; set; }
    [XmlElement(ElementName = "NextContinuationToken")]
    public string? NextContinuationToken { get; set; }
    [XmlElement(ElementName = "EncodingType")]
    public string? EncodingType { get; set; }
    [XmlElement(ElementName = "KeyCount")]
    public int KeyCount { get; set; }
    [XmlElement(ElementName = "StartAfter")]
    public string? StartAfter { get; set; }
}
