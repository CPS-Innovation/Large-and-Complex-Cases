using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.ActivityLog.Models.Responses;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.Data.Dtos;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions
{
    public class GetActivityLogsTestsTest
    {
        private readonly Fixture _fixture;
        private readonly Mock<ILogger<GetActivityLogs>> _loggerMock;
        private readonly Mock<IActivityLogService> _activityLogServiceMock;
        private readonly GetActivityLogs _function;
        private readonly Guid _testCorrelationId;
        private readonly string _testUsername;
        private readonly string _testCmsAuthValues;
        private readonly string _testBearerToken;

        public GetActivityLogsTestsTest()
        {
            _fixture = new Fixture();
            _loggerMock = new Mock<ILogger<GetActivityLogs>>();
            _activityLogServiceMock = new Mock<IActivityLogService>();

            _fixture.Customize<Data.Entities.ActivityLog>(composer =>
                composer.Without(x => x.Details));

            _function = new GetActivityLogs(
                _loggerMock.Object,
                _activityLogServiceMock.Object
            );

            _testCorrelationId = _fixture.Create<Guid>();
            _testUsername = _fixture.Create<string>();
            _testCmsAuthValues = _fixture.Create<string>();
            _testBearerToken = _fixture.Create<string>();
        }

        [Fact]
        public async Task Run_ValidQueryParameters_CallsServiceWithExpectedFilter()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-10);
            var toDate = DateTime.UtcNow;
            var username = _fixture.Create<string>();
            var actionType = _fixture.Create<string>();
            var resourceType = _fixture.Create<string>();
            var resourceId = _fixture.Create<string>();
            var skip = 5;
            var take = 20;

            var queryParams = new Dictionary<string, string>
            {
                [InputParameters.FromDate] = fromDate.ToString("O"),
                [InputParameters.ToDate] = toDate.ToString("O"),
                [InputParameters.UserId] = username,
                [InputParameters.ActionType] = actionType,
                [InputParameters.ResourceType] = resourceType,
                [InputParameters.ResourceId] = resourceId,
                [InputParameters.Skip] = skip.ToString(),
                [InputParameters.Take] = take.ToString()
            };

            var response = _fixture.Create<ActivityLogsResponse>();
            ActivityLogFilterDto? capturedFilter = null;

            _activityLogServiceMock
                .Setup(s => s.GetActivityLogsAsync(It.IsAny<ActivityLogFilterDto>()))
                .Callback<ActivityLogFilterDto>(f => capturedFilter = f)
                .ReturnsAsync(response);

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);

            Assert.NotNull(capturedFilter);
            Assert.True(capturedFilter!.FromDate.HasValue);
            Assert.True(capturedFilter.ToDate.HasValue);
            Assert.Equal(username, capturedFilter.Username, ignoreCase: true);
            Assert.Equal(actionType, capturedFilter.ActionType, ignoreCase: true);
            Assert.Equal(resourceType, capturedFilter.ResourceType, ignoreCase: true);
            Assert.Equal(resourceId, capturedFilter.ResourceId, ignoreCase: true);
            Assert.Equal(skip, capturedFilter.Skip);
            Assert.Equal(take, capturedFilter.Take);
        }

        [Fact]
        public async Task Run_MissingQueryParameters_UsesDefaults()
        {
            // Arrange
            var queryParams = new Dictionary<string, string>();
            var response = _fixture.Create<ActivityLogsResponse>();
            ActivityLogFilterDto? capturedFilter = null;

            _activityLogServiceMock
                .Setup(s => s.GetActivityLogsAsync(It.IsAny<ActivityLogFilterDto>()))
                .Callback<ActivityLogFilterDto>(f => capturedFilter = f)
                .ReturnsAsync(response);

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);

            Assert.NotNull(capturedFilter);
            Assert.Null(capturedFilter!.FromDate);
            Assert.Null(capturedFilter.ToDate);
            Assert.Null(capturedFilter.Username);
            Assert.Null(capturedFilter.ActionType);
            Assert.Null(capturedFilter.ResourceType);
            Assert.Null(capturedFilter.ResourceId);
            Assert.Equal(0, capturedFilter.Skip);
            Assert.Equal(100, capturedFilter.Take);
        }

        [Fact]
        public async Task Run_ReturnsOkObjectResult_WithServiceResponse()
        {
            // Arrange
            var queryParams = new Dictionary<string, string>();
            var response = _fixture.Create<ActivityLogsResponse>();

            _activityLogServiceMock
                .Setup(s => s.GetActivityLogsAsync(It.IsAny<ActivityLogFilterDto>()))
                .ReturnsAsync(response);

            var request = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);
            var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

            // Act
            var result = await _function.Run(request, functionContext);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }
    }
}