namespace CPS.ComplexCases.Common.Attributes;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class AlternateValueAttribute(string value) : Attribute
{
    public string AlternateValue { get; protected set; } = value;
}