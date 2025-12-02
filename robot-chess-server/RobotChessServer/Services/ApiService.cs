using System.Net.Http.Json;
using System.Text.Json;
using RobotChessServer.Utilities;

namespace RobotChessServer.Services
{
    /// <summary>
    /// Service to communicate with Robot Chess REST API
    /// </summary>
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public ApiService(string apiBaseUrl = "https://localhost:7096/")
        {
            // Configure HttpClientHandler to bypass SSL validation for localhost (development only)
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            
            _httpClient = new HttpClient(handler);
            _apiBaseUrl = apiBaseUrl;
            _httpClient.BaseAddress = new Uri(_apiBaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<bool> UpdateRobotStatusAsync(string robotId, bool isOnline, string status)
        {
            try
            {
                var payload = new
                {
                    isOnline = isOnline,
                    status = status,
                    lastOnlineAt = DateTime.UtcNow
                };

                var response = await _httpClient.PutAsJsonAsync($"/api/robots/{robotId}/status", payload);
                
                if (response.IsSuccessStatusCode)
                {
                    LoggerHelper.LogInfo($"Updated robot {robotId} status: {status}");
                    return true;
                }
                else
                {
                    LoggerHelper.LogWarning($"Failed to update robot status: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"Error updating robot status: {ex.Message}");
                return false;
            }
        }

        public async Task<RobotConfigResponse?> GetRobotConfigAsync(string robotId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/robots/{robotId}/config");
                
                if (response.IsSuccessStatusCode)
                {
                    var config = await response.Content.ReadFromJsonAsync<RobotConfigResponse>();
                    LoggerHelper.LogInfo($"Retrieved config for robot {robotId}");
                    return config;
                }
                else
                {
                    LoggerHelper.LogWarning($"Failed to get robot config: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"Error getting robot config: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateCommandStatusAsync(Guid commandId, string status, string? errorMessage = null)
        {
            try
            {
                var payload = new
                {
                    status = status,
                    errorMessage = errorMessage,
                    timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PutAsJsonAsync($"/api/commands/{commandId}/status", payload);
                
                if (response.IsSuccessStatusCode)
                {
                    LoggerHelper.LogInfo($"Updated command {commandId} status: {status}");
                    return true;
                }
                else
                {
                    LoggerHelper.LogWarning($"Failed to update command status: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"Error updating command status: {ex.Message}");
                return false;
            }
        }
    }
}
