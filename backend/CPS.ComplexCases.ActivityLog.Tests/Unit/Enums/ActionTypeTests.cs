using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.Common.Attributes;

namespace CPS.ComplexCases.ActivityLog.Tests.Unit.Enums;

public class ActionTypeTests
{
    [Theory]
    [InlineData(ActionType.FolderCreated, "FOLDER_CREATED")]
    [InlineData(ActionType.FolderDeleted, "FOLDER_DELETED")]
    [InlineData(ActionType.FolderRenamed, "FOLDER_RENAMED")]
    [InlineData(ActionType.FolderCopied, "FOLDER_COPIED")]
    [InlineData(ActionType.FolderMoved, "FOLDER_MOVED")]
    [InlineData(ActionType.MaterialRenamed, "MATERIAL_RENAMED")]
    [InlineData(ActionType.MaterialMoved, "MATERIAL_MOVED")]
    [InlineData(ActionType.FileMoved, "FILE_MOVED")]
    public void GetAlternateValue_ReturnsExpectedString(ActionType actionType, string expected)
    {
        Assert.Equal(expected, actionType.GetAlternateValue());
    }
}
