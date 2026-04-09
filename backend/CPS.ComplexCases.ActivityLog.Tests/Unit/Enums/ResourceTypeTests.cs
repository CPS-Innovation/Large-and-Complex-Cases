using CPS.ComplexCases.ActivityLog.Enums;

namespace CPS.ComplexCases.ActivityLog.Tests.Unit.Enums;

public class ResourceTypeTests
{
    [Theory]
    [InlineData(ResourceType.FileTransfer, "FileTransfer")]
    [InlineData(ResourceType.TransferItem, "TransferItem")]
    [InlineData(ResourceType.TransferItemPart, "TransferItemPart")]
    [InlineData(ResourceType.StorageConnection, "StorageConnection")]
    [InlineData(ResourceType.NetAppFolder, "NetAppFolder")]
    [InlineData(ResourceType.Material, "Material")]
    public void ToString_ReturnsExpectedString(ResourceType resourceType, string expected)
    {
        Assert.Equal(expected, resourceType.ToString());
    }
}
