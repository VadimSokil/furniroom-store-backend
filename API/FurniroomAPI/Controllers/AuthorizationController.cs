using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Authorization;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace FurniroomAPI.Controllers
{
    [Route("authorization")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IValidationService _validationService;
        private readonly ILoggingService _loggingService;
        private readonly HttpRequest _httpRequest;

        public AuthorizationController(IAuthorizationService authorizationService, IValidationService validationService, ILoggingService loggingService, IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _validationService = validationService;
            _loggingService = loggingService;
            _httpRequest = httpContextAccessor.HttpContext.Request;
        }

        private async Task<ActionResult<APIResponseModel>> ProcessRequest<T>(T requestData, Func<T, Task<ServiceResponseModel>> serviceCall, Func<T, string> getQueryParams, Action<T>[] validations)
        {
            var requestId = Guid.NewGuid().ToString();
            var formattedTime = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") + " UTC";

            var queryParams = getQueryParams(requestData);

            await LogActionAsync("Request started", queryParams, requestId);

            foreach (var validate in validations)
            {
                validate(requestData);
            }

            if (!ModelState.IsValid)
            {
                return await HandleValidationError("Invalid request structure", queryParams, requestId, formattedTime);
            }

            var serviceResponse = await serviceCall(requestData);
            var gatewayResponse = new APIResponseModel
            {
                Date = formattedTime,
                Status = serviceResponse.Status,
                Message = serviceResponse.Message,
                Data = serviceResponse.Data
            };

            await LogActionAsync("Request completed", queryParams, requestId);
            return Ok(gatewayResponse);
        }

        private async Task LogActionAsync(string status, string queryParams, string requestId)
        {
            await _loggingService.AddLogAsync(new LogModel
            {
                Date = DateTime.UtcNow,
                HttpMethod = _httpRequest.Method,
                Endpoint = _httpRequest.Path,
                QueryParams = queryParams,
                Status = status,
                RequestId = requestId
            });
        }

        private async Task<ActionResult<APIResponseModel>> HandleValidationError(
            string message, string queryParams, string requestId, string formattedTime)
        {
            await LogActionAsync(message, queryParams, requestId);
            return new APIResponseModel
            {
                Date = formattedTime,
                Status = false,
                Message = message
            };
        }

        [HttpPost("sign-up")]
        public async Task<ActionResult<APIResponseModel>> SignUp([FromBody] SignUpModel signUp)
        {
            return await ProcessRequest(
                signUp,
                data => _authorizationService.SignUpAsync(
                    data,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    JsonSerializer.Serialize(data),
                    Guid.NewGuid().ToString()),
                data => JsonSerializer.Serialize(data),
                new Action<SignUpModel>[]
                {
                    data => ValidateDigit((int)data.AccountId, "Account ID must be a positive number."),
                    data => ValidateLength(data.AccountName, 50, "Account name cannot exceed 50 characters."),
                    data => ValidateEmail(data.Email),
                    data => ValidateLength(data.Email, 254, "Email cannot exceed 254 characters."),
                    data => ValidateLength(data.PasswordHash, 128, "Password hash cannot exceed 128 characters.")
                });
        }

        [HttpPost("sign-in")]
        public async Task<ActionResult<APIResponseModel>> SignIn([FromBody] SignInModel signIn)
        {
            return await ProcessRequest(
                signIn,
                data => _authorizationService.SignInAsync(
                    data,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    JsonSerializer.Serialize(data),
                    Guid.NewGuid().ToString()),
                data => JsonSerializer.Serialize(data),
                new Action<SignInModel>[]
                {
                    data => ValidateEmail(data.Email),
                    data => ValidateLength(data.Email, 254, "Email cannot exceed 254 characters."),
                    data => ValidateLength(data.PasswordHash, 128, "Password hash cannot exceed 128 characters.")
                });
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<APIResponseModel>> ResetPassword([FromBody][Required] string email)
        {
            return await ProcessRequest(
                email,
                data => _authorizationService.ResetPasswordAsync(
                    data,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    string.Empty,
                    Guid.NewGuid().ToString()),
                data => string.Empty,
                new Action<string>[]
                {
                    data => ValidateEmail(data),
                    data => ValidateLength(data, 254, "Email cannot exceed 254 characters.")
                });
        }

        [HttpGet("check-email")]
        public async Task<ActionResult<APIResponseModel>> CheckEmail([FromQuery][Required] string email)
        {
            return await ProcessRequest(
                email,
                data => _authorizationService.CheckEmailAsync(
                    data,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    $"email={WebUtility.UrlEncode(data)}",
                    Guid.NewGuid().ToString()),
                data => $"email={WebUtility.UrlEncode(data)}",
                new Action<string>[]
                {
                    data => ValidateEmail(data),
                    data => ValidateLength(data, 254, "Email cannot exceed 254 characters.")
                });
        }

        [HttpGet("generate-verification-code")]
        public async Task<ActionResult<APIResponseModel>> GenerateCode([FromQuery][Required] string email)
        {
            return await ProcessRequest(
                email,
                data => _authorizationService.GenerateCodeAsync(
                    data,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    string.Empty,
                    Guid.NewGuid().ToString()),
                data => string.Empty,
                new Action<string>[]
                {
                    data => ValidateEmail(data),
                    data => ValidateLength(data, 254, "Email cannot exceed 254 characters.")
                });
        }

        private void ValidateDigit(int value, string errorMessage)
        {
            if (!_validationService.IsValidDigit(value))
            {
                ModelState.AddModelError(string.Empty, errorMessage);
            }
        }

        private void ValidateLength(string value, int maxLength, string errorMessage)
        {
            if (!_validationService.IsValidLength(value, maxLength))
            {
                ModelState.AddModelError(string.Empty, errorMessage);
            }
        }

        private void ValidateEmail(string email)
        {
            if (!_validationService.IsValidEmail(email))
            {
                ModelState.AddModelError(string.Empty,
                    "Email should be in format: example@domain.com");
            }
        }
    }
}