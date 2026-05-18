using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.API.Tests.Unit.Validators;

public class CopyNetAppBatchRequestValidatorTests
    : NetAppBatchDtoValidatorTestsBase<CopyNetAppBatchDto, CopyNetAppBatchOperationDto, CopyNetAppBatchRequestValidator>
{
    protected override CopyNetAppBatchDto CreateValidBatch(List<CopyNetAppBatchOperationDto>? ops = null) => new()
    {
        CaseId = 1,
        DestinationPrefix = "CaseRoot/Folder-B/",
        Operations = ops ?? [new() { Type = NetAppBatchOperationType.Material, SourcePath = "CaseRoot/file.txt" }]
    };

    protected override CopyNetAppBatchOperationDto CreateOperation(NetAppBatchOperationType type, string sourcePath) =>
        new() { Type = type, SourcePath = sourcePath };
}
