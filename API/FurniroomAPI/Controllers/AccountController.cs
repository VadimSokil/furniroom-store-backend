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

            foreach (var validate in validations)
            {
                validate(requestData);
            }

            if (!ModelState.IsValid)
            {
                return await HandleValidationError("Invalid request structure", transfer, formattedTime);
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

        private async Task<ActionResult<APIResponseModel>> HandleValidationError(string message, TransferLogModel transfer, string formattedTime)
        {
            await LogActionAsync(message, transfer);
            return new APIResponseModel
            {
                Date = formattedTime,
                Status = false,
                Message = message
            };
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
                    data => ValidateLength(data.OldName, 50, "Old name cannot exceed 50 characters."),
                    data => ValidateLength(data.NewName, 50, "New name cannot exceed 50 characters.")
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
                    data => ValidateEmail(data.OldEmail),
                    data => ValidateEmail(data.NewEmail),
                    data => ValidateLength(data.OldEmail, 254, "Old email cannot exceed 254 characters."),
                    data => ValidateLength(data.NewEmail, 254, "New email cannot exceed 254 characters.")
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
                    data => ValidateLength(data.OldPasswordHash, 128, "Old password hash cannot exceed 128 characters."),
                    data => ValidateLength(data.NewPasswordHash, 128, "New password hash cannot exceed 128 characters.")
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