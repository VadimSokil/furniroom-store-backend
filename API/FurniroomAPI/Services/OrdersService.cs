using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Orders;
using FurniroomAPI.Models.Response;
using System.Text;
using System.Text.Json;

namespace FurniroomAPI.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _endpointURL;
        public string currentDateTime = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") + " UTC";

        public OrdersService(HttpClient httpClient, Dictionary<string, string> endpointURL)
        {
            _httpClient = httpClient;
            _endpointURL = endpointURL;
        }

        public async Task<ServiceResponseModel> GetAccountOrdersAsync(int accountId)
        {
            return await GetInformationAsync("GetAccountOrders", accountId);
        }

        public async Task<ServiceResponseModel> AddOrderAsync(OrderModel order)
        {
            return await PostInformationAsync("AddOrder", order);
        }

        public async Task<ServiceResponseModel> AddQuestionAsync(QuestionModel question)
        {
            return await PostInformationAsync("AddQuestion", question);
        }

        private async Task<ServiceResponseModel> GetInformationAsync(string endpointKey, int parameter)
        {
            try
            {
                var endpoint = $"{_endpointURL[endpointKey]}?accountId={Uri.EscapeDataString(parameter.ToString())}";

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

        private async Task<ServiceResponseModel> PostInformationAsync<T>(string endpointKey, T model)
        {
            try
            {
                var endpoint = _endpointURL[endpointKey];

                var jsonContent = JsonSerializer.Serialize(model);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
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
