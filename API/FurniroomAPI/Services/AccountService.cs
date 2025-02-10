using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Account;
using FurniroomAPI.Models.Response;
using System.Text;
using System.Text.Json;

namespace FurniroomAPI.Services
{
    public class AccountService : IAccountService
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _endpointURL;
        public string currentDateTime = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") + " UTC";

        public AccountService(HttpClient httpClient, Dictionary<string, string> endpointURL)
        {
            _httpClient = httpClient;
            _endpointURL = endpointURL;
        }

        public async Task<ServiceResponseModel> GetAccountInformationAsync(int accountId)
        {
            return await GetInformationAsync("GetAccountInformation", accountId);
        }

        public async Task<ServiceResponseModel> ChangeNameAsync(ChangeNameModel changeName)
        {
            return await PutInformationAsync("ChangeName", changeName);
        }

        public async Task<ServiceResponseModel> ChangeEmailAsync(ChangeEmailModel changeEmail)
        {
            return await PutInformationAsync("ChangeEmail", changeEmail);
        }

        public async Task<ServiceResponseModel> ChangePasswordAsync(ChangePasswordModel changePassword)
        {
            return await PutInformationAsync("ChangePassword", changePassword);
        }

        public async Task<ServiceResponseModel> DeleteAccountAsync(int accountId)
        {
            return await DeleteInformationAsync("DeleteAccount", accountId);
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

        private async Task<ServiceResponseModel> PutInformationAsync<T>(string endpointKey, T model)
        {
            try
            {
                var endpoint = _endpointURL[endpointKey];

                var requestBody = JsonSerializer.Serialize(model);
                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);
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

        private async Task<ServiceResponseModel> DeleteInformationAsync(string endpointKey, int parameter)
        {
            try
            {
                var endpoint = $"{_endpointURL[endpointKey]}?accountId={Uri.EscapeDataString(parameter.ToString())}";

                var response = await _httpClient.DeleteAsync(endpoint);
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
