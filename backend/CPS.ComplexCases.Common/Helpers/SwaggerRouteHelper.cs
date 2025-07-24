
namespace CPS.ComplexCases.Common.Helpers;

public static class SwaggerRouteHelper
{
    private static readonly string[] BlockedSwaggerRoutes =
    [
        "/api/swagger/ui",
        "/api/swagger.json",
        "/api/swagger"
    ];

    private static readonly string? Environment = System.Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
    public static bool IsProduction =>
        !string.IsNullOrEmpty(Environment) &&
        string.Equals(Environment, "Production", StringComparison.OrdinalIgnoreCase);

    public static bool IsSwaggerRoute(string path)
    {
        return BlockedSwaggerRoutes.Any(route =>
            path.StartsWith(route, StringComparison.OrdinalIgnoreCase));
    }
}