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

        private async Task<ActionResult<APIResponseModel>> ProcessRequest<T>(T requestData, Func<T, TransferLogModel, Task<ServiceResponseModel>> serviceCall, Func<T, string> getQueryParams, Action<T>[] validations)
        {
            var requestId = Guid.NewGuid().ToString();
            var formattedTime = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") + " UTC";
            var queryParams = getQueryParams(requestData);

            var transfer = new TransferLogModel
            {
                HttpMethod = _httpRequest.Method,
                Endpoint = _httpRequest.Path,
                QueryParams = queryParams,
                RequestId = requestId
            };

            await LogActionAsync("Request started", transfer);

            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrEmpty(m));

                if (errorMessages.Any())
                {
                    await LogActionAsync($"Request validation failed: {string.Join("; ", errorMessages)}", transfer);
                    return new APIResponseModel
                    {
                        Date = formattedTime,
                        Status = false,
                        Message = string.Join("; ", errorMessages)
                    };
                }

                var typeErrors = ModelState
                    .Where(x => x.Value.Errors.Any(e => e.Exception != null))
                    .Select(x => $"Field '{x.Key}' has invalid type");

                if (typeErrors.Any())
                {
                    await LogActionAsync("Type validation failed", transfer);
                    return new APIResponseModel
                    {
                        Date = formattedTime,
                        Status = false,
                        Message = string.Join("; ", typeErrors)
                    };
                }

                await LogActionAsync("Invalid request structure", transfer);
                return new APIResponseModel
                {
                    Date = formattedTime,
                    Status = false,
                    Message = "Invalid request structure"
                };
            }

            foreach (var validation in validations)
            {
                validation(requestData);
            }

            if (!ModelState.IsValid)
            {
                return await HandleValidationError(transfer, formattedTime);
            }

            var serviceResponse = await serviceCall(requestData, transfer);
            var gatewayResponse = new APIResponseModel
            {
                Date = formattedTime,
                Status = serviceResponse.Status,
                Message = serviceResponse.Message,
                Data = serviceResponse.Data
            };

            await LogActionAsync("Request completed", transfer);
            return Ok(gatewayResponse);
        }

        private async Task<ActionResult<APIResponseModel>> HandleValidationError(TransferLogModel transfer, string formattedTime)
        {
            var errorMessages = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrEmpty(m));

            await LogActionAsync($"Validation failed: {string.Join("; ", errorMessages)}", transfer);

            return new APIResponseModel
            {
                Date = formattedTime,
                Status = false,
                Message = string.Join("; ", errorMessages)
            };
        }

        [HttpPost("sign-up")]
        public async Task<ActionResult<APIResponseModel>> SignUp([FromBody] SignUpModel signUp)
        {
            return await ProcessRequest(
                signUp,
                (data, transfer) => _authorizationService.SignUpAsync(data, transfer),
                data => JsonSerializer.Serialize(data),
                new Action<SignUpModel>[]
                {
                    data => ValidateRequired(data.AccountId, nameof(data.AccountId)),
                    data => ValidateDigit((int)data.AccountId, nameof(data.AccountId)),
                    data => ValidateRequired(data.AccountName, nameof(data.AccountName)),
                    data => ValidateLength(data.AccountName, nameof(data.AccountName), 50),
                    data => ValidateRequired(data.Email, nameof(data.Email)),
                    data => ValidateEmail(data.Email, nameof(data.Email)),
                    data => ValidateLength(data.Email, nameof(data.Email), 254),
                    data => ValidateRequired(data.PasswordHash, nameof(data.PasswordHash)),
                    data => ValidateLength(data.PasswordHash, nameof(data.PasswordHash), 128)
                });
        }

        [HttpPost("sign-in")]
        public async Task<ActionResult<APIResponseModel>> SignIn([FromBody] SignInModel signIn)
        {
            return await ProcessRequest(
                signIn,
                (data, transfer) => _authorizationService.SignInAsync(data, transfer),
                data => JsonSerializer.Serialize(data),
                new Action<SignInModel>[]
                {
                    data => ValidateRequired(data.Email, nameof(data.Email)),
                    data => ValidateEmail(data.Email, nameof(data.Email)),
                    data => ValidateLength(data.Email, nameof(data.Email), 254),
                    data => ValidateRequired(data.PasswordHash, nameof(data.PasswordHash)),
                    data => ValidateLength(data.PasswordHash, nameof(data.PasswordHash), 128)
                });
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<APIResponseModel>> ResetPassword([FromBody][Required] string email)
        {
            return await ProcessRequest(
                email,
                (data, transfer) => _authorizationService.ResetPasswordAsync(data, transfer),
                data => string.Empty,
                new Action<string>[]
                {
                    data => ValidateRequired(data, nameof(data)),
                    data => ValidateEmail(data, nameof(data)),
                    data => ValidateLength(data, nameof(data), 254)
                });
        }

        [HttpGet("check-email")]
        public async Task<ActionResult<APIResponseModel>> CheckEmail([FromQuery][Required] string email)
        {
            return await ProcessRequest(
                email,
                (data, transfer) => _authorizationService.CheckEmailAsync(data, transfer),
                data => $"email={WebUtility.UrlEncode(data)}",
                new Action<string>[]
                {
                    data => ValidateRequired(data, nameof(data)),
                    data => ValidateEmail(data, nameof(data)),
                    data => ValidateLength(data, nameof(data), 254)
                });
        }

        [HttpGet("generate-verification-code")]
        public async Task<ActionResult<APIResponseModel>> GenerateCode([FromQuery][Required] string email)
        {
            return await ProcessRequest(
                email,
                (data, transfer) => _authorizationService.GenerateCodeAsync(data, transfer),
                data => string.Empty,
                new Action<string>[]
                {
                    data => ValidateRequired(data, nameof(data)),
                    data => ValidateEmail(data, nameof(data)),
                    data => ValidateLength(data, nameof(data), 254)
                });
        }

        private void ValidateRequired(object value, string fieldName)
        {
            if (value == null)
            {
                ModelState.AddModelError(fieldName, $"Field '{fieldName}' is required");
            }
            else if (value is string strValue && string.IsNullOrWhiteSpace(strValue))
            {
                ModelState.AddModelError(fieldName, $"Field '{fieldName}' cannot be empty");
            }
        }

        private void ValidateDigit(int value, string fieldName)
        {
            if (!_validationService.IsValidDigit(value))
            {
                ModelState.AddModelError(fieldName, $"Field '{fieldName}' must be a positive number");
            }
        }

        private void ValidateLength(string value, string fieldName, int maxLength)
        {
            if (value != null && !_validationService.IsValidLength(value, maxLength))
            {
                ModelState.AddModelError(fieldName, $"Field '{fieldName}' cannot exceed {maxLength} characters");
            }
        }

        private void ValidateEmail(string email, string fieldName)
        {
            if (!_validationService.IsValidEmail(email))
            {
                ModelState.AddModelError(fieldName, $"Field '{fieldName}' must be a valid email (example@domain.com)");
            }
        }

        private async Task LogActionAsync(string status, TransferLogModel transfer)
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
    }
}