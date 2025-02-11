using InformationService.Interfaces;
using InformationService.Models.Conditions;
using Microsoft.AspNetCore.Mvc;

namespace InformationService.Controllers
{
    [Route("conditions")]
    [ApiController]
    public class ConditionsController : ControllerBase
    {
        private readonly IConditionsService _conditionsService;
        public ConditionsController(IConditionsService conditionsService)
        {
            _conditionsService = conditionsService;
        }

        [HttpGet("get-delivery-conditions")]
        public async Task<ActionResult<List<ConditionsModel>>> GetDeliveryInformation()
        {
            var result = await _conditionsService.GetDeliveryConditionsAsync();
            return Ok(result);
        }

        [HttpGet("get-payment-conditions")]
        public async Task<ActionResult<List<ConditionsModel>>> GetPaymentInformation()
        {
            var result = await _conditionsService.GetPaymentConditionsAsync();
            return Ok(result);
        }

    }
}
