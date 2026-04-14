using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net.Http.Json;
using System.Text.Json;

namespace BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;

    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    public async Task<UserDTO?> GetUserByUserID(Guid userID)
    {
        try
        {
            string cacheKeyToRead = $"user:{userID}";
            string? cachedUser = await _distributedCache.GetStringAsync(cacheKeyToRead);

            if (cachedUser != null)
            {
                //Deserialize the cached user
                UserDTO? userFromCache = JsonSerializer.Deserialize<UserDTO>(cachedUser);

                return userFromCache;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userID}");

            if (response.IsSuccessStatusCode)
            {
                UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();
                if (user == null) throw new ArgumentException("User data is null.");

                //Store the user data (retrieved from response) into cache
                string cacheKey = $"user:{userID}";
                string userJSON = JsonSerializer.Serialize(user);

                DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5), // Cache expires after 5 minutes
                    SlidingExpiration = TimeSpan.FromMinutes(3) // Cache entry will be renewed if accessed within 3 minutes
                };

                await _distributedCache.SetStringAsync(cacheKey,userJSON, options);

                return user;
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) 
                {
                    UserDTO? fallbackUser = await response.Content.ReadFromJsonAsync<UserDTO>();

                    if (fallbackUser == null) throw new NotImplementedException();

                    return fallbackUser;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
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
