using MySql.Data.MySqlClient;
using OrdersService.Interfaces;
using OrdersService.Models.Orders;
using OrdersService.Models.Response;


namespace OrdersService.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, string> _requests;

        public OrdersService(string connectionString, Dictionary<string, string> requests)
        {
            _connectionString = connectionString;
            _requests = requests;
        }

        public async Task<ServiceResponseModel> GetAccountOrdersAsync(int accountId)
        {
            return await ExecuteGetCommandAsync(
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
        }


        public async Task<ServiceResponseModel> AddOrderAsync(OrderModel order)
        {
            return await ExecuteAddCommandAsync(
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
        }

        public async Task<ServiceResponseModel> AddQuestionAsync(QuestionModel question)
        {
            return await ExecuteAddCommandAsync(
                uniqueCheckQuery: _requests["QuestionUniqueCheck"],
                addQuery: _requests["AddQuestion"],
                uniqueParameter: new KeyValuePair<string, object>("@QuestionId", question.QuestionId),
                parameters: new Dictionary<string, object>
                {
                    { "@QuestionId", question.QuestionId },
                    { "@QuestionDate", question.QuestionDate },
                    { "@UserName", question.UserName },
                    { "@PhoneNumber", question.PhoneNumber },
                    { "Email", question.Email },
                    { "@QuestionText", question.QuestionText }
                },
                name: "Question"
                );
        }

        private async Task<ServiceResponseModel> ExecuteGetCommandAsync<T>(string query, string parameterName, object parameterValue, Func<MySqlDataReader, T> readAction, string notFoundMessage, string successMessage)
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
                return CreateErrorResponse($"A database error occurred: {ex.Message}");
            }
            catch (Exception ex)
            {
                return CreateErrorResponse($"An unexpected error occurred: {ex.Message}");
            }
        }


        private async Task<ServiceResponseModel> ExecuteAddCommandAsync(string uniqueCheckQuery, string addQuery, KeyValuePair<string, object> uniqueParameter, Dictionary<string, object> parameters, string name)
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
                            return CreateErrorResponse($"This ID is already in use.");
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
