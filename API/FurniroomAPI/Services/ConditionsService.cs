using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Conditions;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Response;
using MySql.Data.MySqlClient;
using System.Data;

namespace FurniroomAPI.Services
{
    public class ConditionsService : IConditionsService
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, string> _requests;
        private readonly ILoggingService _loggingService;
        private readonly DateTime _logDate;

        public ConditionsService(
            string connectionString,
            Dictionary<string, string> requests,
            ILoggingService loggingService,
            Func<DateTime> logDate)
        {
            _connectionString = connectionString;
            _requests = requests;
            _loggingService = loggingService;
            _logDate = logDate();
        }

        public async Task<ServiceResponseModel> GetDeliveryConditionsAsync(
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            return await GetInformationAsync(
                "GetDeliveryConditions",
                httpMethod,
                endpoint,
                queryParams,
                requestId);
        }

        public async Task<ServiceResponseModel> GetPaymentConditionsAsync(
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            return await GetInformationAsync(
                "GetPaymentConditions",
                httpMethod,
                endpoint,
                queryParams,
                requestId);
        }

        private async Task<ServiceResponseModel> GetInformationAsync(
            string requestKey,
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            try
            {
                await LogActionAsync("Request started", httpMethod, endpoint, queryParams, requestId);

                var notes = new List<ConditionsModel>();

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand(_requests[requestKey], connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                notes.Add(new ConditionsModel
                                {
                                    NoteId = reader.GetInt32("NoteId"),
                                    Note = reader.GetString("Note")
                                });
                            }
                        }
                    }
                }

                await LogActionAsync("Request completed successfully", httpMethod, endpoint, queryParams, requestId);

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Data retrieved successfully",
                    Data = notes
                };
            }
            catch (MySqlException ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return CreateErrorResponse($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return CreateErrorResponse($"Unexpected error: {ex.Message}");
            }
        }

        private async Task LogActionAsync(
            string status,
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            try
            {
                await _loggingService.AddLogAsync(new LogModel
                {
                    Date = _logDate,
                    HttpMethod = httpMethod,
                    Endpoint = endpoint,
                    QueryParams = queryParams,
                    Status = status,
                    RequestId = requestId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log action: {ex.Message}");
            }
        }

        private async Task LogErrorAsync(
            Exception ex,
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            await LogActionAsync($"ERROR: {ex.GetType().Name} - {ex.Message}",
                httpMethod, endpoint, queryParams, requestId);
        }

        private ServiceResponseModel CreateErrorResponse(string message)
        {
            return new ServiceResponseModel
            {
                Status = false,
                Message = message,
                Data = null
            };
        }

    }
}
