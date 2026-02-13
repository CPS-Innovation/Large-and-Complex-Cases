
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;

public class DurableTaskClientStub : DurableTaskClient
{
    private readonly DurableEntityClient _entityClient;

    public DurableTaskClientStub(DurableEntityClient entityClient)
        : base("TestConnection")
    {
        _entityClient = entityClient;
    }

    public Func<OrchestrationQuery?, AsyncPageable<OrchestrationMetadata>>? OnGetAllInstancesAsync { get; set; }
    public Func<PurgeInstancesFilter, CancellationToken, Task<PurgeResult>>? OnPurgeAllInstancesAsync { get; set; }
    
    public OrchestrationQuery? CapturedQuery { get; private set; }
    public PurgeInstancesFilter? CapturedPurgeFilter { get; private set; }

    public override DurableEntityClient Entities => _entityClient;

    public override ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    public override AsyncPageable<OrchestrationMetadata> GetAllInstancesAsync(OrchestrationQuery? filter = null)
    {
        CapturedQuery = filter;
        
        if (OnGetAllInstancesAsync == null)
        {
            throw new InvalidOperationException("OnGetAllInstancesAsync delegate is not set.");
        }
        
        return OnGetAllInstancesAsync(filter);
    }

    public override Task<OrchestrationMetadata?> GetInstancesAsync(string instanceId, bool getInputsAndOutputs = false, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public override Task RaiseEventAsync(string instanceId, string eventName, object? eventPayload = null, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public override Task ResumeInstanceAsync(string instanceId, string? reason = null, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public override Task<string> ScheduleNewOrchestrationInstanceAsync(TaskName orchestratorName, object? input = null, StartOrchestrationOptions? options = null, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public override Task SuspendInstanceAsync(string instanceId, string? reason = null, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OrchestrationMetadata> WaitForInstanceCompletionAsync(string instanceId, bool getInputsAndOutputs = false, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OrchestrationMetadata> WaitForInstanceStartAsync(string instanceId, bool getInputsAndOutputs = false, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public override Task<PurgeResult> PurgeAllInstancesAsync(PurgeInstancesFilter filter, CancellationToken cancellation = default)
    {
        CapturedPurgeFilter = filter;
        
        if (OnPurgeAllInstancesAsync != null)
        {
            return OnPurgeAllInstancesAsync(filter, cancellation);
        }
        
        return Task.FromResult(new PurgeResult(0));
    }
}
