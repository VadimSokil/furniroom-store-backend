using FurniroomAPI.Interfaces;
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
        public OrdersController(IOrdersService ordersService, Func<DateTime> requestDate , IValidationService validationService)
        {
            _ordersService = ordersService;
            _requestDate = requestDate().ToString("dd/MM/yyyy HH:mm:ss") + " UTC";
            _validationService = validationService;
        }

        [HttpGet("get-account-orders-list")]
        public async Task<ActionResult<APIResponseModel>> GetAccountOrders([FromQuery][Required] int? accountId)
        {
            if (!ModelState.IsValid)
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidDigit(accountId))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Account ID must be a positive number."
                };
            }
            else
            {
                var serviceResponse = await _ordersService.GetAccountOrdersAsync((int)accountId);
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
            if (!ModelState.IsValid)
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Some fields of the request are missing or empty."
                };
            }
            else if (!_validationService.IsValidDigit(order.OrderId))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Order ID must be a positive number."
                };
            }
            else if (!_validationService.IsValidLength(order.OrderDate, 20))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Order date cannot exceed 20 characters in length."
                };
            }
            else if (!_validationService.IsValidDigit(order.AccountId))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Account ID must be a positive number."
                };
            }
            else if (!_validationService.IsValidPhoneNumber(order.PhoneNumber))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "The phone number should be in the international format: +CCCXXXXXXXXXX, where +CCC is the country code and XXXXXXXXXX is the phone number."
                };
            }
            else if (!_validationService.IsValidLength(order.PhoneNumber, 20))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Phone number cannot exceed 20 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.Country, 100))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Country cannot exceed 100 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.Region, 100))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Region cannot exceed 100 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.District, 100))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "District cannot exceed 100 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.City, 100))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "City cannot exceed 100 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.Village, 100))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Village cannot exceed 100 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.Street, 100))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Street cannot exceed 100 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.HouseNumber, 20))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "House number cannot exceed 20 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.ApartmentNumber, 20))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Apartment number cannot exceed 20 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.OrderText, 5000))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Order text cannot exceed 5000 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(order.DeliveryType, 20))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Delivery type cannot exceed 20 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _ordersService.AddOrderAsync(order);
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
            if (!ModelState.IsValid)
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidDigit(question.QuestionId))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Question ID must be a positive number."
                };
            }
            else if (!_validationService.IsValidLength(question.QuestionDate, 20))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Question date cannot exceed 20 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(question.UserName, 50))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "User name cannot exceed 50 characters in length."
                };
            }
            else if (!_validationService.IsValidPhoneNumber(question.PhoneNumber))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "The phone number should be in the international format: +CCCXXXXXXXXXX, where +CCC is the country code and XXXXXXXXXX is the phone number."
                };
            }
            else if (!_validationService.IsValidLength(question.PhoneNumber, 20))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Phone number cannot exceed 20 characters in length."
                };
            }
            else if (!_validationService.IsValidEmail(question.Email))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }

            else if (!_validationService.IsValidLength(question.Email, 254))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address cannot exceed 254 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(question.QuestionText, 5000))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Question text cannot exceed 5000 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _ordersService.AddQuestionAsync(question);
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
