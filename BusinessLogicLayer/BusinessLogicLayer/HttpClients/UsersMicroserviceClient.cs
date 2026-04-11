using BusinessLogicLayer.DTO;
using System.Net.Http.Json;

namespace BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;

    public UsersMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserDTO?> GetUserByUserID(Guid userID)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userID}");

        if (response.IsSuccessStatusCode)
        {
            UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();
            if(user == null) throw new ArgumentException("User data is null.");
            return user;
        }
        else
        {
            if(response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            else if(response.StatusCode == System.Net.HttpStatusCode.BadRequest) throw new HttpRequestException("Bad request.", null, System.Net.HttpStatusCode.BadRequest);
            else throw new HttpRequestException("An error occurred while fetching user data.", null, response.StatusCode);
        }
    }
}
