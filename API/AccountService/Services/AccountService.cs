﻿using AccountService.Interfaces;
using AccountService.Models.Account;
using AccountService.Models.Response;
using MySql.Data.MySqlClient;

namespace AccountService.Services
{
    public class AccountService : IAccountService
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, string> _requests;

        public AccountService(string connectionString, Dictionary<string, string> requests)
        {
            _connectionString = connectionString;
            _requests = requests;
        }

        public async Task<ServiceResponseModel> GetAccountInformationAsync(int accountId)
        {
            return await ExecuteGetCommandAsync(
                query: _requests["GetAccountInformation"],
                parameterName: "@AccountId",
                parameterValue: accountId,
                readAction: reader => new AccountInformationModel
                {
                    AccountName = reader.GetString("AccountName"),
                    Email = reader.GetString("Email")
                },
                notFoundMessage: "Account not found.",
                successMessage: "Account information successfully retrieved."
            );
        }

        public async Task<ServiceResponseModel> ChangeNameAsync(ChangeNameModel changeName)
        {
            return await ExecuteChangeCommandAsync(
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
                successMessage: "Name successfully changed."
            );
        }


        public async Task<ServiceResponseModel> ChangeEmailAsync(ChangeEmailModel changeEmail)
        {
            return await ExecuteChangeCommandAsync(
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
                successMessage: "Email successfully changed."
            );
        }

        public async Task<ServiceResponseModel> ChangePasswordAsync(ChangePasswordModel changePassword)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand(_requests["ChangePassword"], connection))
                    {
                        command.Parameters.AddWithValue("@OldPasswordHash", changePassword.OldPasswordHash);
                        command.Parameters.AddWithValue("@NewPasswordHash", changePassword.NewPasswordHash);

                        int affectedRows = await command.ExecuteNonQueryAsync();
                        if (affectedRows > 0)
                            return new ServiceResponseModel
                            {
                                Status = true,
                                Message = "Password successfully changed."
                            };
                        else
                            return CreateErrorResponse("Old password not found.");
                    }
                }
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

        public async Task<ServiceResponseModel> DeleteAccountAsync(int accountId)
        {
            try
            {
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
                            return new ServiceResponseModel
                            {
                                Status = true,
                                Message = "Account and orders successfully deleted."
                            };
                        }
                        else
                        {
                            return CreateErrorResponse("Account not found.");
                        }
                    }
                }
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

        private async Task<ServiceResponseModel> ExecuteChangeCommandAsync(string checkOldValueQuery, string checkNewValueQuery, string updateQuery, Dictionary<string, object> parameters, string checkMessage, string existsMessage, string successMessage)
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

                    return new ServiceResponseModel
                    {
                        Status = true,
                        Message = successMessage
                    };
                }
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
