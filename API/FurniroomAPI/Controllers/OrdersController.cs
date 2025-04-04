using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Orders;
using FurniroomAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace FurniroomAPI.Controllers
{
    [Route("orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;
        private readonly IValidationService _validationService;
        private readonly ILoggingService _loggingService;
        private readonly HttpRequest _httpRequest;

        public OrdersController(IOrdersService ordersService, IValidationService validationService, ILoggingService loggingService, IHttpContextAccessor httpContextAccessor)
        {
            _ordersService = ordersService;
            _validationService = validationService;
            _loggingService = loggingService;
            _httpRequest = httpContextAccessor.HttpContext.Request;
        }

        private async Task<ActionResult<APIResponseModel>> ProcessRequest<T>(T requestData,Func<T, TransferLogModel, Task<ServiceResponseModel>> serviceCall, Func<T, string> getQueryParams, Action<T>[] validations)
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

        [HttpGet("get-account-orders-list")]
        public async Task<ActionResult<APIResponseModel>> GetAccountOrders([FromQuery][Required] int? accountId)
        {
            return await ProcessRequest(
                accountId,
                (data, transfer) => _ordersService.GetAccountOrdersAsync((int)data, transfer),
                data => $"accountId={WebUtility.UrlEncode(data.ToString())}",
                new Action<int?>[]
                {
                    data => ValidateRequired(data, nameof(data)),
                    data => ValidateDigit((int)data, nameof(data))
                });
        }

        [HttpPost("add-order")]
        public async Task<ActionResult<APIResponseModel>> AddOrder([FromBody] OrderModel order)
        {
            return await ProcessRequest(
                order,
                (data, transfer) => _ordersService.AddOrderAsync(data, transfer),
                data => JsonSerializer.Serialize(data),
                new Action<OrderModel>[]
                {
                    data => ValidateRequired(data.OrderId, nameof(data.OrderId)),
                    data => ValidateDigit((int)data.OrderId, nameof(data.OrderId)),
                    data => ValidateRequired(data.OrderDate, nameof(data.OrderDate)),
                    data => ValidateLength(data.OrderDate, nameof(data.OrderDate), 20),
                    data => ValidateRequired(data.AccountId, nameof(data.AccountId)),
                    data => ValidateDigit((int)data.AccountId, nameof(data.AccountId)),
                    data => ValidateRequired(data.PhoneNumber, nameof(data.PhoneNumber)),
                    data => ValidatePhoneNumber(data.PhoneNumber, nameof(data.PhoneNumber)),
                    data => ValidateLength(data.PhoneNumber, nameof(data.PhoneNumber), 20),
                    data => ValidateLength(data.Country, nameof(data.Country), 100),
                    data => ValidateLength(data.Region, nameof(data.Region), 100),
                    data => ValidateLength(data.District, nameof(data.District), 100),
                    data => ValidateLength(data.City, nameof(data.City), 100),
                    data => ValidateLength(data.Village, nameof(data.Village), 100),
                    data => ValidateLength(data.Street, nameof(data.Street), 100),
                    data => ValidateLength(data.HouseNumber, nameof(data.HouseNumber), 20),
                    data => ValidateLength(data.ApartmentNumber, nameof(data.ApartmentNumber), 20),
                    data => ValidateLength(data.OrderText, nameof(data.OrderText), 5000),
                    data => ValidateLength(data.DeliveryType, nameof(data.DeliveryType), 20)
                });
        }

        [HttpPost("add-question")]
        public async Task<ActionResult<APIResponseModel>> AddQuestion([FromBody] QuestionModel question)
        {
            return await ProcessRequest(
                question,
                (data, transfer) => _ordersService.AddQuestionAsync(data, transfer),
                data => JsonSerializer.Serialize(data),
                new Action<QuestionModel>[]
                {
                    data => ValidateRequired(data.QuestionId, nameof(data.QuestionId)),
                    data => ValidateDigit((int)data.QuestionId, nameof(data.QuestionId)),
                    data => ValidateRequired(data.QuestionDate, nameof(data.QuestionDate)),
                    data => ValidateLength(data.QuestionDate, nameof(data.QuestionDate), 20),
                    data => ValidateRequired(data.UserName, nameof(data.UserName)),
                    data => ValidateLength(data.UserName, nameof(data.UserName), 50),
                    data => ValidateRequired(data.PhoneNumber, nameof(data.PhoneNumber)),
                    data => ValidatePhoneNumber(data.PhoneNumber, nameof(data.PhoneNumber)),
                    data => ValidateLength(data.PhoneNumber, nameof(data.PhoneNumber), 20),
                    data => ValidateRequired(data.Email, nameof(data.Email)),
                    data => ValidateEmail(data.Email, nameof(data.Email)),
                    data => ValidateLength(data.Email, nameof(data.Email), 254),
                    data => ValidateRequired(data.QuestionText, nameof(data.QuestionText)),
                    data => ValidateLength(data.QuestionText, nameof(data.QuestionText), 5000)
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

        private void ValidatePhoneNumber(string phoneNumber, string fieldName)
        {
            if (!_validationService.IsValidPhoneNumber(phoneNumber))
            {
                ModelState.AddModelError(fieldName,
                    $"Field '{fieldName}' must be in international format: +CCCXXXXXXXXXX");
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