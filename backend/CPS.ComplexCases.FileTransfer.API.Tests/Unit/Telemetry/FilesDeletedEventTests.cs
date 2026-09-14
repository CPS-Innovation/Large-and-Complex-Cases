using CPS.ComplexCases.FileTransfer.API.Telemetry;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Telemetry;

public class FilesDeletedEventTests
{
    [Fact]
    public void ToTelemetryEventProps_IncludesFailureReasons_WhenPresent()
    {
        var telemetryEvent = new FilesDeletedEvent
        {
            CaseId = 42,
            TransferId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            TransferDirection = "EgressToNetApp",
            IsSuccessful = false,
            FailureReasons = "locked (1); not found (1)",
            TotalFilesDeleted = 3,
            TotalFilesFailedToDelete = 2
        };

        var (properties, metrics) = telemetryEvent.ToTelemetryEventProps();

        Assert.Equal("locked (1); not found (1)", properties[nameof(FilesDeletedEvent.FailureReasons)]);
        Assert.Equal("False", properties[nameof(FilesDeletedEvent.IsSuccessful)]);
        Assert.Equal(3, metrics[nameof(FilesDeletedEvent.TotalFilesDeleted)]);
        Assert.Equal(2, metrics[nameof(FilesDeletedEvent.TotalFilesFailedToDelete)]);
    }

    [Fact]
    public void ToTelemetryEventProps_OmitsFailureReasons_WhenEmpty()
    {
        var telemetryEvent = new FilesDeletedEvent
        {
            TransferDirection = "EgressToNetApp",
            IsSuccessful = true
        };

        var (properties, _) = telemetryEvent.ToTelemetryEventProps();

        Assert.False(properties.ContainsKey(nameof(FilesDeletedEvent.FailureReasons)));
        Assert.Equal("True", properties[nameof(FilesDeletedEvent.IsSuccessful)]);
    }
}
