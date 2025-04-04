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

        public ConditionsService(string connectionString, Dictionary<string, string> requests, ILoggingService loggingService)
        {
            _connectionString = connectionString;
            _requests = requests;
            _loggingService = loggingService;
        }

        public async Task<ServiceResponseModel> GetDeliveryConditionsAsync(TransferLogModel transfer)
        {
            return await GetInformationAsync("GetDeliveryConditions", transfer);
        }

        public async Task<ServiceResponseModel> GetPaymentConditionsAsync(TransferLogModel transfer)
        {
            return await GetInformationAsync("GetPaymentConditions", transfer);
        }

        private async Task<ServiceResponseModel> GetInformationAsync(string requestKey, TransferLogModel transfer)
        {
            try
            {
                await LogActionAsync($"{requestKey} started", transfer);

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

                await LogActionAsync($"{requestKey} completed successfully", transfer);

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Data retrieved successfully",
                    Data = notes
                };
            }
            catch (MySqlException ex)
            {
                await LogErrorAsync(ex, transfer);
                return CreateErrorResponse($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, transfer);
                return CreateErrorResponse($"Unexpected error: {ex.Message}");
            }
        }

        private async Task LogActionAsync(string status, TransferLogModel transfer)
        {
            try
            {
                await _loggingService.AddLogAsync(new LogModel
                {
                    Date = DateTime.UtcNow,
                    HttpMethod = transfer.HttpMethod,
                    Endpoint = transfer.Endpoint,
                    QueryParams = transfer.QueryParams,
                    Status = status,
                    RequestId = transfer.RequestId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log action: {ex.Message}");
            }
        }

        private async Task LogErrorAsync(Exception ex, TransferLogModel transfer)
        {
            await LogActionAsync($"ERROR: {ex.GetType().Name} - {ex.Message}", transfer);
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