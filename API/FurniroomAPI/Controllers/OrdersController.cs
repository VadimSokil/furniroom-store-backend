using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Orders;
using FurniroomAPI.Models.Response;
using FurniroomAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Controllers
{
    [Route("orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;
        public string currentDateTime = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") + " UTC";
        public ValidationService validationService;
        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        [HttpGet("get-account-orders-list")]
        public async Task<ActionResult<GatewayResponseModel>> GetAccountOrders([FromQuery][Required] int? accountId)
        {
            if (!ModelState.IsValid)
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Your query is missing some fields."
                };
            }
            else if (!validationService.IsNotEmptyValue(accountId))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Account ID cannot be empty."
                };
            }
            else if (!validationService.IsValidDigit(accountId))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Account ID must be a positive number."
                };
            }
            else
            {
                var serviceResponse = await _ordersService.GetAccountOrdersAsync((int)accountId);
                var gatewayResponse = new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = serviceResponse.Status,
                    Message = serviceResponse.Message,
                    Data = serviceResponse.Data
                };
                return Ok(gatewayResponse);
            }

        }

        [HttpPost("add-order")]
        public async Task<ActionResult<GatewayResponseModel>> AddOrder([FromBody] OrderModel order)
        {
            if (!ModelState.IsValid)
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Your query is missing some fields."
                };
            }
            else if (!validationService.IsNotEmptyValue(order.OrderId))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Order ID cannot be empty."
                };
            }
            else if (!validationService.IsValidDigit(order.OrderId))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Order ID must be a positive number."
                };
            }
            else if (!validationService.IsNotEmptyValue(order.OrderDate))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Order date cannot be empty."
                };
            }
            else if (!validationService.IsNotEmptyValue(order.AccountId))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Account ID cannot be empty."
                };
            }
            else if (!validationService.IsValidDigit(order.AccountId))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Account ID must be a positive number."
                };
            }
            else if (!validationService.IsNotEmptyValue(order.PhoneNumber))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Phone number cannot be empty."
                };
            }
            else if (!validationService.IsValidPhoneNumber(order.PhoneNumber))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "The phone number should be in the international format: +CCCXXXXXXXXXX, where +CCC is the country code and XXXXXXXXXX is the phone number."
                };
            }
            else if (!validationService.IsValidLength(order.PhoneNumber, 20))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Phone number cannot exceed 20 characters in length."
                };
            }
            else if (!validationService.IsNotEmptyValue(order.Country))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Country cannot be empty."
                };
            }
            else if (!validationService.IsValidLength(order.Country, 100))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Country cannot exceed 100 characters in length."
                };
            }
            else if (!validationService.IsNotEmptyValue(order.Region))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Region cannot be empty."
                };
            }
            else if (!validationService.IsValidLength(order.Region, 100))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Region cannot exceed 100 characters in length."
                };
            }
            else if (!validationService.IsNotEmptyValue(order.District))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "District cannot be empty."
                };
            }
            else if (!validationService.IsValidLength(order.District, 100))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "District cannot exceed 100 characters in length."
                };
            }
            else if (!validationService.IsNotEmptyValue(order.City))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "City cannot be empty."
                };
            }
            else if (!validationService.IsValidLength(order.City, 100))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "City cannot exceed 100 characters in length."
                };
            }
            else if (!validationService.IsNotEmptyValue(order.Village))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Village cannot be empty."
                };
            }
            else if (!validationService.IsValidLength(order.Village, 100))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Village cannot exceed 100 characters in length."
                };
            }
            else if (!validationService.IsNotEmptyValue(order.Street))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Street cannot be empty."
                };
            }
            else if (!validationService.IsValidLength(order.Street, 100))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Street cannot exceed 100 characters in length."
                };
            }
            else if (!validationService.IsNotEmptyValue(order.HouseNumber))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "House number cannot be empty."
                };
            }
            else if (!validationService.IsValidLength(order.HouseNumber, 20))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "House number cannot exceed 20 characters in length."
                };
            }
            else if (!validationService.IsNotEmptyValue(order.ApartmentNumber))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Apartment number cannot be empty."
                };
            }
            else if (!validationService.IsValidLength(order.ApartmentNumber, 20))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Apartment number cannot exceed 20 characters in length."
                };
            }
            else if (!validationService.IsNotEmptyValue(order.OrderText))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Order text cannot be empty."
                };
            }
            else if (!validationService.IsValidLength(order.OrderText, 5000))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Order text cannot exceed 5000 characters in length."
                };
            }
            else if (!validationService.IsNotEmptyValue(order.DeliveryType))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Delivery type cannot be empty."
                };
            }
            else if (!validationService.IsValidLength(order.DeliveryType, 20))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Delivery type cannot exceed 20 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _ordersService.AddOrderAsync(order);
                var gatewayResponse = new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = serviceResponse.Status,
                    Message = serviceResponse.Message,
                    Data = serviceResponse.Data
                };
                return Ok(gatewayResponse);
            }
        }

        [HttpPost("add-question")]
        public async Task<ActionResult<GatewayResponseModel>> AddQuestion([FromBody] QuestionModel question)
        {
            if (!ModelState.IsValid)
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Your query is missing some fields."
                };
            }
            else if (!validationService.IsNotEmptyValue(question.QuestionId))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Question ID cannot be empty."
                };
            }
            else if (!validationService.IsValidDigit(question.QuestionId))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Question ID must be a positive number."
                };
            }
            else if (!validationService.IsNotEmptyValue(question.QuestionDate))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Question date cannot be empty."
                };
            }
            else if (!validationService.IsNotEmptyValue(question.UserName))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "User name cannot be empty."
                };
            }
            else if (!validationService.IsValidLength(question.UserName, 50))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "User name cannot exceed 50 characters in length."
                };
            }
            else if (!validationService.IsNotEmptyValue(question.PhoneNumber))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Phone number cannot be empty."
                };
            }
            else if (!validationService.IsValidPhoneNumber(question.PhoneNumber))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "The phone number should be in the international format: +CCCXXXXXXXXXX, where +CCC is the country code and XXXXXXXXXX is the phone number."
                };
            }
            else if (!validationService.IsValidLength(question.PhoneNumber, 20))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Phone number cannot exceed 20 characters in length."
                };
            }
            else if (!validationService.IsNotEmptyValue(question.Email))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Email address cannot be empty."
                };
            }
            else if (!validationService.IsValidEmail(question.Email))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }

            else if (!validationService.IsValidLength(question.Email, 254))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Email address cannot exceed 254 characters in length."
                };
            }
            else if (!validationService.IsNotEmptyValue(question.QuestionText))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Question text cannot be empty."
                };
            }
            else if (!validationService.IsValidLength(question.QuestionText, 5000))
            {
                return new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Question text cannot exceed 5000 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _ordersService.AddQuestionAsync(question);
                var gatewayResponse = new GatewayResponseModel
                {
                    Date = currentDateTime,
                    Status = serviceResponse.Status,
                    Message = serviceResponse.Message,
                    Data = serviceResponse.Data
                };

                return Ok(gatewayResponse);
            }
        }

    }
}
