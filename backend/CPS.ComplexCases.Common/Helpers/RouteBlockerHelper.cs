namespace CPS.ComplexCases.Common.Helpers;

public static class RouteBlockerHelper
{
    private static readonly string[] BlockedProductionRoutes =
    [
        "/api/swagger/ui",
        "/api/swagger.json",
        "/api/swagger",
        "/api/tactical/login"
    ];

    private static readonly string? Environment = System.Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");

    public static bool IsProduction =>
        !string.IsNullOrEmpty(Environment) &&
        string.Equals(Environment, "Production", StringComparison.OrdinalIgnoreCase);

    public static bool IsBlockedRoute(string path)
    {
        return BlockedProductionRoutes.Any(route =>
            path.StartsWith(route, StringComparison.OrdinalIgnoreCase));
    }

    public static bool ShouldBlockRoute(string path)
    {
        return IsProduction && IsBlockedRoute(path);
    }
}