using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Account;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Response;
using MySql.Data.MySqlClient;
using System.Data;

namespace FurniroomAPI.Services
{
    public class AccountService : IAccountService
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, string> _requests;
        private readonly ILoggingService _loggingService;

        public AccountService(string connectionString, Dictionary<string, string> requests, ILoggingService loggingService)
        {
            _connectionString = connectionString;
            _requests = requests;
            _loggingService = loggingService;
        }

        public async Task<ServiceResponseModel> GetAccountInformationAsync(int accountId, TransferLogModel transfer)
        {
            return await ExecuteDatabaseOperation(async (connection) =>
            {
                using (var command = new MySqlCommand(_requests["GetAccountInformation"], connection))
                {
                    command.Parameters.AddWithValue("@AccountId", accountId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ServiceResponseModel
                            {
                                Status = true,
                                Message = "Account information successfully retrieved.",
                                Data = new AccountInformationModel
                                {
                                    AccountName = reader.GetString("AccountName"),
                                    Email = reader.GetString("Email")
                                }
                            };
                        }
                    }
                }
                return CreateErrorResponse("Account not found.");
            }, "Get account information", transfer);
        }

        public async Task<ServiceResponseModel> ChangeNameAsync(ChangeNameModel changeName, TransferLogModel transfer)
        {
            return await ExecuteDatabaseOperation(async (connection) =>
            {
                using (var checkCommand = new MySqlCommand(_requests["CheckOldAccountName"], connection))
                {
                    checkCommand.Parameters.AddWithValue("@OldName", changeName.OldName);
                    checkCommand.Parameters.AddWithValue("@AccountId", changeName.AccountId);
                    if (Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) == 0)
                    {
                        return CreateErrorResponse("Old name does not match the current account.");
                    }
                }

                using (var checkCommand = new MySqlCommand(_requests["CheckNewAccountName"], connection))
                {
                    checkCommand.Parameters.AddWithValue("@NewName", changeName.NewName);
                    if (Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0)
                    {
                        return CreateErrorResponse("New name is already in use.");
                    }
                }

                using (var command = new MySqlCommand(_requests["ChangeAccountName"], connection))
                {
                    command.Parameters.AddWithValue("@AccountId", changeName.AccountId);
                    command.Parameters.AddWithValue("@OldName", changeName.OldName);
                    command.Parameters.AddWithValue("@NewName", changeName.NewName);
                    await command.ExecuteNonQueryAsync();
                }

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Name successfully changed."
                };
            }, "Change account name", transfer);
        }

        public async Task<ServiceResponseModel> ChangeEmailAsync(ChangeEmailModel changeEmail, TransferLogModel transfer)
        {
            return await ExecuteDatabaseOperation(async (connection) =>
            {
                using (var checkCommand = new MySqlCommand(_requests["CheckOldEmail"], connection))
                {
                    checkCommand.Parameters.AddWithValue("@OldEmail", changeEmail.OldEmail);
                    checkCommand.Parameters.AddWithValue("@AccountId", changeEmail.AccountId);
                    if (Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) == 0)
                    {
                        return CreateErrorResponse("Old email does not match the current account.");
                    }
                }

                using (var checkCommand = new MySqlCommand(_requests["CheckNewEmail"], connection))
                {
                    checkCommand.Parameters.AddWithValue("@NewEmail", changeEmail.NewEmail);
                    if (Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0)
                    {
                        return CreateErrorResponse("New email is already in use.");
                    }
                }

                using (var command = new MySqlCommand(_requests["ChangeEmail"], connection))
                {
                    command.Parameters.AddWithValue("@AccountId", changeEmail.AccountId);
                    command.Parameters.AddWithValue("@OldEmail", changeEmail.OldEmail);
                    command.Parameters.AddWithValue("@NewEmail", changeEmail.NewEmail);
                    await command.ExecuteNonQueryAsync();
                }

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Email successfully changed."
                };
            }, "Change account email", transfer);
        }

        public async Task<ServiceResponseModel> ChangePasswordAsync(ChangePasswordModel changePassword, TransferLogModel transfer)
        {
            return await ExecuteDatabaseOperation(async (connection) =>
            {
                using (var command = new MySqlCommand(_requests["ChangePassword"], connection))
                {
                    command.Parameters.AddWithValue("@AccountId", changePassword.AccountId);
                    command.Parameters.AddWithValue("@OldPasswordHash", changePassword.OldPasswordHash);
                    command.Parameters.AddWithValue("@NewPasswordHash", changePassword.NewPasswordHash);

                    int affectedRows = await command.ExecuteNonQueryAsync();
                    if (affectedRows == 0)
                    {
                        return CreateErrorResponse("Old password does not match the current account.");
                    }
                }

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Password successfully changed."
                };
            }, "Change account password", transfer);
        }

        public async Task<ServiceResponseModel> DeleteAccountAsync(int accountId, TransferLogModel transfer)
        {
            return await ExecuteDatabaseOperation(async (connection) =>
            {
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        using (var deleteOrdersCommand = new MySqlCommand(_requests["DeleteOrders"], connection, transaction))
                        {
                            deleteOrdersCommand.Parameters.AddWithValue("@AccountId", accountId);
                            await deleteOrdersCommand.ExecuteNonQueryAsync();
                        }

                        using (var deleteAccountCommand = new MySqlCommand(_requests["DeleteAccount"], connection, transaction))
                        {
                            deleteAccountCommand.Parameters.AddWithValue("@AccountId", accountId);
                            int affectedRows = await deleteAccountCommand.ExecuteNonQueryAsync();

                            if (affectedRows == 0)
                            {
                                await transaction.RollbackAsync();
                                return CreateErrorResponse("Account not found.");
                            }
                        }

                        await transaction.CommitAsync();
                        return new ServiceResponseModel
                        {
                            Status = true,
                            Message = "Account and orders successfully deleted."
                        };
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }, "Delete account", transfer);
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