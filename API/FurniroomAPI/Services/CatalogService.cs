using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Response;
using System.Text.Json;

namespace FurniroomAPI.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _endpointURL;

        public CatalogService(HttpClient httpClient, Dictionary<string, string> endpointURL)
        {
            _httpClient = httpClient;
            _endpointURL = endpointURL;
        }

        public async Task<ServiceResponseModel> GetAllCategoriesAsync()
        {
            return await GetInformationAsync("GetAllCategories");
        }

        public async Task<ServiceResponseModel> GetAllSubcategoriesAsync()
        {
            return await GetInformationAsync("GetAllSubcategories");
        }

        public async Task<ServiceResponseModel> GetAllSetsAsync()
        {
            return await GetInformationAsync("GetAllSets");
        }

        public async Task<ServiceResponseModel> GetAllImagesAsync()
        {
            return await GetInformationAsync("GetAllImages");
        }

        public async Task<ServiceResponseModel> GetAllModulesAsync()
        {
            return await GetInformationAsync("GetAllModules");
        }

        private async Task<ServiceResponseModel> GetInformationAsync(string endpointKey)
        {
            try
            {
                var endpoint = _endpointURL[endpointKey];

                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();

                return DeserializeResponse(responseBody);
            }
            catch (HttpRequestException httpEx)
            {
                return CreateErrorResponse($"HTTP request error: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                return CreateErrorResponse($"Error parsing service response: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                return CreateErrorResponse($"An unexpected error occurred: {ex.Message}");
            }
        }

        private ServiceResponseModel DeserializeResponse(string responseBody)
        {
            try
            {
                var serviceResponse = JsonSerializer.Deserialize<ServiceResponseModel>(responseBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (serviceResponse?.Status == null)
                {
                    return CreateErrorResponse("The data transmitted by the service to the gateway is in an incorrect format");
                }

                return serviceResponse;
            }
            catch (Exception ex)
            {
                return CreateErrorResponse($"Error deserializing response: {ex.Message}");
            }
        }

        private ServiceResponseModel CreateErrorResponse(string message)
        {
            return new ServiceResponseModel
            {
                Status = false,
                Message = message
            };
        }

    }
}
