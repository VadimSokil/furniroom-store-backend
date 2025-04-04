using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace FurniroomAPI.Controllers
{
    [Route("conditions")]
    [ApiController]
    public class ConditionsController : ControllerBase
    {
        private readonly IConditionsService _conditionsService;
        private readonly ILoggingService _loggingService;
        private readonly HttpRequest _httpRequest;

        public ConditionsController(IConditionsService conditionsService, ILoggingService loggingService, IHttpContextAccessor httpContextAccessor)
        {
            _conditionsService = conditionsService;
            _loggingService = loggingService;
            _httpRequest = httpContextAccessor.HttpContext.Request;
        }

        private async Task<ActionResult<APIResponseModel>> ProcessRequest(Func<TransferLogModel, Task<ServiceResponseModel>> serviceCall)
        {
            var requestId = Guid.NewGuid().ToString();
            var formattedTime = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") + " UTC";

            var transfer = new TransferLogModel
            {
                HttpMethod = _httpRequest.Method,
                Endpoint = _httpRequest.Path,
                QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                RequestId = requestId
            };

            await LogActionAsync("Request started", transfer);

            var serviceResponse = await serviceCall(transfer);

            await LogActionAsync("Request completed", transfer);

            return Ok(new APIResponseModel
            {
                Date = formattedTime,
                Status = serviceResponse.Status,
                Message = serviceResponse.Message,
                Data = serviceResponse.Data
            });
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

        [HttpGet("get-delivery-conditions")]
        public async Task<ActionResult<APIResponseModel>> GetDelivery()
        {
            return await ProcessRequest(
                transfer => _conditionsService.GetDeliveryConditionsAsync(transfer));
        }

        [HttpGet("get-payment-conditions")]
        public async Task<ActionResult<APIResponseModel>> GetPayment()
        {
            return await ProcessRequest(
                transfer => _conditionsService.GetPaymentConditionsAsync(transfer));
        }
    }
}