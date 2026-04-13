using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net.Http.Json;

namespace BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;

    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserDTO?> GetUserByUserID(Guid userID)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userID}");

            if (response.IsSuccessStatusCode)
            {
                UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();
                if (user == null) throw new ArgumentException("User data is null.");
                return user;
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) throw new HttpRequestException("Bad request.", null, System.Net.HttpStatusCode.BadRequest);
                else
                {                
                    return new UserDTO(Username: "Unavailabe",
                        Email: "Unavailabe",
                        Gender: "Unavailable",
                        UserID: Guid.Empty);
                }
            }
        }
        catch (BrokenCircuitException ex) 
        {
            _logger.LogError(ex, $"Request failed because circuit " +
                $"breaker is in Open State. Returning dummy data.");

            return new UserDTO(Username: "Unavailabe (circuit breaker)",
                       Email: "Unavailabe (circuit breaker)",
                       Gender: "Unavailable (circuit breaker)",
                       UserID: Guid.Empty);
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, $"Timeout occured while fetching user data " +
                $"Returning dummy data.");

            return new UserDTO(Username: "Unavailabe (timeout)",
                       Email: "Unavailabe (timeout)",
                       Gender: "Unavailable (timeout)",
                       UserID: Guid.Empty);
        }
    }
}
