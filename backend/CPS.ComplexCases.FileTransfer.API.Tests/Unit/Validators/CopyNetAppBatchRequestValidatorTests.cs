using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Validators;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Validators;

public class CopyNetAppBatchRequestValidatorTests
    : NetAppBatchRequestValidatorTestsBase<CopyNetAppBatchRequest, CopyNetAppBatchOperationRequest, CopyNetAppBatchRequestValidator>
{
    protected override CopyNetAppBatchRequest CreateValidRequest(List<CopyNetAppBatchOperationRequest>? ops = null) => new()
    {
        CaseId = 1,
        DestinationPrefix = "CaseRoot/Folder-B/",
        BearerToken = "token",
        BucketName = "bucket",
        Operations = ops ?? [new() { Type = "Material", SourcePath = "CaseRoot/file.txt" }]
    };

    protected override CopyNetAppBatchOperationRequest CreateOperation(string type, string sourcePath) =>
        new() { Type = type, SourcePath = sourcePath };

    protected override CopyNetAppBatchRequest CreateRequest(int caseId, string destinationPrefix, string bearerToken, string bucketName, List<CopyNetAppBatchOperationRequest> operations) => new()
    {
        CaseId = caseId,
        DestinationPrefix = destinationPrefix,
        BearerToken = bearerToken,
        BucketName = bucketName,
        Operations = operations
    };
}
