using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Account;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Controllers
{
    [Route("account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IValidationService _validationService;
        private readonly string _requestDate;
        private readonly DateTime _logDate;
        private readonly ILoggingService _loggingService;
        private readonly string _requestId;
        private readonly HttpRequest _httpRequest;

        public AccountController(IAccountService accountService, IValidationService validationService, Func<DateTime> requestDate, ILoggingService loggingService, IHttpContextAccessor httpContextAccessor)
        {
            _accountService = accountService;
            _validationService = validationService;
            _logDate = requestDate();
            _requestDate = requestDate().ToString("dd/MM/yyyy HH:mm:ss") + " UTC";
            _loggingService = loggingService;
            _requestId = Guid.NewGuid().ToString();
            _httpRequest = httpContextAccessor.HttpContext.Request;
        }

        [HttpGet("get-account-information")]
        public async Task<ActionResult<APIResponseModel>> AccountInformation([FromQuery][Required] int? accountId)
        {
            var log = new LogModel
            {
                Date = _logDate,
                HttpMethod = _httpRequest.Method,
                Endpoint = _httpRequest.Path,
                QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                Status = "Received a new request",
                RequestId = _requestId
            };

            await _loggingService.AddLogAsync(log);

            if (!ModelState.IsValid)
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Structure of your request is different from what the server expects or has empty fields.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidDigit(accountId))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Account ID must be a positive number.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Account ID must be a positive number."
                };
            }
            else
            {
                var serviceResponse = await _accountService.GetAccountInformationAsync(
                    (int)accountId,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    _httpRequest.QueryString.Value ?? string.Empty,
                    _requestId);

                var gatewayResponse = new APIResponseModel
                {
                    Date = _requestDate,
                    Status = serviceResponse.Status,
                    Message = serviceResponse.Message,
                    Data = serviceResponse.Data
                };
                return Ok(gatewayResponse);
            }
        }

        [HttpPut("change-name")]
        public async Task<ActionResult<APIResponseModel>> ChangeName([FromBody] ChangeNameModel changeName)
        {
            var log = new LogModel
            {
                Date = _logDate,
                HttpMethod = _httpRequest.Method,
                Endpoint = _httpRequest.Path,
                QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                Status = "Received a new request",
                RequestId = _requestId
            };

            await _loggingService.AddLogAsync(log);

            if (!ModelState.IsValid)
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Structure of your request is different from what the server expects or has empty fields.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidLength(changeName.OldName, 50))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Old name cannot exceed 50 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Old name cannot exceed 50 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(changeName.NewName, 50))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "New name cannot exceed 50 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "New name cannot exceed 50 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _accountService.ChangeNameAsync(
                    changeName,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    _httpRequest.QueryString.Value ?? string.Empty,
                    _requestId);

                var gatewayResponse = new APIResponseModel
                {
                    Date = _requestDate,
                    Status = serviceResponse.Status,
                    Message = serviceResponse.Message,
                    Data = serviceResponse.Data
                };
                return Ok(gatewayResponse);
            }
        }

        [HttpPut("change-email")]
        public async Task<ActionResult<APIResponseModel>> ChangeEmail([FromBody] ChangeEmailModel changeEmail)
        {
            var log = new LogModel
            {
                Date = _logDate,
                HttpMethod = _httpRequest.Method,
                Endpoint = _httpRequest.Path,
                QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                Status = "Received a new request",
                RequestId = _requestId
            };

            await _loggingService.AddLogAsync(log);

            if (!ModelState.IsValid)
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Structure of your request is different from what the server expects or has empty fields.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidEmail(changeEmail.OldEmail))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Old email address should be in the format: example@domain.com, where example is the username and domain.com is the domain.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Old email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }
            else if (!_validationService.IsValidEmail(changeEmail.NewEmail))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "New email address should be in the format: example@domain.com, where example is the username and domain.com is the domain.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "New email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }
            else if (!_validationService.IsValidLength(changeEmail.OldEmail, 254))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Old email address cannot exceed 254 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Old email address cannot exceed 254 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(changeEmail.NewEmail, 254))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "New email address cannot exceed 254 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "New email address cannot exceed 254 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _accountService.ChangeEmailAsync(
                    changeEmail,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    _httpRequest.QueryString.Value ?? string.Empty,
                    _requestId);
                var gatewayResponse = new APIResponseModel
                {
                    Date = _requestDate,
                    Status = serviceResponse.Status,
                    Message = serviceResponse.Message,
                    Data = serviceResponse.Data
                };
                return Ok(gatewayResponse);
            }
        }

        [HttpPut("change-password")]
        public async Task<ActionResult<APIResponseModel>> ChangePassword([FromBody] ChangePasswordModel changePassword)
        {
            var log = new LogModel
            {
                Date = _logDate,
                HttpMethod = _httpRequest.Method,
                Endpoint = _httpRequest.Path,
                QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                Status = "Received a new request",
                RequestId = _requestId
            };

            await _loggingService.AddLogAsync(log);

            if (!ModelState.IsValid)
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Structure of your request is different from what the server expects or has empty fields.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidLength(changePassword.OldPasswordHash, 128))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Old password hash cannot exceed 128 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Old password hash cannot exceed 128 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(changePassword.NewPasswordHash, 128))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "New password hash cannot exceed 128 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "New password hash cannot exceed 128 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _accountService.ChangePasswordAsync(
                    changePassword,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    _httpRequest.QueryString.Value ?? string.Empty,
                    _requestId);
                var gatewayResponse = new APIResponseModel
                {
                    Date = _requestDate,
                    Status = serviceResponse.Status,
                    Message = serviceResponse.Message,
                    Data = serviceResponse.Data
                };
                return Ok(gatewayResponse);
            }
        }

        [HttpDelete("delete-account")]
        public async Task<ActionResult<APIResponseModel>> DeleteAccount([FromQuery][Required] int? accountId)
        {
            var log = new LogModel
            {
                Date = _logDate,
                HttpMethod = _httpRequest.Method,
                Endpoint = _httpRequest.Path,
                QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                Status = "Received a new request",
                RequestId = _requestId
            };

            await _loggingService.AddLogAsync(log);

            if (!ModelState.IsValid)
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Structure of your request is different from what the server expects or has empty fields.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidDigit(accountId))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Account ID must be a positive number.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Account ID must be a positive number."
                };
            }
            else
            {
                var serviceResponse = await _accountService.DeleteAccountAsync(
                    (int)accountId,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    _httpRequest.QueryString.Value ?? string.Empty,
                    _requestId);
                var gatewayResponse = new APIResponseModel
                {
                    Date = _requestDate,
                    Status = serviceResponse.Status,
                    Message = serviceResponse.Message,
                    Data = serviceResponse.Data
                };
                return Ok(gatewayResponse);
            }
        }
    }
}
