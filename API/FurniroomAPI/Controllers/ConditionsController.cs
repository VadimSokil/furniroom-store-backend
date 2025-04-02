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
        private readonly string _requestDate;
        private readonly DateTime _logDate;
        private readonly ILoggingService _loggingService;
        private readonly string _requestId;
        private readonly HttpRequest _httpRequest;

        public ConditionsController(IConditionsService conditionsService, Func<DateTime> requestDate, ILoggingService loggingService, IHttpContextAccessor httpContextAccessor)
        {
            _conditionsService = conditionsService;
            _logDate = requestDate();
            _requestDate = requestDate().ToString("dd/MM/yyyy HH:mm:ss") + " UTC";
            _loggingService = loggingService;
            _requestId = Guid.NewGuid().ToString();
            _httpRequest = httpContextAccessor.HttpContext.Request;
        }

        [HttpGet("get-delivery-conditions")]
        public async Task<ActionResult<string>> GetDelivery()
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

            var serviceResponse = await _conditionsService.GetDeliveryConditionsAsync(
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

        [HttpGet("get-payment-conditions")]
        public async Task<ActionResult<string>> GetPayment()
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

            var serviceResponse = await _conditionsService.GetPaymentConditionsAsync(
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
