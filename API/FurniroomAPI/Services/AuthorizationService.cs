using FurniroomAPI.Interfaces;
using FurniroomAPI.Models;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Response;
using MySql.Data.MySqlClient;
using System.Net;
using System.Net.Mail;

namespace FurniroomAPI.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly string _connectionString;
        private readonly string _serviceEmail;
        private readonly string _servicePassword;
        private readonly Dictionary<string, string> _requests;
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private readonly ILoggingService _loggingService;

        public AuthorizationService(
            string connectionString,
            string serviceEmail,
            string servicePassword,
            Dictionary<string, string> requests,
            ILoggingService loggingService)
        {
            _connectionString = connectionString;
            _serviceEmail = serviceEmail;
            _servicePassword = servicePassword;
            _requests = requests;
            _loggingService = loggingService;
        }

        public async Task<ServiceResponseModel> CheckEmailAsync(string email, string httpMethod, string endpoint, string queryParams, string requestId)
        {
            return await ExecuteDatabaseOperation(async (connection) =>
            {
                using (var command = new MySqlCommand(_requests["CheckEmail"], connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    var result = Convert.ToInt32(await command.ExecuteScalarAsync());

                    return new ServiceResponseModel
                    {
                        Status = true,
                        Message = result > 0
                            ? "This email address is already in use."
                            : "This email address is available."
                    };
                }
            }, "Check email", httpMethod, endpoint, queryParams, requestId);
        }

        public async Task<ServiceResponseModel> GenerateCodeAsync(string email, string httpMethod, string endpoint, string queryParams, string requestId)
        {
            try
            {
                await LogActionAsync("Generate code started", httpMethod, endpoint, queryParams, requestId);

                int verificationCode = Random.Shared.Next(1000, 9999);
                string messageBody = $"Hi, your verification code: {verificationCode}";

                await SendEmailAsync(email, messageBody, "Verification code");

                await LogActionAsync("Verification code sent", httpMethod, endpoint, queryParams, requestId);

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Verification code has been generated.",
                    Data = verificationCode
                };
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return CreateErrorResponse($"Error generating code: {ex.Message}");
            }
        }

        public async Task<ServiceResponseModel> ResetPasswordAsync(string email, string httpMethod, string endpoint, string queryParams, string requestId)
        {
            return await ExecuteDatabaseOperation(async (connection) =>
            {
                using (var checkCommand = new MySqlCommand(_requests["CheckEmail"], connection))
                {
                    checkCommand.Parameters.AddWithValue("@Email", email);
                    if (Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) == 0)
                    {
                        return CreateErrorResponse("Email address is not found.");
                    }
                }

                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                string newPassword = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());

                using (var updateCommand = new MySqlCommand(_requests["ResetPassword"], connection))
                {
                    updateCommand.Parameters.AddWithValue("@Email", email);
                    updateCommand.Parameters.AddWithValue("@PasswordHash", newPassword);
                    await updateCommand.ExecuteNonQueryAsync();
                }

                await SendEmailAsync(email, $"Hi, your new password: {newPassword}", "Reset Password");

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Password successfully reset.",
                    Data = newPassword
                };
            }, "Reset password", httpMethod, endpoint, queryParams, requestId);
        }

        public async Task<ServiceResponseModel> SignInAsync(SignInModel signIn, string httpMethod, string endpoint, string queryParams, string requestId)
        {
            return await ExecuteDatabaseOperation(async (connection) =>
            {
                using (var command = new MySqlCommand(_requests["SignIn"], connection))
                {
                    command.Parameters.AddWithValue("@Email", signIn.Email);
                    command.Parameters.AddWithValue("@PasswordHash", signIn.PasswordHash);

                    var result = await command.ExecuteScalarAsync();
                    if (result == null)
                    {
                        return CreateErrorResponse("Incorrect email address or password.");
                    }

                    return new ServiceResponseModel
                    {
                        Status = true,
                        Message = "Authentication successful.",
                        Data = result
                    };
                }
            }, "Sign in", httpMethod, endpoint, queryParams, requestId);
        }

        public async Task<ServiceResponseModel> SignUpAsync(SignUpModel signUp, string httpMethod, string endpoint, string queryParams, string requestId)
        {
            return await ExecuteDatabaseOperation(async (connection) =>
            {
                using (var checkIdCommand = new MySqlCommand(_requests["CheckAccountId"], connection))
                {
                    checkIdCommand.Parameters.AddWithValue("@AccountId", signUp.AccountId);
                    if (Convert.ToInt32(await checkIdCommand.ExecuteScalarAsync()) > 0)
                    {
                        return CreateErrorResponse("This Account ID is already in use.");
                    }
                }

                using (var checkEmailCommand = new MySqlCommand(_requests["CheckEmail"], connection))
                {
                    checkEmailCommand.Parameters.AddWithValue("@Email", signUp.Email);
                    if (Convert.ToInt32(await checkEmailCommand.ExecuteScalarAsync()) > 0)
                    {
                        return CreateErrorResponse("This Email is already in use.");
                    }
                }

                using (var checkNameCommand = new MySqlCommand(_requests["CheckAccountName"], connection))
                {
                    checkNameCommand.Parameters.AddWithValue("@AccountName", signUp.AccountName);
                    if (Convert.ToInt32(await checkNameCommand.ExecuteScalarAsync()) > 0)
                    {
                        return CreateErrorResponse("This Account name is already in use.");
                    }
                }

                using (var command = new MySqlCommand(_requests["SignUp"], connection))
                {
                    command.Parameters.AddWithValue("@AccountId", signUp.AccountId);
                    command.Parameters.AddWithValue("@AccountName", signUp.AccountName);
                    command.Parameters.AddWithValue("@Email", signUp.Email);
                    command.Parameters.AddWithValue("@PasswordHash", signUp.PasswordHash);

                    await command.ExecuteNonQueryAsync();
                }

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Account successfully created."
                };
            }, "Sign up", httpMethod, endpoint, queryParams, requestId);
        }

        private async Task SendEmailAsync(string recipientEmail, string messageBody, string subject)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_serviceEmail, "Furniroom"),
                Subject = subject,
                Body = messageBody,
                IsBodyHtml = false
            };

            message.To.Add(new MailAddress(recipientEmail));

            using (var smtp = new SmtpClient(SmtpHost, SmtpPort))
            {
                smtp.Credentials = new NetworkCredential(_serviceEmail, _servicePassword);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
            }
        }

        private async Task<ServiceResponseModel> ExecuteDatabaseOperation(
            Func<MySqlConnection, Task<ServiceResponseModel>> operation,
            string operationName,
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            try
            {
                await LogActionAsync($"{operationName} started", httpMethod, endpoint, queryParams, requestId);

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var result = await operation(connection);

                    await LogActionAsync(
                        result.Status
                            ? $"{operationName} completed"
                            : $"{operationName} failed: {result.Message}",
                        httpMethod, endpoint, queryParams, requestId);

                    return result;
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return CreateErrorResponse($"Error during {operationName.ToLower()}: {ex.Message}");
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
                    Date = DateTime.UtcNow,
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