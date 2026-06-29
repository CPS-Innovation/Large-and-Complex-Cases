using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CPS.ComplexCases.API.Constants;

namespace CPS.ComplexCases.API.Extensions;

public static class HttpRequestExtensions
{
    /// <summary>
    /// Tries to parse the required <c>caseId</c> integer query parameter.
    /// Returns <c>true</c> and sets <paramref name="caseId"/> when successful;
    /// otherwise returns <c>false</c> and sets <paramref name="error"/> to a <see cref="BadRequestObjectResult"/>.
    /// </summary>
    public static bool TryGetCaseId(this HttpRequest req, out int caseId, out IActionResult? error)
    {
        var caseIdQuery = req.Query[InputParameters.CaseId];
        if (string.IsNullOrEmpty(caseIdQuery) || !int.TryParse(caseIdQuery, out caseId))
        {
            caseId = 0;
            error = new BadRequestObjectResult("Invalid or missing caseId query parameter.");
            return false;
        }

        error = null;
        return true;
    }
}
