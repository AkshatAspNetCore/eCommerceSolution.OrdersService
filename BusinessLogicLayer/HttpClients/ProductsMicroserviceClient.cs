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
            //Key: product: {productID}
            //Value: {ProductID: 123, ProductName: "Product A", Category: "Category A", UnitPrice: 10.99, Stock: 100}

            string cacheKey = $"product:{productID}";
            string? cachedProduct = await _distributedCache.GetStringAsync(cacheKey);

            if (cachedProduct != null) 
            {
                ProductDTO? productFromCache = JsonSerializer.Deserialize<ProductDTO>(cachedProduct);
                return productFromCache;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/products/search/product-id/{productID}");

            if (response.IsSuccessStatusCode)
            {
                ProductDTO? product = await response.Content.ReadFromJsonAsync<ProductDTO>();
                if (product == null) throw new ArgumentException("Product data is null.");

                //key: product:{productID}
                //value: {ProductID: 123, ProductName: "Product A", Category: "Category A", UnitPrice: 10.99, Stock: 100}

                string productJson = JsonSerializer.Serialize(product);

                DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(250), // Cache expires after 250 minutes
                    SlidingExpiration = TimeSpan.FromMinutes(100) // Cache entry will be renewed if accessed within 100 minutes
                };

                string cacheKeyToWrite = $"product:{productID}";

                await _distributedCache.SetStringAsync(cacheKeyToWrite, productJson, options);

                return product;
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) 
                {
                    ProductDTO? productFromFallbackPolicy = await response.Content.ReadFromJsonAsync<ProductDTO>();

                    if (productFromFallbackPolicy == null) throw new NotImplementedException("Fallback policy was not implemented.");

                    return productFromFallbackPolicy;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
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
                QuantityInStock: 0);
        }
    }
}
