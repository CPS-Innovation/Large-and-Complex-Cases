using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.Data.Enums;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.NetApp.Constants;
using CPS.ComplexCases.NetApp.Models;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class DeleteNetAppBatchHelperTests
{
    [Fact]
    public void CreateOutOfCaseDeleteFailure_WhenPathInCase_ReturnsNull()
    {
        var op = new DeleteNetAppBatchOperationDto
        {
            Type = NetAppOperationType.Material,
            SourcePath = "case/file.txt"
        };

        Assert.Null(DeleteNetAppBatch.CreateOutOfCaseDeleteFailure(op, "case/"));
    }

    [Fact]
    public void CreateOutOfCaseDeleteFailure_WhenPathOutsideCase_ReturnsFailedItem()
    {
        var op = new DeleteNetAppBatchOperationDto
        {
            Type = NetAppOperationType.Material,
            SourcePath = "other/file.txt"
        };

        var result = DeleteNetAppBatch.CreateOutOfCaseDeleteFailure(op, "case/");

        Assert.NotNull(result);
        Assert.Equal(OperationResultStatus.Failed, result!.Status);
        Assert.Equal("Path is not within the case's NetApp folder.", result.Error);
    }

    [Fact]
    public void MapDeleteResultToItemResult_WhenNotSuccess_ReturnsFailedBeforeNotFound()
    {
        var op = new DeleteNetAppBatchOperationDto
        {
            Type = NetAppOperationType.Material,
            SourcePath = "case/file.txt"
        };
        var deleteResult = new DeleteNetAppResult(Success: false, WasFound: false, KeysDeleted: 0, ErrorMessage: "boom", ErrorStatusCode: null);

        var item = DeleteNetAppBatch.MapDeleteResultToItemResult(op, deleteResult);

        Assert.Equal(OperationResultStatus.Failed, item.Status);
        Assert.Equal("boom", item.Error);
    }

    [Fact]
    public void MapDeleteResultToItemResult_WhenNotFound_ReturnsNotFound()
    {
        var op = new DeleteNetAppBatchOperationDto
        {
            Type = NetAppOperationType.Material,
            SourcePath = "case/file.txt"
        };
        var deleteResult = new DeleteNetAppResult(Success: true, WasFound: false, KeysDeleted: 0, ErrorMessage: null, ErrorStatusCode: null);

        var item = DeleteNetAppBatch.MapDeleteResultToItemResult(op, deleteResult);

        Assert.Equal(OperationResultStatus.NotFound, item.Status);
    }

    [Fact]
    public void MapDeleteResultToItemResult_WhenDeleted_SetsKeysDeletedOnlyWhenGreaterThanOne()
    {
        var op = new DeleteNetAppBatchOperationDto
        {
            Type = NetAppOperationType.Folder,
            SourcePath = "case/folder/"
        };

        var single = DeleteNetAppBatch.MapDeleteResultToItemResult(
            op, new DeleteNetAppResult(true, true, 1, null, null));
        var multi = DeleteNetAppBatch.MapDeleteResultToItemResult(
            op, new DeleteNetAppResult(true, true, 3, null, null));

        Assert.Equal(OperationResultStatus.Deleted, single.Status);
        Assert.Null(single.KeysDeleted);
        Assert.Equal(3, multi.KeysDeleted);
    }

    [Theory]
    [InlineData(true, true, ActivityLog.Enums.ActionType.FolderAndMaterialDeleted)]
    [InlineData(true, false, ActivityLog.Enums.ActionType.FolderDeleted)]
    [InlineData(false, true, ActivityLog.Enums.ActionType.MaterialDeleted)]
    [InlineData(false, false, ActivityLog.Enums.ActionType.MaterialDeleted)]
    public void ResolveDeleteActivityTypes_MapsFolderMaterialMix(
        bool hasFolder, bool hasMaterial, ActivityLog.Enums.ActionType expected)
    {
        var (actionType, _) = DeleteNetAppBatch.ResolveDeleteActivityTypes(hasFolder, hasMaterial);
        Assert.Equal(expected, actionType);
    }
}
