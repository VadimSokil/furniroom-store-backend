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

        [HttpGet("get-account-orders-list")]
        public async Task<ActionResult<APIResponseModel>> GetAccountOrders([FromQuery][Required] int? accountId)
        {
            return await ProcessRequest(
                accountId,
                (data, transfer) => _ordersService.GetAccountOrdersAsync((int)data, transfer),
                data => $"accountId={data}",
                new Action<int?>[]
                {
                    data => ValidateDigit(data, "Account ID must be a positive number.")
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
                    data => ValidateDigit(data.OrderId, "Order ID must be a positive number."),
                    data => ValidateLength(data.OrderDate, 20, "Order date cannot exceed 20 characters."),
                    data => ValidateDigit(data.AccountId, "Account ID must be a positive number."),
                    data => ValidatePhoneNumber(data.PhoneNumber),
                    data => ValidateLength(data.PhoneNumber, 20, "Phone number cannot exceed 20 characters."),
                    data => ValidateLength(data.Country, 100, "Country cannot exceed 100 characters."),
                    data => ValidateLength(data.Region, 100, "Region cannot exceed 100 characters."),
                    data => ValidateLength(data.District, 100, "District cannot exceed 100 characters."),
                    data => ValidateLength(data.City, 100, "City cannot exceed 100 characters."),
                    data => ValidateLength(data.Village, 100, "Village cannot exceed 100 characters."),
                    data => ValidateLength(data.Street, 100, "Street cannot exceed 100 characters."),
                    data => ValidateLength(data.HouseNumber, 20, "House number cannot exceed 20 characters."),
                    data => ValidateLength(data.ApartmentNumber, 20, "Apartment number cannot exceed 20 characters."),
                    data => ValidateLength(data.OrderText, 5000, "Order text cannot exceed 5000 characters."),
                    data => ValidateLength(data.DeliveryType, 20, "Delivery type cannot exceed 20 characters.")
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
                    data => ValidateDigit(data.QuestionId, "Question ID must be a positive number."),
                    data => ValidateLength(data.QuestionDate, 20, "Question date cannot exceed 20 characters."),
                    data => ValidateLength(data.UserName, 50, "User name cannot exceed 50 characters."),
                    data => ValidatePhoneNumber(data.PhoneNumber),
                    data => ValidateLength(data.PhoneNumber, 20, "Phone number cannot exceed 20 characters."),
                    data => ValidateEmail(data.Email),
                    data => ValidateLength(data.Email, 254, "Email cannot exceed 254 characters."),
                    data => ValidateLength(data.QuestionText, 5000, "Question text cannot exceed 5000 characters.")
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

        private void ValidatePhoneNumber(string phoneNumber)
        {
            if (!_validationService.IsValidPhoneNumber(phoneNumber))
            {
                ModelState.AddModelError(string.Empty,
                    "Phone number should be in international format: +CCCXXXXXXXXXX");
            }
        }
    }
}