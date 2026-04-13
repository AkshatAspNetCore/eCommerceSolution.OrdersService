using BusinessLogicLayer.DTO;
using DnsClient.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System.Net.Http.Json;
using System.Text.Json;

namespace BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;

    public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    public async Task<ProductDTO?> GetProductByProductID(Guid productID)
    {
        try
        {
            //Key: product: 123
            //Value: {ProductID: 123, ProductName: "Product A", Category: "Category A", UnitPrice: 10.99, Stock: 100}

            string cacheKey = $"product:{productID}";
            string? cachedProduct = await _distributedCache.GetStringAsync(cacheKey);

            if (cachedProduct != null) 
            {
                ProductDTO? productFromCache = JsonSerializer.Deserialize<ProductDTO>(cachedProduct);
                return productFromCache;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/api/products/search/product-id/{productID}");

            if (response.IsSuccessStatusCode)
            {
                ProductDTO? product = await response.Content.ReadFromJsonAsync<ProductDTO>();
                if (product == null) throw new ArgumentException("Product data is null.");
                return product;
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) throw new HttpRequestException("Bad request.", null, System.Net.HttpStatusCode.BadRequest);
                else throw new HttpRequestException("An error occurred while fetching product data.", null, response.StatusCode);
            }
        }
        catch (BulkheadRejectedException ex) 
        {
            _logger.LogError(ex, $"Bulkhead isolation blocks the request since the request queue is full.");

            return new ProductDTO(
                ProductID: Guid.Empty,
                ProductName: "Unavailable (bulkhead isolation)",
                Category: "Unavailable (bulkhead isolation)",
                UnitPrice: 0,
                Stock: 0);
        }
    }
}
