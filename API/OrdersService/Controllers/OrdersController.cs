using Microsoft.AspNetCore.Mvc;
using OrdersService.Interfaces;
using OrdersService.Models.Orders;
using OrdersService.Models.Response;

namespace OrdersService.Controllers
{
    [Route("orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;
        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        [HttpGet("get-account-orders-list")]
        public async Task<ActionResult<ServiceResponseModel>> GetAccountOrders([FromQuery] int accountId)
        {
            var result = await _ordersService.GetAccountOrdersAsync(accountId);
            return Ok(result);
        }

        [HttpPost("add-order")]
        public async Task<ActionResult<ServiceResponseModel>> AddOrder([FromBody] OrderModel order)
        {
            var result = await _ordersService.AddOrderAsync(order);
            return Ok(result);
        }

        [HttpPost("add-question")]
        public async Task<ActionResult<ServiceResponseModel>> AddQuestion([FromBody] QuestionModel question)
        {
            var result = await _ordersService.AddQuestionAsync(question);
            return Ok(result);
        }
    }
}
