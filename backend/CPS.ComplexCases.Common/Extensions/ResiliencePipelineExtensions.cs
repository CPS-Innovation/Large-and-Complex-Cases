using System.Net;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace CPS.ComplexCases.Common.Extensions;

public static class ResiliencePipelineExtensions
{
	private const int RetryAttempts = 3;
	private const int FirstRetryDelaySeconds = 1;

	public static ResiliencePipelineBuilder<HttpResponseMessage> AddStandardHttpResilience(
		this ResiliencePipelineBuilder<HttpResponseMessage> pipeline,
		int concurrencyPermitLimit = 30)
	{
		pipeline.AddConcurrencyLimiter(permitLimit: concurrencyPermitLimit, queueLimit: int.MaxValue);

		// https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience
		pipeline.AddRetry(new HttpRetryStrategyOptions
		{
			MaxRetryAttempts = RetryAttempts,
			Delay = TimeSpan.FromSeconds(FirstRetryDelaySeconds),
			BackoffType = DelayBackoffType.Exponential,
			UseJitter = true,
			ShouldHandle = static args =>
			{
				if (args.Outcome.Result is null)
					return ValueTask.FromResult(false);

				var response = args.Outcome.Result;
				var isRetryableStatus = response.StatusCode >= HttpStatusCode.InternalServerError
					|| response.StatusCode == HttpStatusCode.TooManyRequests;
				var isRetryableMethod = response.RequestMessage?.Method != HttpMethod.Post
					&& response.RequestMessage?.Method != HttpMethod.Put;

				return ValueTask.FromResult(isRetryableStatus && isRetryableMethod);
			}
		});

		return pipeline;
	}
}
