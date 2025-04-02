using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Authorization;
using FurniroomAPI.Models.Response;
using MySql.Data.MySqlClient;
using System.Net.Mail;
using System.Net;
using FurniroomAPI.Models.Log;

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
        private readonly DateTime _logDate;

        public AuthorizationService(
            string connectionString,
            string serviceEmail,
            string servicePassword,
            Dictionary<string, string> requests,
            ILoggingService loggingService,
            Func<DateTime> logDate)
        {
            _connectionString = connectionString;
            _serviceEmail = serviceEmail;
            _servicePassword = servicePassword;
            _requests = requests;
            _loggingService = loggingService;
            _logDate = logDate();
        }

        public async Task<ServiceResponseModel> CheckEmailAsync(string email, string httpMethod, string endpoint, string queryParams, string requestId)
        {
            try
            {
                await LogActionAsync("Check email started", httpMethod, endpoint, queryParams, requestId);

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand(_requests["CheckEmail"], connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        var result = Convert.ToInt32(await command.ExecuteScalarAsync());

                        var response = new ServiceResponseModel
                        {
                            Status = true,
                            Message = result > 0
                                ? "This email address is already in use."
                                : "This email address is available."
                        };

                        await LogActionAsync("Check email completed", httpMethod, endpoint, queryParams, requestId);
                        return response;
                    }
                }
            }
            catch (MySqlException ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"A database error occurred: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
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
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"Error generating code: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseModel> ResetPasswordAsync(string email, string httpMethod, string endpoint, string queryParams, string requestId)
        {
            try
            {
                await LogActionAsync("Reset password started", httpMethod, endpoint, queryParams, requestId);

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var checkEmailCommand = new MySqlCommand(_requests["CheckEmail"], connection))
                    {
                        checkEmailCommand.Parameters.AddWithValue("@Email", email);
                        var emailExists = Convert.ToInt32(await checkEmailCommand.ExecuteScalarAsync()) > 0;

                        if (!emailExists)
                        {
                            await LogActionAsync("Email not found", httpMethod, endpoint, queryParams, requestId);
                            return new ServiceResponseModel
                            {
                                Status = false,
                                Message = "Email address is not found."
                            };
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

                    await LogActionAsync("Password reset completed", httpMethod, endpoint, queryParams, requestId);

                    return new ServiceResponseModel
                    {
                        Status = true,
                        Message = "Password successfully reset.",
                        Data = newPassword
                    };
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"Error resetting password: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseModel> SignInAsync(SignInModel signIn, string httpMethod, string endpoint, string queryParams, string requestId)
        {
            try
            {
                await LogActionAsync("Sign in attempt", httpMethod, endpoint, queryParams, requestId);

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand(_requests["SignIn"], connection))
                    {
                        command.Parameters.AddWithValue("@Email", signIn.Email);
                        command.Parameters.AddWithValue("@PasswordHash", signIn.PasswordHash);

                        var result = await command.ExecuteScalarAsync();
                        if (result == null)
                        {
                            await LogActionAsync("Sign in failed: invalid credentials", httpMethod, endpoint, queryParams, requestId);
                            return new ServiceResponseModel
                            {
                                Status = false,
                                Message = "Incorrect email address or password."
                            };
                        }

                        await LogActionAsync("Sign in successful", httpMethod, endpoint, queryParams, requestId);
                        return new ServiceResponseModel
                        {
                            Status = true,
                            Message = "Authentication successful.",
                            Data = result
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"Error during sign in: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseModel> SignUpAsync(SignUpModel signUp, string httpMethod, string endpoint, string queryParams, string requestId)
        {
            try
            {
                await LogActionAsync("Sign up started", httpMethod, endpoint, queryParams, requestId);

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var checkIdCommand = new MySqlCommand(_requests["CheckAccountId"], connection))
                    {
                        checkIdCommand.Parameters.AddWithValue("@AccountId", signUp.AccountId);
                        if (Convert.ToInt32(await checkIdCommand.ExecuteScalarAsync()) > 0)
                        {
                            await LogActionAsync("Account ID already exists", httpMethod, endpoint, queryParams, requestId);
                            return new ServiceResponseModel
                            {
                                Status = false,
                                Message = "This Account ID is already in use."
                            };
                        }
                    }

                    using (var checkEmailCommand = new MySqlCommand(_requests["CheckEmail"], connection))
                    {
                        checkEmailCommand.Parameters.AddWithValue("@Email", signUp.Email);
                        if (Convert.ToInt32(await checkEmailCommand.ExecuteScalarAsync()) > 0)
                        {
                            await LogActionAsync("Email already exists", httpMethod, endpoint, queryParams, requestId);
                            return new ServiceResponseModel
                            {
                                Status = false,
                                Message = "This Email is already in use."
                            };
                        }
                    }

                    using (var checkNameCommand = new MySqlCommand(_requests["CheckAccountName"], connection))
                    {
                        checkNameCommand.Parameters.AddWithValue("@AccountName", signUp.AccountName);
                        if (Convert.ToInt32(await checkNameCommand.ExecuteScalarAsync()) > 0)
                        {
                            await LogActionAsync("Account name already exists", httpMethod, endpoint, queryParams, requestId);
                            return new ServiceResponseModel
                            {
                                Status = false,
                                Message = "This Account name is already in use."
                            };
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
                }

                await LogActionAsync("Account created successfully", httpMethod, endpoint, queryParams, requestId);
                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Account successfully created."
                };
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"Error during sign up: {ex.Message}"
                };
            }
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
    }
}