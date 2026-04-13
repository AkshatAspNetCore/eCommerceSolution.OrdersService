using BusinessLogicLayer.DTO;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System.Net.Http.Json;

namespace BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;

    public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductDTO?> GetProductByProductID(Guid productID)
    {
        try
        {
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
