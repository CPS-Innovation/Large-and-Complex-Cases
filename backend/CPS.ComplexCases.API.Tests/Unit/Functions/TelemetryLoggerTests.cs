using System.Text;
using System.Text.Json;
using AutoFixture;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.Common.TelemetryEvents;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class TelemetryLoggerTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<ITelemetryClient> _mockTelemetryClient;
    private readonly TelemetryLogger _telemetryLogger;

    public TelemetryLoggerTests()
    {
        _mockTelemetryClient = new Mock<ITelemetryClient>();
        _telemetryLogger = new TelemetryLogger(_mockTelemetryClient.Object);
    }

    [Fact]
    public async Task Run_WithEventTelemetry_CallsTrackEvent()
    {
        // Arrange
        var correlationId = _fixture.Create<Guid>();
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

        var uiTelemetry = new UiTelemetry
        {
            TelemetryType = TelemetryType.Event,
            EventTimestamp = DateTime.UtcNow,
            Properties =
            [
                new Dictionary<string, object> { { "eventName", "TestEvent" } }
            ]
        };

        httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(uiTelemetry)));

        // Act
        var result = await _telemetryLogger.Run(httpRequest, functionContext);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockTelemetryClient.Verify(
            x => x.TrackEvent(It.Is<UiTelemetryEvent>(e =>
                e.CorrelationId == correlationId &&
                e.Properties.Count() == 1)),
            Times.Once);
    }

    [Fact]
    public async Task Run_WithExceptionTelemetry_CallsTrackException()
    {
        // Arrange
        var correlationId = _fixture.Create<Guid>();
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

        var uiTelemetry = new UiTelemetry
        {
            TelemetryType = TelemetryType.Exception,
            EventTimestamp = DateTime.UtcNow,
            Properties =
            [
                new Dictionary<string, object>
                {
                    { "exceptionMessage", "Test exception" },
                    { "exceptionStackTrace", "Test stack trace" }
                }
            ]
        };

        httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(uiTelemetry)));

        // Act
        var result = await _telemetryLogger.Run(httpRequest, functionContext);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockTelemetryClient.Verify(
            x => x.TrackException(It.Is<UiTelemetryEvent>(e =>
                e.CorrelationId == correlationId &&
                e.Properties.Count() == 1)),
            Times.Once);
    }

    [Fact]
    public async Task Run_WithMetricTelemetry_CallsTrackMetric()
    {
        // Arrange
        var correlationId = _fixture.Create<Guid>();
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

        var uiTelemetry = new UiTelemetry
        {
            TelemetryType = TelemetryType.Metric,
            EventTimestamp = DateTime.UtcNow,
            Properties =
            [
                new Dictionary<string, object>
                {
                    { "metricName", "TestMetric" },
                    { "metricValue", 42.0 }
                }
            ]
        };

        httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(uiTelemetry)));

        // Act
        var result = await _telemetryLogger.Run(httpRequest, functionContext);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockTelemetryClient.Verify(
            x => x.TrackMetric(It.Is<UiTelemetryEvent>(e =>
                e.CorrelationId == correlationId)),
            Times.Once);
    }

    [Fact]
    public async Task Run_WithPageViewTelemetry_CallsTrackPageView()
    {
        // Arrange
        var correlationId = _fixture.Create<Guid>();
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

        var uiTelemetry = new UiTelemetry
        {
            TelemetryType = TelemetryType.PageView,
            EventTimestamp = DateTime.UtcNow,
            Properties =
            [
                new Dictionary<string, object> { { "pageName", "HomePage" } }
            ]
        };

        httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(uiTelemetry)));

        // Act
        var result = await _telemetryLogger.Run(httpRequest, functionContext);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockTelemetryClient.Verify(
            x => x.TrackPageView(It.Is<UiTelemetryEvent>(e =>
                e.CorrelationId == correlationId)),
            Times.Once);
    }

    [Fact]
    public async Task Run_WithTraceTelemetry_CallsTrackTrace()
    {
        // Arrange
        var correlationId = _fixture.Create<Guid>();
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

        var uiTelemetry = new UiTelemetry
        {
            TelemetryType = TelemetryType.Trace,
            EventTimestamp = DateTime.UtcNow,
            Properties =
            [
                new Dictionary<string, object>
                {
                    { "message", "Test trace message" },
                    { "severityLevel", "Warning" }
                }
            ]
        };

        httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(uiTelemetry)));

        // Act
        var result = await _telemetryLogger.Run(httpRequest, functionContext);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockTelemetryClient.Verify(
            x => x.TrackTrace(It.Is<UiTelemetryEvent>(e =>
                e.CorrelationId == correlationId)),
            Times.Once);
    }

    [Fact]
    public async Task Run_WithNullBody_ReturnsBadRequest()
    {
        // Arrange
        var correlationId = _fixture.Create<Guid>();
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

        // Act
        var result = await _telemetryLogger.Run(httpRequest, functionContext);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is empty.", badRequestResult.Value);
        _mockTelemetryClient.Verify(
            x => x.TrackEvent(It.IsAny<UiTelemetryEvent>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WithInvalidJson_ReturnsBadRequest()
    {
        // Arrange
        var correlationId = _fixture.Create<Guid>();
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

        httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes("{ invalid json }"));

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(async () =>
            await _telemetryLogger.Run(httpRequest, functionContext));
    }

    [Fact]
    public async Task Run_WithUnsupportedTelemetryType_ReturnsBadRequest()
    {
        // Arrange
        var correlationId = _fixture.Create<Guid>();
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

        var uiTelemetry = new UiTelemetry
        {
            TelemetryType = (TelemetryType)999, // Invalid enum value
            EventTimestamp = DateTime.UtcNow
        };

        httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(uiTelemetry)));

        // Act
        var result = await _telemetryLogger.Run(httpRequest, functionContext);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Unsupported telemetry type.", badRequestResult.Value);
    }

    [Fact]
    public async Task Run_WithNullProperties_CreatesEmptyList()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

        var uiTelemetry = new UiTelemetry
        {
            TelemetryType = TelemetryType.Event,
            EventTimestamp = DateTime.UtcNow,
            Properties = null
        };

        httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(uiTelemetry)));

        // Act
        var result = await _telemetryLogger.Run(httpRequest, functionContext);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockTelemetryClient.Verify(
            x => x.TrackEvent(It.Is<UiTelemetryEvent>(e =>
                e.Properties != null &&
                e.Properties.Count() == 0)),
            Times.Once);
    }

    [Fact]
    public async Task Run_WithMultipleProperties_PassesAllProperties()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

        var uiTelemetry = new UiTelemetry
        {
            TelemetryType = TelemetryType.Event,
            EventTimestamp = DateTime.UtcNow,
            Properties =
            [
                new Dictionary<string, object> { { "key1", "value1" } },
                new Dictionary<string, object> { { "key2", "value2" } },
                new Dictionary<string, object> { { "key3", 123 } }
            ]
        };

        httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(uiTelemetry)));

        // Act
        var result = await _telemetryLogger.Run(httpRequest, functionContext);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockTelemetryClient.Verify(
            x => x.TrackEvent(It.Is<UiTelemetryEvent>(e =>
                e.Properties.Count() == 3)),
            Times.Once);
    }

    [Fact]
    public async Task Run_PreservesCorrelationId()
    {
        // Arrange
        var expectedCorrelationId = Guid.NewGuid();
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(expectedCorrelationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequest(expectedCorrelationId);

        var uiTelemetry = new UiTelemetry
        {
            TelemetryType = TelemetryType.Event,
            EventTimestamp = DateTime.UtcNow,
            Properties = []
        };

        httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(uiTelemetry)));

        // Act
        await _telemetryLogger.Run(httpRequest, functionContext);

        // Assert
        _mockTelemetryClient.Verify(
            x => x.TrackEvent(It.Is<UiTelemetryEvent>(e =>
                e.CorrelationId == expectedCorrelationId)),
            Times.Once);
    }

    [Fact]
    public async Task Run_PreservesEventTimestamp()
    {
        // Arrange
        var expectedTimestamp = new DateTime(2025, 11, 5, 10, 30, 0, DateTimeKind.Utc);
        var correlationId = Guid.NewGuid();
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

        var uiTelemetry = new UiTelemetry
        {
            TelemetryType = TelemetryType.Event,
            EventTimestamp = expectedTimestamp,
            Properties = []
        };

        httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(uiTelemetry)));

        // Act
        await _telemetryLogger.Run(httpRequest, functionContext);

        // Assert
        _mockTelemetryClient.Verify(
            x => x.TrackEvent(It.Is<UiTelemetryEvent>(e =>
                e.EventTimestamp == expectedTimestamp)),
            Times.Once);
    }

    [Theory]
    [InlineData(TelemetryType.Event)]
    [InlineData(TelemetryType.Exception)]
    [InlineData(TelemetryType.Metric)]
    [InlineData(TelemetryType.PageView)]
    [InlineData(TelemetryType.Trace)]
    public async Task Run_WithValidTelemetryTypes_ReturnsOk(TelemetryType telemetryType)
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

        var uiTelemetry = new UiTelemetry
        {
            TelemetryType = telemetryType,
            EventTimestamp = DateTime.UtcNow,
            Properties = []
        };

        httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(uiTelemetry)));

        // Act
        var result = await _telemetryLogger.Run(httpRequest, functionContext);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Run_WithCaseInsensitiveJson_DeserializesCorrectly()
    {
        // Arrange
        var correlationId = _fixture.Create<Guid>();
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(correlationId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var httpRequest = HttpRequestStubHelper.CreateHttpRequest(correlationId);

        var json = @"{
            ""telemetrytype"": 0,
            ""eventtimestamp"": ""2025-11-05T10:30:00Z"",
            ""properties"": []
        }";

        httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

        // Act
        var result = await _telemetryLogger.Run(httpRequest, functionContext);

        // Assert
        Assert.IsType<OkResult>(result);
    }
}
