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
                return await HandleValidationError(transfer, formattedTime);
            }

            foreach (var validate in validations)
            {
                validate(requestData);
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
                .Where(m => !string.IsNullOrEmpty(m))
                .Distinct();

            string message = errorMessages.Any()
                ? string.Join("; ", errorMessages)
                : "Invalid request structure";

            await LogActionAsync($"Validation failed: {message}", transfer);

            return new APIResponseModel
            {
                Date = formattedTime,
                Status = false,
                Message = message
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
                    data => ValidateDigit(data, "Account ID must be a positive number.")
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
                    data => ValidateLength(data.OldName, "OldName", 50, "Old name cannot exceed 50 characters."),
                    data => ValidateLength(data.NewName, "NewName", 50, "New name cannot exceed 50 characters.")
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
                    data => ValidateEmail(data.OldEmail, "OldEmail"),
                    data => ValidateEmail(data.NewEmail, "NewEmail"),
                    data => ValidateLength(data.OldEmail, "OldEmail", 254, "Old email cannot exceed 254 characters."),
                    data => ValidateLength(data.NewEmail, "NewEmail", 254, "New email cannot exceed 254 characters.")
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
                    data => ValidateLength(data.OldPasswordHash, "OldPasswordHash", 128, "Old password hash cannot exceed 128 characters."),
                    data => ValidateLength(data.NewPasswordHash, "NewPasswordHash", 128, "New password hash cannot exceed 128 characters.")
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
                    data => ValidateDigit(data, "Account ID must be a positive number.")
                });
        }

        private void ValidateDigit(int? value, string errorMessage)
        {
            if (!_validationService.IsValidDigit(value))
            {
                ModelState.AddModelError("AccountId", errorMessage);
            }
        }

        private void ValidateLength(string value, string fieldName, int maxLength, string errorMessage)
        {
            if (!_validationService.IsValidLength(value, maxLength))
            {
                ModelState.AddModelError(fieldName, errorMessage);
            }
        }

        private void ValidateEmail(string email, string fieldName)
        {
            if (!_validationService.IsValidEmail(email))
            {
                ModelState.AddModelError(fieldName, "Email should be in format: example@domain.com");
            }
        }
    }
}