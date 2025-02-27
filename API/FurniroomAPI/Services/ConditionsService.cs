using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Conditions;
using FurniroomAPI.Models.Response;
using MySql.Data.MySqlClient;
using System.Data;

namespace FurniroomAPI.Services
{
    public class ConditionsService : IConditionsService
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, string> _requests;

        public ConditionsService(string connectionString, Dictionary<string, string> requests)
        {
            _connectionString = connectionString;
            _requests = requests;
        }

        public async Task<ServiceResponseModel> GetDeliveryConditionsAsync()
        {
            return await GetInformationAsync("GetDeliveryConditions");
        }

        public async Task<ServiceResponseModel> GetPaymentConditionsAsync()
        {
            return await GetInformationAsync("GetPaymentConditions");
        }

        private async Task<ServiceResponseModel> GetInformationAsync(string requestKey)
        {
            var notes = new List<ConditionsModel>();

            try
            {
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

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Data retrieved successfully.",
                    Data = notes
                };
            }
            catch (MySqlException ex)
            {
                return CreateErrorResponse($"A database error occurred: {ex.Message}");
            }
            catch (Exception ex)
            {
                return CreateErrorResponse($"An unexpected error occurred: {ex.Message}");
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
