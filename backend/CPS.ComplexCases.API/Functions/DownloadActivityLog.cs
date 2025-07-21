using System.Net;
using System.Text;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.API.Functions;

public class DownloadActivityLog(ILogger<DownloadActivityLog> logger, IActivityLogService activityLogService)
{
    private readonly ILogger<DownloadActivityLog> logger = logger;
    private readonly IActivityLogService activityLogService = activityLogService;

    [Function(nameof(DownloadActivityLog))]
    [OpenApiOperation(operationId: nameof(DownloadActivityLog), tags: ["ActivityLog"], Description = "Download activity log data in CSV format.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/csv", bodyType: typeof(byte[]), Description = "CSV file containing activity log file paths")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/activity/{activityId}/logs/download")] HttpRequest req, FunctionContext context, Guid activityId)
    {

        logger.LogInformation("DownloadActivityLog function triggered for activityId: {ActivityId}", activityId);

        var activityLog = await activityLogService.GetActivityLogByIdAsync(activityId);

        if (activityLog == null)
        {
            logger.LogWarning("Activity log not found for ID: {ActivityId}", activityId);
            return new NotFoundObjectResult("Activity log not found");
        }

        var csvContent = activityLogService.GenerateFileDetailsCsvAsync(activityLog);

        if (string.IsNullOrEmpty(csvContent))
        {
            logger.LogInformation("No file paths found in activity log details for ID: {ActivityId}", activityId);
            return new BadRequestObjectResult("No file paths found in activity log details");
        }

        var csvBytes = Encoding.UTF8.GetBytes(csvContent);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var fileName = $"activity-log-{activityId}-files-{timestamp}.csv";

        return new FileContentResult(csvBytes, "text/csv")
        {
            FileDownloadName = fileName
        };
    }
}