using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class UpdateActivityLog(IActivityLogService activityLogService)
{
    private readonly IActivityLogService _activityLogService = activityLogService;

    [Function(nameof(UpdateActivityLog))]
    public async Task Run([ActivityTrigger] UpdateActivityLogPayload payload, [DurableClient] DurableTaskClient client)
    {
        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload), "UpdateActivityLog payload cannot be null.");
        }

        await _activityLogService.ThrowIfNull(nameof(_activityLogService));
    }
}