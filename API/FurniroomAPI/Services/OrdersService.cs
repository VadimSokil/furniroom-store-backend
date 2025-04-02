using MySql.Data.MySqlClient;
using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Orders;
using FurniroomAPI.Models.Response;
using FurniroomAPI.Models.Log;

namespace FurniroomAPI.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, string> _requests;
        private readonly ILoggingService _loggingService;
        private readonly DateTime _logDate;

        public OrdersService(
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

        public async Task<ServiceResponseModel> GetAccountOrdersAsync(int accountId, string httpMethod, string endpoint, string queryParams, string requestId)
        {
            try
            {
                await LogActionAsync("Request started", httpMethod, endpoint, queryParams, requestId);

                var result = await ExecuteGetCommandAsync(
                    query: _requests["GetAccountOrders"],
                    parameterName: "@AccountId",
                    parameterValue: accountId,
                    readAction: reader => new AccountOrdersModel
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
                    },
                    notFoundMessage: "Orders not found.",
                    successMessage: "Orders information successfully retrieved."
                );

                await LogActionAsync(result.Status ? "Request completed" : "Request failed",
                    httpMethod, endpoint, queryParams, requestId);

                return result;
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return CreateErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ServiceResponseModel> AddOrderAsync(OrderModel order, string httpMethod, string endpoint, string queryParams, string requestId)
        {
            try
            {
                await LogActionAsync("Add order started", httpMethod, endpoint, queryParams, requestId);

                var result = await ExecuteAddCommandAsync(
                    uniqueCheckQuery: _requests["OrderUniqueCheck"],
                    addQuery: _requests["AddOrder"],
                    uniqueParameter: new KeyValuePair<string, object>("@OrderId", order.OrderId),
                    parameters: new Dictionary<string, object>
                    {
                        { "@OrderId", order.OrderId },
                        { "@OrderDate", order.OrderDate },
                        { "@AccountId", order.AccountId },
                        { "@PhoneNumber", order.PhoneNumber },
                        { "@Country", order.Country },
                        { "@Region", order.Region },
                        { "@District", order.District },
                        { "@City", order.City },
                        { "@Village", order.Village },
                        { "@Street", order.Street },
                        { "@HouseNumber", order.HouseNumber },
                        { "@ApartmentNumber", order.ApartmentNumber },
                        { "@OrderText", order.OrderText },
                        { "@DeliveryType", order.DeliveryType }
                    },
                    name: "Order"
                );

                await LogActionAsync(result.Status ? "Order added" : "Add order failed",
                    httpMethod, endpoint, queryParams, requestId);

                return result;
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return CreateErrorResponse($"Error adding order: {ex.Message}");
            }
        }

        public async Task<ServiceResponseModel> AddQuestionAsync(QuestionModel question, string httpMethod, string endpoint, string queryParams, string requestId)
        {
            try
            {
                await LogActionAsync("Add question started", httpMethod, endpoint, queryParams, requestId);

                var result = await ExecuteAddCommandAsync(
                    uniqueCheckQuery: _requests["QuestionUniqueCheck"],
                    addQuery: _requests["AddQuestion"],
                    uniqueParameter: new KeyValuePair<string, object>("@QuestionId", question.QuestionId),
                    parameters: new Dictionary<string, object>
                    {
                        { "@QuestionId", question.QuestionId },
                        { "@QuestionDate", question.QuestionDate },
                        { "@UserName", question.UserName },
                        { "@PhoneNumber", question.PhoneNumber },
                        { "@Email", question.Email },
                        { "@QuestionText", question.QuestionText }
                    },
                    name: "Question"
                );

                await LogActionAsync(result.Status ? "Question added" : "Add question failed",
                    httpMethod, endpoint, queryParams, requestId);

                return result;
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return CreateErrorResponse($"Error adding question: {ex.Message}");
            }
        }

        private async Task<ServiceResponseModel> ExecuteGetCommandAsync<T>(
            string query,
            string parameterName,
            object parameterValue,
            Func<MySqlDataReader, T> readAction,
            string notFoundMessage,
            string successMessage)
        {
            if (parameterValue == null)
            {
                return CreateErrorResponse($"{parameterName} cannot be null.");
            }

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(parameterName, parameterValue);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var resultData = new List<T>();

                            while (await reader.ReadAsync())
                            {
                                resultData.Add(readAction((MySqlDataReader)reader));
                            }

                            if (resultData.Count > 0)
                            {
                                return new ServiceResponseModel
                                {
                                    Status = true,
                                    Message = successMessage,
                                    Data = resultData
                                };
                            }
                        }
                    }
                }

                return CreateErrorResponse(notFoundMessage);
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Operation failed: {ex.Message}");
            }
        }

        private async Task<ServiceResponseModel> ExecuteAddCommandAsync(
            string uniqueCheckQuery,
            string addQuery,
            KeyValuePair<string, object> uniqueParameter,
            Dictionary<string, object> parameters,
            string name)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var checkCommand = new MySqlCommand(uniqueCheckQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue(uniqueParameter.Key, uniqueParameter.Value);

                        var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

                        if (exists)
                        {
                            return CreateErrorResponse($"{name} with this ID already exists");
                        }
                    }

                    using (var command = new MySqlCommand(addQuery, connection))
                    {
                        foreach (var parameter in parameters)
                        {
                            command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                        }

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = $"{name} successfully added."
                };
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Operation failed: {ex.Message}");
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
                Console.WriteLine($"Logging failed: {ex.Message}");
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