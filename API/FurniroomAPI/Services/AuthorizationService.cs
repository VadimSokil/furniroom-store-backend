using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Authorization;
using FurniroomAPI.Models.Response;
using MySql.Data.MySqlClient;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;

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

        public AuthorizationService(string connectionString, string serviceEmail, string servicePassword, Dictionary<string, string> requests)
        {
            _connectionString = connectionString;
            _serviceEmail = serviceEmail;
            _servicePassword = servicePassword;
            _requests = requests;
        }

        public async Task<ServiceResponseModel> CheckEmailAsync(string email)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand(_requests["CheckEmail"], connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        var result = Convert.ToInt32(await command.ExecuteScalarAsync());

                        if (result > 0)
                        {
                            return new ServiceResponseModel
                            {
                                Status = true,
                                Message = "This email address is already in use."
                            };
                        }
                    }
                }

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "This email address is available."
                };
            }
            catch (MySqlException ex)
            {
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"A database error occurred: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }


        public async Task<ServiceResponseModel> GenerateCodeAsync(string email)
        {
            try
            {
                int verificationCode = Random.Shared.Next(1000, 9999);

                string messageBody = $"Hi, your verification code: {verificationCode}";
                await SendEmailAsync(email, messageBody, "Verification code");

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Verification code has been generated.",
                    Data = verificationCode
                };
            }
            catch (MySqlException ex)
            {
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"A database error occurred: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }

        }

        public async Task<ServiceResponseModel> ResetPasswordAsync(string email)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var checkEmailCommand = new MySqlCommand(_requests["CheckEmail"], connection))
                    {
                        checkEmailCommand.Parameters.AddWithValue("@Email", email);
                        var emailExists = Convert.ToInt32(await checkEmailCommand.ExecuteScalarAsync()) > 0;

                        if (!emailExists)
                        {
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

                    string hashedPassword = HashPasswordWithMD5(newPassword);

                    using (var updateCommand = new MySqlCommand(_requests["ResetPassword"], connection))
                    {
                        updateCommand.Parameters.AddWithValue("@Email", email);
                        updateCommand.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                        await updateCommand.ExecuteNonQueryAsync();
                    }

                    await SendEmailAsync(email, $"Hi, your new password: {newPassword}", "Reset Password");

                    return new ServiceResponseModel
                    {
                        Status = true,
                        Message = "Password successfully reseted.",
                        Data = newPassword
                    };
                }

            }
            catch (MySqlException ex)
            {
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"Database error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseModel> SignInAsync(SignInModel signIn)
        {
            try
            {
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
                            return new ServiceResponseModel
                            {
                                Status = true,
                                Message = $"Incorrect email address or password."
                            };
                        }

                        return new ServiceResponseModel
                        {
                            Status = true,
                            Message = "Data confirmed.",
                            Data = result
                        };
                    }
                }
            }
            catch (MySqlException ex)
            {
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"A database error occurred: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseModel> SignUpAsync(SignUpModel signUp)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var checkIdCommand = new MySqlCommand(_requests["CheckAccountId"], connection))
                    {
                        checkIdCommand.Parameters.AddWithValue("@AccountId", signUp.AccountId);
                        var idExists = Convert.ToInt32(await checkIdCommand.ExecuteScalarAsync()) > 0;

                        if (idExists)
                        {
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
                        var emailExists = Convert.ToInt32(await checkEmailCommand.ExecuteScalarAsync()) > 0;

                        if (emailExists)
                        {
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
                        var nameExists = Convert.ToInt32(await checkNameCommand.ExecuteScalarAsync()) > 0;

                        if (nameExists)
                        {
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

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Account successfully created."
                };
            }
            catch (MySqlException ex)
            {
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"A database error occurred: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseModel
                {
                    Status = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
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

        private string HashPasswordWithMD5(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }


    }
}
