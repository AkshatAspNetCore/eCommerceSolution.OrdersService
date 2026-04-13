using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using System;

namespace BusinessLogicLayer.Policies;

public class UsersMicroservicePolicies : IUsersMicroservicePolicies
{
    private readonly ILogger<UsersMicroservicePolicies> _logger;

    public UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        AsyncRetryPolicy<HttpResponseMessage> policy =
        Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
          .WaitAndRetryAsync(retryCount: 5,
          sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
          onRetry: (outcome, timespan, retryAttempt, context) =>
          {
              _logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
          });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy =
        Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
          .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3, // Number of retries
            durationOfBreak: TimeSpan.FromMinutes(2), // Delay before retrying
            onBreak: (outcome, timespan) =>
            {
                _logger.LogWarning($"Circuit breaker opened for {timespan.TotalMinutes} minutes due to: {outcome.Result?.StatusCode}.");
            },
            onReset: () =>
            {
                _logger.LogInformation("Circuit breaker closed. The subsequest requests will be allowed.");
            });
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(15));
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy() 
    {
        var retryPolicy = GetRetryPolicy();
        var circuitBreakerPolicy = GetCircuitBreakerPolicy();
        var timeoutPolicy = GetTimeoutPolicy();

        AsyncPolicyWrap<HttpResponseMessage> wrappedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
        return wrappedPolicy;
    }
}
