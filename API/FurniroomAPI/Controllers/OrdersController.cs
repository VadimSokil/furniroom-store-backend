using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Orders;
using FurniroomAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Controllers
{
    [Route("orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;
        private readonly IValidationService _validationService;
        private readonly string _requestDate;
        private readonly DateTime _logDate;
        private readonly ILoggingService _loggingService;
        private readonly string _requestId;
        private readonly HttpRequest _httpRequest;

        public OrdersController(IOrdersService ordersService, IValidationService validationService, Func<DateTime> requestDate, ILoggingService loggingService, IHttpContextAccessor httpContextAccessor)
        {
            _ordersService = ordersService;
            _validationService = validationService;
            _logDate = requestDate();
            _requestDate = requestDate().ToString("dd/MM/yyyy HH:mm:ss") + " UTC";
            _loggingService = loggingService;
            _requestId = Guid.NewGuid().ToString();
            _httpRequest = httpContextAccessor.HttpContext.Request;
        }

        [HttpGet("get-account-orders-list")]
        public async Task<ActionResult<APIResponseModel>> GetAccountOrders([FromQuery][Required] int? accountId)
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
                var serviceResponse = await _ordersService.GetAccountOrdersAsync(
                    (int)accountId,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    _httpRequest.QueryString.Value ?? string.Empty,
                    _requestId
                );
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

        [HttpPost("add-order")]
        public async Task<ActionResult<APIResponseModel>> AddOrder([FromBody] OrderModel order)
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
            else if (!_validationService.IsValidDigit(order.OrderId))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Order ID must be a positive number.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Order ID must be a positive number."
                };
            }
            else if (!_validationService.IsValidLength(order.OrderDate, 20))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Order date cannot exceed 20 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Order date cannot exceed 20 characters in length."
                };
            }
            else if (!_validationService.IsValidDigit(order.AccountId))
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
            else if (!_validationService.IsValidPhoneNumber(order.PhoneNumber))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "The phone number should be in the international format: +CCCXXXXXXXXXX, where +CCC is the country code and XXXXXXXXXX is the phone number.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "The phone number should be in the international format: +CCCXXXXXXXXXX, where +CCC is the country code and XXXXXXXXXX is the phone number."
                };
            }
            else if (!_validationService.IsValidLength(order.PhoneNumber, 20))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Phone number cannot exceed 20 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Phone number cannot exceed 20 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.Country, 100))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Country cannot exceed 100 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Country cannot exceed 100 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.Region, 100))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Region cannot exceed 100 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Region cannot exceed 100 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.District, 100))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "District cannot exceed 100 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "District cannot exceed 100 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.City, 100))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "City cannot exceed 100 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "City cannot exceed 100 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.Village, 100))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Village cannot exceed 100 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Village cannot exceed 100 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.Street, 100))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Street cannot exceed 100 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Street cannot exceed 100 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.HouseNumber, 20))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "House number cannot exceed 20 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "House number cannot exceed 20 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.ApartmentNumber, 20))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Apartment number cannot exceed 20 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Apartment number cannot exceed 20 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.OrderText, 5000))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Order text cannot exceed 5000 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Order text cannot exceed 5000 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.DeliveryType, 20))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Delivery type cannot exceed 20 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Delivery type cannot exceed 20 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _ordersService.AddOrderAsync(
                    order,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    _httpRequest.QueryString.Value ?? string.Empty,
                    _requestId
            );
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

        [HttpPost("add-question")]
        public async Task<ActionResult<APIResponseModel>> AddQuestion([FromBody] QuestionModel question)
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
            else if (!_validationService.IsValidDigit(question.QuestionId))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Question ID must be a positive number.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Question ID must be a positive number."
                };
            }
            else if (!_validationService.IsValidLength(question.QuestionDate, 20))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Question date cannot exceed 20 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Question date cannot exceed 20 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(question.UserName, 50))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "User name cannot exceed 50 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "User name cannot exceed 50 characters in length."
                };
            }
            else if (!_validationService.IsValidPhoneNumber(question.PhoneNumber))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "The phone number should be in the international format: +CCCXXXXXXXXXX, where +CCC is the country code and XXXXXXXXXX is the phone number.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "The phone number should be in the international format: +CCCXXXXXXXXXX, where +CCC is the country code and XXXXXXXXXX is the phone number."
                };
            }
            else if (!_validationService.IsValidLength(question.PhoneNumber, 20))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Phone number cannot exceed 20 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Phone number cannot exceed 20 characters in length."
                };
            }
            else if (!_validationService.IsValidEmail(question.Email))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }

            else if (!_validationService.IsValidLength(question.Email, 254))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Email address cannot exceed 254 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address cannot exceed 254 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(question.QuestionText, 5000))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Question text cannot exceed 5000 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Question text cannot exceed 5000 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _ordersService.AddQuestionAsync(
                    question,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    _httpRequest.QueryString.Value ?? string.Empty,
                    _requestId
            );
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
