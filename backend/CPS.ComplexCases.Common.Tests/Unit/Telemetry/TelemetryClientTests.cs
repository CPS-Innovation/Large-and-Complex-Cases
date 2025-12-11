using AutoFixture;
using CPS.ComplexCases.Common.Telemetry;
using Microsoft.ApplicationInsights.DataContracts;
using Moq;

namespace CPS.ComplexCases.Common.Tests.Unit.Telemetry;

public class TelemetryClientTest
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IAppInsightsTelemetryClient> _mockAppInsightsTelemetryClient;
    private readonly TelemetryClient _telemetryClient;

    public TelemetryClientTest()
    {
        _mockAppInsightsTelemetryClient = new Mock<IAppInsightsTelemetryClient>();
        _telemetryClient = new TelemetryClient(_mockAppInsightsTelemetryClient.Object);
    }

    [Fact]
    public void TrackEvent_WithValidEvent_CallsTrackEventWithCorrectParameters()
    {
        // Arrange
        var expectedEventName = "TestTelemetry";
        var telemetryEvent = new TestTelemetryEvent
        {
            CorrelationId = _fixture.Create<Guid>(),
            EventTimestamp = _fixture.Create<DateTime>()
        };

        // Act
        _telemetryClient.TrackEvent(telemetryEvent);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackEvent(
                expectedEventName,
                It.Is<IDictionary<string, string>>(p =>
                    p.ContainsKey("correlationId") &&
                    p.ContainsKey("eventTimestamp") &&
                    !p.ContainsKey("isFailure")),
                It.IsAny<IDictionary<string, double>>()),
            Times.Once);
    }

    [Fact]
    public void TrackEventFailure_WithValidEvent_AddsFailureFlag()
    {
        // Arrange
        var telemetryEvent = new TestTelemetryEvent
        {
            CorrelationId = _fixture.Create<Guid>()
        };

        // Act
        _telemetryClient.TrackEventFailure(telemetryEvent);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackEvent(
                It.IsAny<string>(),
                It.Is<IDictionary<string, string>>(p =>
                    p.ContainsKey("isFailure") &&
                    p["isFailure"] == "true"),
                It.IsAny<IDictionary<string, double>>()),
            Times.Once);
    }

    [Fact]
    public void TrackEvent_WithNullEvent_DoesNothing()
    {
        // Act
        _telemetryClient.TrackEvent(null!);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackEvent(
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<IDictionary<string, double>>()),
            Times.Never);
    }

    [Fact]
    public void TrackException_WithValidEvent_CallsTrackException()
    {
        // Arrange
        var telemetryEvent = new TestTelemetryEvent
        {
            CorrelationId = _fixture.Create<Guid>(),
            Properties = new Dictionary<string, string>
            {
                { "exceptionMessage", "Test exception message" },
                { "exceptionStackTrace", "Test stack trace" }
            }
        };

        // Act
        _telemetryClient.TrackException(telemetryEvent);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackException(
                It.Is<Exception>(e => e.Message == "Test exception message"),
                It.Is<IDictionary<string, string>>(p =>
                    p.ContainsKey("exceptionMessage") &&
                    p.ContainsKey("exceptionStackTrace")),
                It.IsAny<IDictionary<string, double>>()),
            Times.Once);
    }

    [Fact]
    public void TrackException_WithNullEvent_DoesNothing()
    {
        // Act
        _telemetryClient.TrackException(null!);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackException(
                It.IsAny<Exception>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<IDictionary<string, double>>()),
            Times.Never);
    }

    [Fact]
    public void TrackException_WithMissingExceptionMessage_UsesDefaultMessage()
    {
        // Arrange
        var telemetryEvent = new TestTelemetryEvent();

        // Act
        _telemetryClient.TrackException(telemetryEvent);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackException(
                It.Is<Exception>(e => e.Message == "Exception occurred"),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<IDictionary<string, double>>()),
            Times.Once);
    }

    [Fact]
    public void TrackMetric_WithValidEvent_CallsTrackMetricForEachMetric()
    {
        // Arrange
        var telemetryEvent = new TestTelemetryEvent
        {
            Metrics = new Dictionary<string, double?>
            {
                { "metric1", 100.0 },
                { "metric2", 200.0 }
            }
        };

        // Act
        _telemetryClient.TrackMetric(telemetryEvent);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackMetric(
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<IDictionary<string, string>>()),
            Times.Exactly(2));
    }

    [Fact]
    public void TrackMetric_WithNullMetrics_DoesNothing()
    {
        // Arrange
        var telemetryEvent = new TestTelemetryEvent
        {
            Metrics = null
        };

        // Act
        _telemetryClient.TrackMetric(telemetryEvent);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackMetric(
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<IDictionary<string, string>>()),
            Times.Never);
    }

    [Fact]
    public void TrackMetric_FiltersNullMetricValues()
    {
        // Arrange
        var telemetryEvent = new TestTelemetryEvent
        {
            Metrics = new Dictionary<string, double?>
            {
                { "metric1", 100.0 },
                { "metric2", null },
                { "metric3", 300.0 }
            }
        };

        // Act
        _telemetryClient.TrackMetric(telemetryEvent);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackMetric(
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<IDictionary<string, string>>()),
            Times.Exactly(2));
    }

    [Fact]
    public void TrackPageView_WithValidPageName_CallsTrackPageView()
    {
        // Arrange
        var telemetryEvent = new TestTelemetryEvent
        {
            Properties = new Dictionary<string, string>
            {
                { "pageName", "HomePage" }
            }
        };

        // Act
        _telemetryClient.TrackPageView(telemetryEvent);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackPageView("HomePage"),
            Times.Once);
    }

    [Fact]
    public void TrackPageView_WithMissingPageName_DoesNotCallTrackPageView()
    {
        // Arrange
        var telemetryEvent = new TestTelemetryEvent();

        // Act
        _telemetryClient.TrackPageView(telemetryEvent);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackPageView(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void TrackTrace_WithValidEvent_CallsTrackTrace()
    {
        // Arrange
        var telemetryEvent = new TestTelemetryEvent
        {
            Properties = new Dictionary<string, string>
            {
                { "message", "Test trace message" },
                { "severityLevel", "Warning" }
            }
        };

        // Act
        _telemetryClient.TrackTrace(telemetryEvent);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackTrace(
                "Test trace message",
                SeverityLevel.Warning,
                It.IsAny<IDictionary<string, string>>()),
            Times.Once);
    }

    [Fact]
    public void TrackTrace_WithInvalidSeverityLevel_UsesInformationAsDefault()
    {
        // Arrange
        var telemetryEvent = new TestTelemetryEvent
        {
            Properties = new Dictionary<string, string>
            {
                { "message", "Test trace message" },
                { "severityLevel", "InvalidLevel" }
            }
        };

        // Act
        _telemetryClient.TrackTrace(telemetryEvent);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackTrace(
                It.IsAny<string>(),
                SeverityLevel.Information,
                It.IsAny<IDictionary<string, string>>()),
            Times.Once);
    }

    [Fact]
    public void TrackTrace_WithNullEvent_DoesNothing()
    {
        // Act
        _telemetryClient.TrackTrace(null!);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackTrace(
                It.IsAny<string>(),
                It.IsAny<SeverityLevel>(),
                It.IsAny<IDictionary<string, string>>()),
            Times.Never);
    }

    [Theory]
    [InlineData("TestTelemetry")]
    public void PrepareEventName_RemovesEventSuffix(string expected)
    {
        // Arrange
        var telemetryEvent = new TestTelemetryEvent();

        // Act
        _telemetryClient.TrackEvent(telemetryEvent);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackEvent(
                expected,
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<IDictionary<string, double>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("_TestProperty", "testProperty")]
    [InlineData("Test_Property", "testProperty")]
    public void CleanPropertyName_TransformsCorrectly(string input, string expected)
    {
        // Arrange
        var telemetryEvent = new TestTelemetryEvent
        {
            Properties = new Dictionary<string, string>
            {
                { input, "value" }
            }
        };

        // Act
        _telemetryClient.TrackEvent(telemetryEvent);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackEvent(
                It.IsAny<string>(),
                It.Is<IDictionary<string, string>>(p =>
                    string.IsNullOrEmpty(expected) || p.ContainsKey(expected)),
                It.IsAny<IDictionary<string, double>>()),
            Times.Once);
    }

    [Fact]
    public void ToLowerFirstChar_WithUpperCaseFirstChar_ConvertsToLower()
    {
        // Act
        var result = TelemetryClient.ToLowerFirstChar("TestString");

        // Assert
        Assert.Equal("testString", result);
    }

    [Fact]
    public void ToLowerFirstChar_WithLowerCaseFirstChar_RemainsUnchanged()
    {
        // Act
        var result = TelemetryClient.ToLowerFirstChar("testString");

        // Assert
        Assert.Equal("testString", result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    public void ToLowerFirstChar_WithNullOrEmpty_ReturnsInput(string input, string expected)
    {
        // Act
        var result = TelemetryClient.ToLowerFirstChar(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TrackEvent_WithEmptyCorrelationId_DoesNotAddCorrelationId()
    {
        // Arrange
        var telemetryEvent = new TestTelemetryEvent
        {
            CorrelationId = Guid.Empty
        };

        // Act
        _telemetryClient.TrackEvent(telemetryEvent);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackEvent(
                It.IsAny<string>(),
                It.Is<IDictionary<string, string>>(p => !p.ContainsKey("correlationId")),
                It.IsAny<IDictionary<string, double>>()),
            Times.Once);
    }

    [Fact]
    public void TrackEvent_WithMinValueTimestamp_DoesNotAddTimestamp()
    {
        // Arrange
        var telemetryEvent = new TestTelemetryEvent
        {
            EventTimestamp = DateTime.MinValue
        };

        // Act
        _telemetryClient.TrackEvent(telemetryEvent);

        // Assert
        _mockAppInsightsTelemetryClient.Verify(
            x => x.TrackEvent(
                It.IsAny<string>(),
                It.Is<IDictionary<string, string>>(p => !p.ContainsKey("eventTimestamp")),
                It.IsAny<IDictionary<string, double>>()),
            Times.Once);
    }

    // Test helper class
    private class TestTelemetryEvent : BaseTelemetryEvent
    {
        public Dictionary<string, string>? Properties { get; set; }
        public Dictionary<string, double?>? Metrics { get; set; }

        public override (IDictionary<string, string> Properties, IDictionary<string, double?> Metrics) ToTelemetryEventProps()
        {
            return (
                Properties ?? new Dictionary<string, string>(),
                Metrics ?? new Dictionary<string, double?>()
            );
        }
    }
}