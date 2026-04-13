using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.Fallback;
using System.Text;
using System.Text.Json;

namespace BusinessLogicLayer.Policies;

public class ProductsMicroservicePolicies : IProductsMicroservicePolicies
{
    private readonly ILogger<ProductsMicroservicePolicies> _logger;

    public ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger) {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .FallbackAsync(async (context) => {
            _logger.LogWarning("Fallback triggered: The request failed, returning dummy data.");

                ProductDTO dummyProduct = new ProductDTO(
                    ProductID: Guid.Empty,
                    ProductName: "Unavailable(fallback)",
                    Category: "Unavailable(fallback)",
                    UnitPrice: 0,
                    Stock: 0
                );

                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) {
                Content = new StringContent(JsonSerializer.Serialize(dummyProduct), Encoding.UTF8, "application/json")
                };

                return response;
            });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy()
    {
        AsyncBulkheadPolicy<HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>(
            maxParallelization: 10, // Maximum number of concurrent requests allowed
            maxQueuingActions: 50, // Maximum number of requests that can be queued
            onBulkheadRejectedAsync: (context) =>
            {
                _logger.LogWarning("Bulkhead isolation triggered: Too many concurrent requests. Request rejected.");

                throw new BulkheadRejectedException("Bulkhead queue is full.");
            });

        return policy;
    }
}
