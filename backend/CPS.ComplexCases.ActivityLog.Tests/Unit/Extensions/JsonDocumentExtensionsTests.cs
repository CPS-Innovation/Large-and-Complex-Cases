using System.Text.Json;
using CPS.ComplexCases.ActivityLog.Extensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.ActivityLog.Tests.Unit.Extensions;

public class JsonDocumentExtensionsTests
{
    public class TestClass
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    [Fact]
    public void DeserializeJsonDocument_ReturnsObject_WhenValidJson()
    {
        var json = "{\"name\":\"test\",\"value\":42}";
        using var doc = JsonDocument.Parse(json);

        var result = doc.DeserializeJsonDocument<TestClass>();

        Assert.NotNull(result);
        Assert.Equal("test", result.Name);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void DeserializeJsonDocument_ReturnsDefaultAndLogs_WhenInvalidJson()
    {
        var loggerMock = new Mock<ILogger>();
        var json = "{\"name\":\"test\",\"value\":\"notAnInt\"}";
        using var doc = JsonDocument.Parse(json);

        var result = doc.DeserializeJsonDocument<TestClass>(loggerMock.Object);

        Assert.Null(result);
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to deserialize JsonDocument")),
                It.IsAny<JsonException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void DeserializeJsonDocument_ReturnsDefaultAndLogs_WhenNullDocument()
    {
        var loggerMock = new Mock<ILogger>();
        JsonDocument? doc = null;

        var result = JsonDocumentExtensions.DeserializeJsonDocument<TestClass>(doc, loggerMock.Object);

        Assert.Null(result);
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempted to deserialize a null JsonDocument")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void SerializeToJsonDocument_ReturnsJsonDocument_WhenValidObject()
    {
        var obj = new TestClass { Name = "activity", Value = 123 };

        using var doc = obj.SerializeToJsonDocument();

        Assert.Equal("activity", doc.RootElement.GetProperty("name").GetString());
        Assert.Equal(123, doc.RootElement.GetProperty("value").GetInt32());
    }

    [Fact]
    public void SerializeToJsonDocument_ReturnsEmptyJsonAndLogs_WhenSerializationFails()
    {
        var loggerMock = new Mock<ILogger>();

        var circular = new Circular();
        circular.Self = circular;

        using var doc = circular.SerializeToJsonDocument(loggerMock.Object);

        Assert.True(doc.RootElement.ValueKind == JsonValueKind.Object);
        Assert.Empty(doc.RootElement.EnumerateObject());

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to serialize object to JsonDocument")),
                It.IsAny<JsonException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class Circular
    {
        public Circular? Self { get; set; }
    }
}