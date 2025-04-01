using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Log;
using MySql.Data.MySqlClient;

namespace FurniroomAPI.Services
{

    public class LoggingService : ILoggingService
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, string> _requests;

        public LoggingService(string connectionString, Dictionary<string, string> requests)
        {
            _connectionString = connectionString;
            _requests = requests;
        }

        public async Task AddLogAsync(LogModel log)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(_requests["AddLog"], connection))
                    {
                        command.Parameters.AddWithValue("@Date", log.Date);
                        command.Parameters.AddWithValue("@HttpMethod", log.HttpMethod);
                        command.Parameters.AddWithValue("@Endpoint", log.Endpoint);
                        command.Parameters.AddWithValue("@QueryParams", log.QueryParams);
                        command.Parameters.AddWithValue("@Status", log.Status);
                        command.Parameters.AddWithValue("@RequestId", log.RequestId);

                        await command.ExecuteNonQueryAsync();
                        Console.WriteLine("Log is recorded.");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Logging error - {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging error - {ex.Message}");
            }
        }
    }
}
