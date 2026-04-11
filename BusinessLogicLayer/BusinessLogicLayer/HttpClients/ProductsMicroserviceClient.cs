using BusinessLogicLayer.DTO;
using System.Net.Http.Json;

namespace BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;

    public ProductsMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductDTO?> GetProductByProductID(Guid productID)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/products/search/product-id/{productID}");

        if (response.IsSuccessStatusCode)
        {
            ProductDTO? product = await response.Content.ReadFromJsonAsync<ProductDTO>();
            if(product == null) throw new ArgumentException("Product data is null.");
            return product;
        }
        else
        {
            if(response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            else if(response.StatusCode == System.Net.HttpStatusCode.BadRequest) throw new HttpRequestException("Bad request.", null, System.Net.HttpStatusCode.BadRequest);
            else throw new HttpRequestException("An error occurred while fetching product data.", null, response.StatusCode);
        }
    }
}
