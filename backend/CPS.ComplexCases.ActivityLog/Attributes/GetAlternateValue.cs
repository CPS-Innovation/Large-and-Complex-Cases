using System.Reflection;

namespace CPS.ComplexCases.ActivityLog.Attributes
{
    public static class EnumExtensions
    {
        public static string GetAlternateValue(this Enum Value)
        {
            Type Type = Value.GetType();

            string? fieldName = Value?.ToString();
            if (string.IsNullOrEmpty(fieldName))
            {
                return string.Empty;
            }

            FieldInfo? FieldInfo = Type.GetField(fieldName);
            if (FieldInfo == null)
            {
                return string.Empty;
            }

            AlternateValueAttribute? attribute = FieldInfo.GetCustomAttribute(
                typeof(AlternateValueAttribute)
            ) as AlternateValueAttribute;

            return attribute?.AlternateValue ?? string.Empty;
        }
    }
}