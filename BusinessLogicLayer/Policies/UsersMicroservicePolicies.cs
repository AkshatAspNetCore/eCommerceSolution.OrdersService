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
    private readonly IPollyPolicies _pollyPolicies;

    public UsersMicroservicePolicies(IPollyPolicies pollyPolicies)
    {
        _pollyPolicies = pollyPolicies;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy() 
    {
        var retryPolicy = _pollyPolicies.GetRetryPolicy(5);
        var circuitBreakerPolicy = _pollyPolicies.GetCircuitBreakerPolicy(3, TimeSpan.FromMinutes(2));
        var timeoutPolicy = _pollyPolicies.GetTimeoutPolicy(TimeSpan.FromSeconds(5));

        AsyncPolicyWrap<HttpResponseMessage> wrappedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
        return wrappedPolicy;
    }
}
