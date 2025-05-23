namespace CPS.ComplexCases.Common.Attributes;

public class AlternateValueAttribute(string value) : Attribute
{
    public string AlternateValue { get; protected set; } = value;
}