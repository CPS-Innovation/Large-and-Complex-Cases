using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Validators;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Validators;

public class MoveNetAppBatchRequestValidatorTests
    : NetAppBatchRequestValidatorTestsBase<MoveNetAppBatchRequest, MoveNetAppBatchOperationRequest, MoveNetAppBatchRequestValidator>
{
    protected override MoveNetAppBatchRequest CreateValidRequest(List<MoveNetAppBatchOperationRequest>? ops = null) => new()
    {
        CaseId = 1,
        DestinationPrefix = "CaseRoot/Folder-B/",
        BearerToken = "token",
        BucketName = "bucket",
        Operations = ops ?? [new() { Type = "Material", SourcePath = "CaseRoot/file.txt" }]
    };

    protected override MoveNetAppBatchOperationRequest CreateOperation(string type, string sourcePath) =>
        new() { Type = type, SourcePath = sourcePath };

    protected override MoveNetAppBatchRequest CreateRequest(int caseId, string destinationPrefix, string bearerToken, string bucketName, List<MoveNetAppBatchOperationRequest> operations) => new()
    {
        CaseId = caseId,
        DestinationPrefix = destinationPrefix,
        BearerToken = bearerToken,
        BucketName = bucketName,
        Operations = operations
    };
}
