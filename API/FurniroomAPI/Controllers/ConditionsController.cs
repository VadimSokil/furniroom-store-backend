using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace FurniroomAPI.Controllers
{
    [Route("conditions")]
    [ApiController]
    public class ConditionsController : ControllerBase
    {
        private readonly IConditionsService _conditionsService;
        private readonly string _requestDate;

        public ConditionsController(IConditionsService conditionsService, Func<DateTime> requestDate)
        {
            _conditionsService = conditionsService;
            _requestDate = requestDate().ToString("dd/MM/yyyy HH:mm:ss") + " UTC";
        }

        [HttpGet("get-delivery-conditions")]
        public async Task<ActionResult<string>> GetDelivery()
        {
            var serviceResponse = await _conditionsService.GetDeliveryConditionsAsync();
            var gatewayResponse = new APIResponseModel
            {
                Date = _requestDate,
                Status = serviceResponse.Status,
                Message = serviceResponse.Message,
                Data = serviceResponse.Data
            };
            return Ok(gatewayResponse);
        }

        [HttpGet("get-payment-conditions")]
        public async Task<ActionResult<string>> GetPayment()
        {
            var serviceResponse = await _conditionsService.GetPaymentConditionsAsync();
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
