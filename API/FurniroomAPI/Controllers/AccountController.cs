using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Account;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FurniroomAPI.Controllers
{
    [Route("account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IValidationService _validationService;
        private readonly ILoggingService _loggingService;
        private readonly HttpRequest _httpRequest;

        public AccountController(IAccountService accountService, IValidationService validationService, ILoggingService loggingService, IHttpContextAccessor httpContextAccessor)
        {
            _accountService = accountService;
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

        [HttpGet("get-account-information")]
        public async Task<ActionResult<APIResponseModel>> AccountInformation([FromQuery][Required] int? accountId)
        {
            return await ProcessRequest(
                accountId,
                (data, transfer) => _accountService.GetAccountInformationAsync((int)data, transfer),
                data => $"accountId={data}",
                new Action<int?>[]
                {
                    data => ValidateRequired(data, nameof(data)),
                    data => ValidateDigit((int)data, nameof(data))
                });
        }

        [HttpPut("change-name")]
        public async Task<ActionResult<APIResponseModel>> ChangeName([FromBody] ChangeNameModel changeName)
        {
            return await ProcessRequest(
                changeName,
                (data, transfer) => _accountService.ChangeNameAsync(data, transfer),
                data => JsonSerializer.Serialize(data),
                new Action<ChangeNameModel>[]
                {
                    data => ValidateRequired(data.OldName, nameof(data.OldName)),
                    data => ValidateRequired(data.NewName, nameof(data.NewName)),
                    data => ValidateLength(data.OldName, nameof(data.OldName), 50),
                    data => ValidateLength(data.NewName, nameof(data.NewName), 50)
                });
        }

        [HttpPut("change-email")]
        public async Task<ActionResult<APIResponseModel>> ChangeEmail([FromBody] ChangeEmailModel changeEmail)
        {
            return await ProcessRequest(
                changeEmail,
                (data, transfer) => _accountService.ChangeEmailAsync(data, transfer),
                data => JsonSerializer.Serialize(data),
                new Action<ChangeEmailModel>[]
                {
                    data => ValidateRequired(data.OldEmail, nameof(data.OldEmail)),
                    data => ValidateRequired(data.NewEmail, nameof(data.NewEmail)),
                    data => ValidateEmail(data.OldEmail, nameof(data.OldEmail)),
                    data => ValidateEmail(data.NewEmail, nameof(data.NewEmail)),
                    data => ValidateLength(data.OldEmail, nameof(data.OldEmail), 254),
                    data => ValidateLength(data.NewEmail, nameof(data.NewEmail), 254)
                });
        }

        [HttpPut("change-password")]
        public async Task<ActionResult<APIResponseModel>> ChangePassword([FromBody] ChangePasswordModel changePassword)
        {
            return await ProcessRequest(
                changePassword,
                (data, transfer) => _accountService.ChangePasswordAsync(data, transfer),
                data => JsonSerializer.Serialize(data),
                new Action<ChangePasswordModel>[]
                {
                    data => ValidateRequired(data.OldPasswordHash, nameof(data.OldPasswordHash)),
                    data => ValidateRequired(data.NewPasswordHash, nameof(data.NewPasswordHash)),
                    data => ValidateLength(data.OldPasswordHash, nameof(data.OldPasswordHash), 128),
                    data => ValidateLength(data.NewPasswordHash, nameof(data.NewPasswordHash), 128)
                });
        }

        [HttpDelete("delete-account")]
        public async Task<ActionResult<APIResponseModel>> DeleteAccount([FromQuery][Required] int? accountId)
        {
            return await ProcessRequest(
                accountId,
                (data, transfer) => _accountService.DeleteAccountAsync((int)data, transfer),
                data => $"accountId={data}",
                new Action<int?>[]
                {
                    data => ValidateRequired(data, nameof(data)),
                    data => ValidateDigit((int)data, nameof(data))
                });
        }

        private void ValidateRequired(object value, string fieldName)
        {
            if (value == null)
            {
                ModelState.AddModelError(fieldName, $"Field '{fieldName}' is missing");
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
                ModelState.AddModelError(fieldName, $"Field '{fieldName}' must be positive number");
            }
        }

        private void ValidateLength(string value, string fieldName, int maxLength)
        {
            if (!_validationService.IsValidLength(value, maxLength))
            {
                ModelState.AddModelError(fieldName, $"Field '{fieldName}' cannot exceed {maxLength} characters");
            }
        }

        private void ValidateEmail(string email, string fieldName)
        {
            if (!_validationService.IsValidEmail(email))
            {
                ModelState.AddModelError(fieldName, $"Field '{fieldName}' must be valid email (example@domain.com)");
            }
        }
    }
}