using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Account;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Response;
using MySql.Data.MySqlClient;

namespace FurniroomAPI.Services
{
    public class AccountService : IAccountService
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, string> _requests;
        private readonly ILoggingService _loggingService;
        private readonly DateTime _logDate;

        public AccountService(
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

        public async Task<ServiceResponseModel> GetAccountInformationAsync(
            int accountId,
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            try
            {
                await LogActionAsync("Get account info started", httpMethod, endpoint, queryParams, requestId);

                var result = await ExecuteGetCommandAsync(
                    query: _requests["GetAccountInformation"],
                    parameterName: "@AccountId",
                    parameterValue: accountId,
                    readAction: reader => new AccountInformationModel
                    {
                        AccountName = reader.GetString("AccountName"),
                        Email = reader.GetString("Email")
                    },
                    notFoundMessage: "Account not found.",
                    successMessage: "Account information successfully retrieved.",
                    httpMethod,
                    endpoint,
                    queryParams,
                    requestId
                );

                await LogActionAsync(result.Status ? "Get account info completed" : "Account not found",
                    httpMethod, endpoint, queryParams, requestId);

                return result;
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return CreateErrorResponse($"Error getting account info: {ex.Message}");
            }
        }

        public async Task<ServiceResponseModel> ChangeNameAsync(
            ChangeNameModel changeName,
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            try
            {
                await LogActionAsync("Change name started", httpMethod, endpoint, queryParams, requestId);

                var result = await ExecuteChangeCommandAsync(
                    checkOldValueQuery: _requests["CheckOldAccountName"],
                    checkNewValueQuery: _requests["CheckNewAccountName"],
                    updateQuery: _requests["ChangeAccountName"],
                    parameters: new Dictionary<string, object>
                    {
                        { "@OldName", changeName.OldName },
                        { "@NewName", changeName.NewName }
                    },
                    checkMessage: "Old name not found.",
                    existsMessage: "New name is already in use.",
                    successMessage: "Name successfully changed.",
                    httpMethod,
                    endpoint,
                    queryParams,
                    requestId
                );

                await LogActionAsync(result.Status ? "Name changed" : "Failed to change name",
                    httpMethod, endpoint, queryParams, requestId);

                return result;
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return CreateErrorResponse($"Error changing name: {ex.Message}");
            }
        }

        public async Task<ServiceResponseModel> ChangeEmailAsync(
            ChangeEmailModel changeEmail,
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            try
            {
                await LogActionAsync("Change email started", httpMethod, endpoint, queryParams, requestId);

                var result = await ExecuteChangeCommandAsync(
                    checkOldValueQuery: _requests["CheckOldEmail"],
                    checkNewValueQuery: _requests["CheckNewEmail"],
                    updateQuery: _requests["ChangeEmail"],
                    parameters: new Dictionary<string, object>
                    {
                        { "@OldEmail", changeEmail.OldEmail },
                        { "@NewEmail", changeEmail.NewEmail }
                    },
                    checkMessage: "Old email not found.",
                    existsMessage: "New email is already in use.",
                    successMessage: "Email successfully changed.",
                    httpMethod,
                    endpoint,
                    queryParams,
                    requestId
                );

                await LogActionAsync(result.Status ? "Email changed" : "Failed to change email",
                    httpMethod, endpoint, queryParams, requestId);

                return result;
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return CreateErrorResponse($"Error changing email: {ex.Message}");
            }
        }

        public async Task<ServiceResponseModel> ChangePasswordAsync(
            ChangePasswordModel changePassword,
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            try
            {
                await LogActionAsync("Change password started", httpMethod, endpoint, queryParams, requestId);

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand(_requests["ChangePassword"], connection))
                    {
                        command.Parameters.AddWithValue("@OldPasswordHash", changePassword.OldPasswordHash);
                        command.Parameters.AddWithValue("@NewPasswordHash", changePassword.NewPasswordHash);

                        int affectedRows = await command.ExecuteNonQueryAsync();
                        if (affectedRows > 0)
                        {
                            await LogActionAsync("Password changed", httpMethod, endpoint, queryParams, requestId);
                            return new ServiceResponseModel
                            {
                                Status = true,
                                Message = "Password successfully changed."
                            };
                        }
                        else
                        {
                            await LogActionAsync("Old password not found", httpMethod, endpoint, queryParams, requestId);
                            return CreateErrorResponse("Old password not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return CreateErrorResponse($"Error changing password: {ex.Message}");
            }
        }

        public async Task<ServiceResponseModel> DeleteAccountAsync(
            int accountId,
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            try
            {
                await LogActionAsync("Delete account started", httpMethod, endpoint, queryParams, requestId);

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var deleteOrdersCommand = new MySqlCommand(_requests["DeleteOrders"], connection))
                    {
                        deleteOrdersCommand.Parameters.AddWithValue("@AccountId", accountId);
                        await deleteOrdersCommand.ExecuteNonQueryAsync();
                    }

                    using (var deleteAccountCommand = new MySqlCommand(_requests["DeleteAccount"], connection))
                    {
                        deleteAccountCommand.Parameters.AddWithValue("@AccountId", accountId);

                        int affectedRows = await deleteAccountCommand.ExecuteNonQueryAsync();
                        if (affectedRows > 0)
                        {
                            await LogActionAsync("Account deleted", httpMethod, endpoint, queryParams, requestId);
                            return new ServiceResponseModel
                            {
                                Status = true,
                                Message = "Account and orders successfully deleted."
                            };
                        }
                        else
                        {
                            await LogActionAsync("Account not found", httpMethod, endpoint, queryParams, requestId);
                            return CreateErrorResponse("Account not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return CreateErrorResponse($"Error deleting account: {ex.Message}");
            }
        }

        private async Task<ServiceResponseModel> ExecuteGetCommandAsync<T>(
            string query,
            string parameterName,
            object parameterValue,
            Func<MySqlDataReader, T> readAction,
            string notFoundMessage,
            string successMessage,
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            if (parameterValue == null)
            {
                await LogActionAsync($"{parameterName} is null", httpMethod, endpoint, queryParams, requestId);
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
                            if (await reader.ReadAsync())
                            {
                                var resultData = readAction((MySqlDataReader)reader);
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

                await LogActionAsync(notFoundMessage, httpMethod, endpoint, queryParams, requestId);
                return CreateErrorResponse(notFoundMessage);
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                throw;
            }
        }

        private async Task<ServiceResponseModel> ExecuteChangeCommandAsync(
            string checkOldValueQuery,
            string checkNewValueQuery,
            string updateQuery,
            Dictionary<string, object> parameters,
            string checkMessage,
            string existsMessage,
            string successMessage,
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var checkOldValueCommand = new MySqlCommand(checkOldValueQuery, connection))
                    {
                        checkOldValueCommand.Parameters.AddWithValue("@OldName", parameters.ContainsKey("@OldName") ? parameters["@OldName"] : DBNull.Value);
                        checkOldValueCommand.Parameters.AddWithValue("@OldEmail", parameters.ContainsKey("@OldEmail") ? parameters["@OldEmail"] : DBNull.Value);

                        var oldValueExists = Convert.ToInt32(await checkOldValueCommand.ExecuteScalarAsync()) > 0;

                        if (!oldValueExists)
                        {
                            await LogActionAsync(checkMessage, httpMethod, endpoint, queryParams, requestId);
                            return CreateErrorResponse(checkMessage);
                        }
                    }

                    using (var checkNewValueCommand = new MySqlCommand(checkNewValueQuery, connection))
                    {
                        checkNewValueCommand.Parameters.AddWithValue("@NewName", parameters.ContainsKey("@NewName") ? parameters["@NewName"] : DBNull.Value);
                        checkNewValueCommand.Parameters.AddWithValue("@NewEmail", parameters.ContainsKey("@NewEmail") ? parameters["@NewEmail"] : DBNull.Value);

                        var newValueExists = Convert.ToInt32(await checkNewValueCommand.ExecuteScalarAsync()) > 0;

                        if (newValueExists &&
                            ((parameters.ContainsKey("@OldName") && parameters["@OldName"].ToString() != parameters["@NewName"].ToString()) ||
                             (parameters.ContainsKey("@OldEmail") && parameters["@OldEmail"].ToString() != parameters["@NewEmail"].ToString())))
                        {
                            await LogActionAsync(existsMessage, httpMethod, endpoint, queryParams, requestId);
                            return CreateErrorResponse(existsMessage);
                        }
                    }

                    using (var updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        foreach (var parameter in parameters)
                        {
                            updateCommand.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                        }

                        await updateCommand.ExecuteNonQueryAsync();
                    }

                    await LogActionAsync(successMessage, httpMethod, endpoint, queryParams, requestId);
                    return new ServiceResponseModel
                    {
                        Status = true,
                        Message = successMessage
                    };
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                throw;
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
                Message = message
            };
        }
    }
}