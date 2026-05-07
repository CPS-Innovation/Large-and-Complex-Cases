using System.Net;
using System.Reflection;
using CPS.ComplexCases.API.Attributes;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.Common.Functions;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.OpenApi.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

// >>> ADDED
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
// <<< END ADDED

namespace CPS.ComplexCases.API.Functions;

public class Status(
    ILogger<Status> logger,
    DbContext dbContext // >>> ADDED
)
{
  private readonly ILogger<Status> _logger = logger;

  // >>> ADDED
  private readonly DbContext _dbContext = dbContext;
  // <<< END ADDED
  
  [Function(nameof(Status))]
  [OpenApiOperation(operationId: nameof(Status), tags: ["Health"], Description = "Gets the current status of the function app.")]
  [OpenApiNoSecurity]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(AssemblyStatus), Description = ApiResponseDescriptions.Success)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
  public IActionResult Run(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequest req,
      [HttpTelemetry] object leaveThisInPlace)
  {
    _logger.LogDebug("Calling the Status EndPoint.");

    // >>> ADDED: one-off managed identity write test
    try
    {
        var id = Guid.NewGuid();
        var username = $"mi-check-{id}";

        _dbContext.Database.ExecuteSqlRaw(
            """
            INSERT INTO large_complex_cases.test_new_table_perms (Id, UserName)
            VALUES (@id, @username)
            """,
            new SqlParameter("@id", id),
            new SqlParameter("@username", username));

        _dbContext.Database.ExecuteSqlRaw(
            """
            DELETE FROM large_complex_cases.test_new_table_perms
            WHERE Id = @id
            """,
            new SqlParameter("@id", id));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Managed identity write test failed for large_complex_cases.test_new_table_perms");
        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }
    // <<< END ADDED

    return StatusFunction.GetStatus(Assembly.GetExecutingAssembly());
  }
}