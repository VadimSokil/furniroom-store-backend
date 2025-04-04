using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Orders;
using FurniroomAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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

        [HttpGet("get-account-orders-list")]
        public async Task<ActionResult<APIResponseModel>> GetAccountOrders([FromQuery][Required] int? accountId)
        {
            return await ProcessRequest(
                accountId,
                (data, transfer) => _ordersService.GetAccountOrdersAsync((int)data, transfer),
                data => $"accountId={data}",
                new Action<int?>[]
                {
                    data => ValidateDigit(data, "OrderId", "Order ID must be a positive number.")
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
                    data => ValidateDigit(data.OrderId, "OrderId", "Order ID must be a positive number."),
                    data => ValidateLength(data.OrderDate, "OrderDate", 20, "Order date cannot exceed 20 characters."),
                    data => ValidateDigit(data.AccountId, "AccountId", "Account ID must be a positive number."),
                    data => ValidatePhoneNumber(data.PhoneNumber, "PhoneNumber"),
                    data => ValidateLength(data.PhoneNumber, "PhoneNumber", 20, "Phone number cannot exceed 20 characters."),
                    data => ValidateLength(data.Country, "Country", 100, "Country cannot exceed 100 characters."),
                    data => ValidateLength(data.Region, "Region", 100, "Region cannot exceed 100 characters."),
                    data => ValidateLength(data.District, "District", 100, "District cannot exceed 100 characters."),
                    data => ValidateLength(data.City, "City", 100, "City cannot exceed 100 characters."),
                    data => ValidateLength(data.Village, "Village", 100, "Village cannot exceed 100 characters."),
                    data => ValidateLength(data.Street, "Street", 100, "Street cannot exceed 100 characters."),
                    data => ValidateLength(data.HouseNumber, "HouseNumber", 20, "House number cannot exceed 20 characters."),
                    data => ValidateLength(data.ApartmentNumber, "ApartmentNumber", 20, "Apartment number cannot exceed 20 characters."),
                    data => ValidateLength(data.OrderText, "OrderText", 5000, "Order text cannot exceed 5000 characters."),
                    data => ValidateLength(data.DeliveryType, "DeliveryType", 20, "Delivery type cannot exceed 20 characters.")
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
                    data => ValidateDigit(data.QuestionId, "QuestionId", "Question ID must be a positive number."),
                    data => ValidateLength(data.QuestionDate, "QuestionDate", 20, "Question date cannot exceed 20 characters."),
                    data => ValidateLength(data.UserName, "UserName", 50, "User name cannot exceed 50 characters."),
                    data => ValidatePhoneNumber(data.PhoneNumber, "PhoneNumber"),
                    data => ValidateLength(data.PhoneNumber, "PhoneNumber", 20, "Phone number cannot exceed 20 characters."),
                    data => ValidateEmail(data.Email, "Email"),
                    data => ValidateLength(data.Email, "Email", 254, "Email cannot exceed 254 characters."),
                    data => ValidateLength(data.QuestionText, "QuestionText", 5000, "Question text cannot exceed 5000 characters.")
                });
        }

        private void ValidateDigit(int? value, string fieldName, string errorMessage)
        {
            if (!_validationService.IsValidDigit(value))
            {
                ModelState.AddModelError(fieldName, errorMessage);
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

        private void ValidatePhoneNumber(string phoneNumber, string fieldName)
        {
            if (!_validationService.IsValidPhoneNumber(phoneNumber))
            {
                ModelState.AddModelError(fieldName,
                    "Phone number should be in international format: +CCCXXXXXXXXXX");
            }
        }
    }
}