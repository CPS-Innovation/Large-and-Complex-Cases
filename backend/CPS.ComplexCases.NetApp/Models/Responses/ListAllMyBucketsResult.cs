using System.Xml.Serialization;
//using Amazon.S3.Model;

namespace CPS.ComplexCases.NetApp.Models.Responses;

[XmlRoot(Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
public class ListAllMyBucketsResult
{
    public Owner? Owner { get; set; }
    public List<Bucket> Buckets { get; set; } = [];
}

public class Owner
{
    [XmlElement(ElementName = "ID")]
    public string Id { get; set; }
    [XmlElement(ElementName = "DisplayName")]
    public string DisplayName { get; set; }
}

public class Bucket
{
    [XmlElement(ElementName = "Name")]
    public string BucketName { get; set; }
    [XmlElement(ElementName = "CreationDate")]
    public DateTime CreationDate { get; set; }
}