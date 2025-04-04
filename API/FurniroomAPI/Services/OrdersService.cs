using MySql.Data.MySqlClient;
using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Orders;
using FurniroomAPI.Models.Response;
using FurniroomAPI.Models.Log;
using System.Data;

namespace FurniroomAPI.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, string> _requests;
        private readonly ILoggingService _loggingService;

        public OrdersService(string connectionString, Dictionary<string, string> requests, ILoggingService loggingService)
        {
            _connectionString = connectionString;
            _requests = requests;
            _loggingService = loggingService;
        }

        public async Task<ServiceResponseModel> GetAccountOrdersAsync(int accountId, TransferLogModel transfer)
        {
            return await ExecuteDatabaseOperation(async (connection) =>
            {
                var result = new List<AccountOrdersModel>();

                using (var command = new MySqlCommand(_requests["GetAccountOrders"], connection))
                {
                    command.Parameters.AddWithValue("@AccountId", accountId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new AccountOrdersModel
                            {
                                OrderId = reader.GetInt32("OrderId"),
                                OrderDate = reader.GetString("OrderDate"),
                                AccountId = reader.GetInt32("AccountId"),
                                PhoneNumber = reader.GetString("PhoneNumber"),
                                Country = reader.GetString("Country"),
                                Region = reader.GetString("Region"),
                                District = reader.GetString("District"),
                                City = reader.GetString("City"),
                                Village = reader.GetString("Village"),
                                Street = reader.GetString("Street"),
                                HouseNumber = reader.GetString("HouseNumber"),
                                ApartmentNumber = reader.GetString("ApartmentNumber"),
                                OrderText = reader.GetString("OrderText"),
                                DeliveryType = reader.GetString("DeliveryType"),
                                OrderStatus = reader.GetString("OrderStatus")
                            });
                        }
                    }
                }

                return result.Count > 0
                    ? new ServiceResponseModel
                    {
                        Status = true,
                        Message = "Orders information successfully retrieved.",
                        Data = result
                    }
                    : CreateErrorResponse("Orders not found.");
            }, "Get account orders", transfer);
        }

        public async Task<ServiceResponseModel> AddOrderAsync(OrderModel order, TransferLogModel transfer)
        {
            return await ExecuteDatabaseOperation(async (connection) =>
            {
                using (var checkCommand = new MySqlCommand(_requests["OrderUniqueCheck"], connection))
                {
                    checkCommand.Parameters.AddWithValue("@OrderId", order.OrderId);
                    if (Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0)
                    {
                        return CreateErrorResponse("Order with this ID already exists");
                    }
                }

                using (var command = new MySqlCommand(_requests["AddOrder"], connection))
                {
                    command.Parameters.AddWithValue("@OrderId", order.OrderId);
                    command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                    command.Parameters.AddWithValue("@AccountId", order.AccountId);
                    command.Parameters.AddWithValue("@PhoneNumber", order.PhoneNumber);
                    command.Parameters.AddWithValue("@Country", order.Country);
                    command.Parameters.AddWithValue("@Region", order.Region);
                    command.Parameters.AddWithValue("@District", order.District);
                    command.Parameters.AddWithValue("@City", order.City);
                    command.Parameters.AddWithValue("@Village", order.Village);
                    command.Parameters.AddWithValue("@Street", order.Street);
                    command.Parameters.AddWithValue("@HouseNumber", order.HouseNumber);
                    command.Parameters.AddWithValue("@ApartmentNumber", order.ApartmentNumber);
                    command.Parameters.AddWithValue("@OrderText", order.OrderText);
                    command.Parameters.AddWithValue("@DeliveryType", order.DeliveryType);

                    await command.ExecuteNonQueryAsync();
                }

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Order successfully added."
                };
            }, "Add order", transfer);
        }

        public async Task<ServiceResponseModel> AddQuestionAsync(QuestionModel question, TransferLogModel transfer)
        {
            return await ExecuteDatabaseOperation(async (connection) =>
            {
                using (var checkCommand = new MySqlCommand(_requests["QuestionUniqueCheck"], connection))
                {
                    checkCommand.Parameters.AddWithValue("@QuestionId", question.QuestionId);
                    if (Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0)
                    {
                        return CreateErrorResponse("Question with this ID already exists");
                    }
                }

                using (var command = new MySqlCommand(_requests["AddQuestion"], connection))
                {
                    command.Parameters.AddWithValue("@QuestionId", question.QuestionId);
                    command.Parameters.AddWithValue("@QuestionDate", question.QuestionDate);
                    command.Parameters.AddWithValue("@UserName", question.UserName);
                    command.Parameters.AddWithValue("@PhoneNumber", question.PhoneNumber);
                    command.Parameters.AddWithValue("@Email", question.Email);
                    command.Parameters.AddWithValue("@QuestionText", question.QuestionText);

                    await command.ExecuteNonQueryAsync();
                }

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Question successfully added."
                };
            }, "Add question", transfer);
        }

        private async Task<ServiceResponseModel> ExecuteDatabaseOperation(Func<MySqlConnection, Task<ServiceResponseModel>> operation, string operationName, TransferLogModel transfer)
        {
            try
            {
                await LogActionAsync($"{operationName} started", transfer);

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var result = await operation(connection);

                    await LogActionAsync(
                        result.Status
                            ? $"{operationName} completed"
                            : $"{operationName} failed: {result.Message}",
                        transfer);

                    return result;
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, transfer);
                return CreateErrorResponse($"Error during {operationName.ToLower()}: {ex.Message}");
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