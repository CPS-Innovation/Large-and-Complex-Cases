namespace CPS.ComplexCases.ActivityLog.Attributes;

public class AlternateValueAttribute(string value) : Attribute
{
    public string AlternateValue { get; protected set; } = value;
}