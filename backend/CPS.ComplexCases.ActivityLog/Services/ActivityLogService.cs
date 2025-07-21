using System.Text.Json;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Models;
using CPS.ComplexCases.ActivityLog.Models.Responses;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Models.Domain.Dto;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Data.Dtos;
using CPS.ComplexCases.Data.Repositories;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.ActivityLog.Extensions;

namespace CPS.ComplexCases.ActivityLog.Services;

public class ActivityLogService(IActivityLogRepository activityLogRepository, ILogger<ActivityLogService> logger) : IActivityLogService
{
    private readonly IActivityLogRepository _activityLogRepository = activityLogRepository;
    private readonly ILogger<ActivityLogService> _logger = logger;
    private const string EgressResource = "Egress";
    private const string SharedDriveResource = "Shared Drive";

    public async Task CreateActivityLogAsync(ActionType actionType, ResourceType resourceType, int caseId, string resourceId, string? resourceName, string? userName, JsonDocument? details = null)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            _logger.LogWarning("Attempted to create activity log with null or empty resourceId.");
            throw new ArgumentException("ResourceId cannot be null or empty.", nameof(resourceId));
        }

        FileTransferDetails? transferDetails = null;

        if (actionType.ToString().StartsWith("Transfer") && details != null)
        {
            transferDetails = details.DeserializeJsonDocument<FileTransferDetails>(_logger);
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
            Description = SetDescription(actionType, resourceName, transferDetails)
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

    public async Task<ActivityLogsResponse> GetActivityLogsAsync(ActivityLogFilterDto filter)
    {
        _logger.LogInformation("Getting activity logs with filter {@Filter}", filter);

        var result = await _activityLogRepository.GetByFilterAsync(filter);

        return new ActivityLogsResponse
        {
            Data = result.Logs,
            Pagination = new PaginationDto
            {
                TotalResults = result.TotalCount,
                Skip = filter.Skip,
                Take = filter.Take,
                Count = result.Logs.Count()
            }
        };
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

    private List<FileRecordCsvDto> ExtractFileRecordsFromDetails(JsonDocument? details)
    {
        var fileRecords = new List<FileRecordCsvDto>();

        if (details == null)
        {
            _logger.LogWarning("Details are null");
            return fileRecords;
        }

        var rootElement = details.RootElement;
        if (rootElement.ValueKind != JsonValueKind.Object)
        {
            _logger.LogWarning("Details is not a valid JSON object");
            return fileRecords;
        }

        int successCount = 0, failCount = 0;

        try
        {
            if (rootElement.TryGetProperty("Files", out var filesElement) &&
                filesElement.ValueKind == JsonValueKind.Array)
            {
                successCount = AddFileRecords(filesElement, FileRecordStatus.Success, fileRecords);
            }

            if (rootElement.TryGetProperty("Errors", out var errorsElement) &&
                errorsElement.ValueKind == JsonValueKind.Array)
            {
                failCount = AddFileRecords(errorsElement, FileRecordStatus.Fail, fileRecords);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting file records from JSON details");
        }

        _logger.LogInformation("Extracted {SuccessCount} successful and {FailCount} failed file records from activity log details", successCount, failCount);
        return fileRecords;
    }

    private static int AddFileRecords(JsonElement parent, FileRecordStatus status, List<FileRecordCsvDto> records)
    {
        int count = 0;
        foreach (var element in parent.EnumerateArray())
        {
            var path = element.TryGetProperty("Path", out var pathElement) ? pathElement.GetString() : null;
            records.Add(new FileRecordCsvDto
            {
                Path = path,
                FileName = !string.IsNullOrEmpty(path) ? Path.GetFileName(path) : null,
                Status = status
            });
            count++;
        }
        return count;
    }

    private static string SetDescription(ActionType actionType, string? resourceName, FileTransferDetails? transferDetails = null)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            resourceName = transferDetails?.SourcePath ?? "Unknown Resource";
        }

        if (transferDetails != null)
        {
            var transferType = transferDetails.TransferType == TransferType.Copy.ToString() ? "copied" : "moved";
            var (source, destination) = transferDetails.TransferDirection == TransferDirection.EgressToNetApp.ToString()
                ? (EgressResource, SharedDriveResource)
                : (SharedDriveResource, EgressResource);

            return actionType switch
            {
                ActionType.TransferInitiated => $"Transfer initiated from {source} to {destination}",
                ActionType.TransferCompleted => $"Documents/folders {transferType} from {source} to {destination}",
                ActionType.TransferFailed => $"Transfer failed from {source} to {destination}",
                _ => $"Performed action {actionType} on resource {resourceName}"
            };
        }

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
