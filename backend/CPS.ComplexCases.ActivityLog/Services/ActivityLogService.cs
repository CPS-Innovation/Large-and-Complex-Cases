using System.Text.Json;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.Data.Dtos;
using CPS.ComplexCases.Data.Repositories;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.ActivityLog.Models;

namespace CPS.ComplexCases.ActivityLog.Services;

public class ActivityLogService(IActivityLogRepository activityLogRepository, ILogger<ActivityLogService> logger) : IActivityLogService
{
    private readonly IActivityLogRepository _activityLogRepository = activityLogRepository;
    private readonly ILogger<ActivityLogService> _logger = logger;

    public async Task CreateActivityLogAsync(ActionType actionType, ResourceType resourceType, int caseId, string resourceId, string? resourceName, string? userName, JsonDocument? details = null)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            _logger.LogWarning("Attempted to create activity log with null or empty resourceId.");
            throw new ArgumentException("ResourceId cannot be null or empty.", nameof(resourceId));
        }

        _logger.LogInformation("Creating activity log for {ResourceType} {ResourceId}", resourceType, resourceId);
        var activityLog = new Data.Entities.ActivityLog
        {
            ActionType = actionType.GetAlternateValue(),
            ResourceType = resourceType.ToString(),
            CaseId = caseId,
            ResourceId = resourceId,
            ResourceName = resourceName,
            UserName = userName,
            Details = details,
            Timestamp = DateTime.UtcNow,
            Description = SetDescription(actionType, resourceName)
        };

        await _activityLogRepository.AddAsync(activityLog);
    }

    public Task<Data.Entities.ActivityLog?> GetActivityLogByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Attempted to get activity log with empty Guid.");
            throw new ArgumentException("Id cannot be Guid.Empty.", nameof(id));
        }

        _logger.LogInformation("Getting activity log by ID {Id}", id);

        return _activityLogRepository.GetByIdAsync(id);
    }

    public Task<IEnumerable<Data.Entities.ActivityLog>> GetActivityLogsAsync(ActivityLogFilterDto filter)
    {
        _logger.LogInformation("Getting activity logs with filter {@Filter}", filter);

        return _activityLogRepository.GetByFilterAsync(filter);
    }

    public Task<IEnumerable<Data.Entities.ActivityLog>> GetActivityLogsByResourceIdAsync(string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            _logger.LogWarning("Attempted to get activity logs with null or empty resourceId.");
            throw new ArgumentException("ResourceId cannot be null or empty.", nameof(resourceId));
        }

        _logger.LogInformation("Getting activity logs for {ResourceId}", resourceId);

        return _activityLogRepository.GetByResourceIdAsync(resourceId);
    }

    public Task<Data.Entities.ActivityLog?> UpdateActivityLogAsync(Data.Entities.ActivityLog activityLog)
    {
        if (string.IsNullOrWhiteSpace(activityLog.ResourceId))
        {
            _logger.LogWarning("Attempted to update activity log with null or empty resourceId.");
            throw new ArgumentException("ResourceId cannot be null or empty.", nameof(activityLog.ResourceId));
        }
        if (string.IsNullOrWhiteSpace(activityLog.ResourceType))
        {
            _logger.LogWarning("Attempted to update activity log with null or empty resourceType.");
            throw new ArgumentException("ResourceType cannot be null or empty.", nameof(activityLog.ResourceType));
        }
        if (activityLog.Id == Guid.Empty)
        {
            _logger.LogWarning("Attempted to update activity log with empty Guid.");
            throw new ArgumentException("ActivityLog Id cannot be Guid.Empty.", nameof(activityLog.Id));
        }

        _logger.LogInformation("Updating activity log for case {ResourceType} {ResourceId}", activityLog.ResourceType, activityLog.ResourceId);

        return _activityLogRepository.UpdateAsync(activityLog);
    }

    public JsonDocument? ConvertToJsonDocument<T>(T data)
    {
        try
        {
            return JsonDocument.Parse(JsonSerializer.Serialize(data));
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error converting data to JsonDocument");
            return null;
        }
    }

    public string GenerateFileDetailsCsvAsync(Data.Entities.ActivityLog activityLog)
    {
        _logger.LogInformation("Generating file details CSV for activity log {ActivityLogId}", activityLog.Id);

        if (activityLog.Details == null)
        {
            _logger.LogWarning("Activity log details are null for ID {ActivityLogId}", activityLog.Id);
            return string.Empty;
        }

        var fileRecords = ExtractFileRecordsFromDetails(activityLog.Details);

        if (fileRecords.Count == 0)
        {
            _logger.LogInformation("No file records to export for activity log ID: {ActivityLogId}", activityLog.Id);
            return string.Empty;
        }

        return CsvGeneratorHelper.GenerateCsv(fileRecords);
    }

    private List<FileRecordCsvDto> ExtractFileRecordsFromDetails(JsonDocument details)
    {
        var fileRecords = new List<FileRecordCsvDto>();

        if (details?.RootElement.ValueKind != JsonValueKind.Object)
        {
            _logger.LogWarning("Details is not a valid JSON object or is null");
            return fileRecords;
        }

        try
        {
            var rootElement = details.RootElement;

            // Handle Files (Success)
            if (rootElement.TryGetProperty("Files", out var filesElement) &&
                filesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var fileElement in filesElement.EnumerateArray())
                {
                    var path = fileElement.TryGetProperty("Path", out var pathElement) ? pathElement.GetString() : null;
                    fileRecords.Add(new FileRecordCsvDto
                    {
                        Path = path,
                        FileName = !string.IsNullOrEmpty(path) ? Path.GetFileName(path) : null,
                        Status = FileRecordStatus.Success
                    });
                }
            }

            // Handle Errors (Fail)
            if (rootElement.TryGetProperty("Errors", out var errorsElement) &&
                errorsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var errorElement in errorsElement.EnumerateArray())
                {
                    var path = errorElement.TryGetProperty("Path", out var pathElement) ? pathElement.GetString() : null;
                    fileRecords.Add(new FileRecordCsvDto
                    {
                        Path = path,
                        FileName = !string.IsNullOrEmpty(path) ? Path.GetFileName(path) : null,
                        Status = FileRecordStatus.Fail
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting file records from JSON details");
        }

        _logger.LogInformation("Extracted {Count} file records from activity log details", fileRecords.Count);
        return fileRecords;
    }

    private static string SetDescription(ActionType actionType, string? resourceName)
    {
        return actionType switch
        {
            ActionType.ConnectionToEgress => $"Connected to Egress workspace {resourceName}",
            ActionType.ConnectionToNetApp => $"Connected to NetApp folder {resourceName}",
            ActionType.TransferInitiated => $"Transfer initiated between {resourceName}",
            ActionType.TransferCompleted => $"Transfer completed between {resourceName}",
            ActionType.TransferFailed => $"Transfer failed between {resourceName}",
            _ => $"Performed action {actionType} on resource {resourceName}"
        };
    }
}
