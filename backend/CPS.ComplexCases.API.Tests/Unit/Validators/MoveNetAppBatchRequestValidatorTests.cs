using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.API.Tests.Unit.Validators;

public class MoveNetAppBatchRequestValidatorTests
    : NetAppBatchDtoValidatorTestsBase<MoveNetAppBatchDto, MoveNetAppBatchOperationDto, MoveNetAppBatchRequestValidator>
{
    protected override MoveNetAppBatchDto CreateValidBatch(List<MoveNetAppBatchOperationDto>? ops = null) => new()
    {
        CaseId = 1,
        DestinationPrefix = "CaseRoot/Folder-B/",
        Operations = ops ?? [new() { Type = NetAppBatchOperationType.Material, SourcePath = "CaseRoot/file.txt" }]
    };

    protected override MoveNetAppBatchOperationDto CreateOperation(NetAppBatchOperationType type, string sourcePath) =>
        new() { Type = type, SourcePath = sourcePath };
}
