using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Response;
using FurniroomAPI.Models.Log;
using Microsoft.AspNetCore.Mvc;

namespace FurniroomAPI.Controllers
{
    [Route("catalog")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogService _catalogService;
        private readonly string _requestDate;
        private readonly DateTime _logDate;
        private readonly ILoggingService _loggingService;
        private readonly string _requestId;
        private readonly HttpRequest _httpRequest;

        public CatalogController(ICatalogService catalogService, Func<DateTime> requestDate, ILoggingService loggingService, IHttpContextAccessor httpContextAccessor)
        {
            _catalogService = catalogService;
            _logDate = requestDate();
            _requestDate = requestDate().ToString("dd/MM/yyyy HH:mm:ss") + " UTC";
            _loggingService = loggingService;
            _requestId = Guid.NewGuid().ToString();
            _httpRequest = httpContextAccessor.HttpContext.Request;

        }

        [HttpGet("get-categories-list")]
        public async Task<ActionResult<APIResponseModel>> GetCategories()
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

            var serviceResponse = await _catalogService.GetAllCategoriesAsync(
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

        [HttpGet("get-subcategories-list")]
        public async Task<ActionResult<APIResponseModel>> GetSubcategories()
        {
            var log = new LogModel
            {
                Date = _logDate,
                HttpMethod = HttpContext.Request.Method,
                Endpoint = HttpContext.Request.Path,
                QueryParams = HttpContext.Request.QueryString.Value ?? string.Empty,
                Status = "Received a new request",
                RequestId = _requestId
            };
            await _loggingService.AddLogAsync(log);

            var serviceResponse = await _catalogService.GetAllSubcategoriesAsync(
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

        [HttpGet("get-sets-list")]
        public async Task<ActionResult<APIResponseModel>> GetSets()
        {
            var log = new LogModel
            {
                Date = _logDate,
                HttpMethod = HttpContext.Request.Method,
                Endpoint = HttpContext.Request.Path,
                QueryParams = HttpContext.Request.QueryString.Value ?? string.Empty,
                Status = "Received a new request",
                RequestId = _requestId
            };
            await _loggingService.AddLogAsync(log);

            var serviceResponse = await _catalogService.GetAllSetsAsync(
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

        [HttpGet("get-images-list")]
        public async Task<ActionResult<APIResponseModel>> GetImages()
        {
            var log = new LogModel
            {
                Date = _logDate,
                HttpMethod = HttpContext.Request.Method,
                Endpoint = HttpContext.Request.Path,
                QueryParams = HttpContext.Request.QueryString.Value ?? string.Empty,
                Status = "Received a new request",
                RequestId = _requestId
            };
            await _loggingService.AddLogAsync(log);

            var serviceResponse = await _catalogService.GetAllImagesAsync(
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

        [HttpGet("get-modules-list")]
        public async Task<ActionResult<APIResponseModel>> GetModules()
        {
            var log = new LogModel
            {
                Date = _logDate,
                HttpMethod = HttpContext.Request.Method,
                Endpoint = HttpContext.Request.Path,
                QueryParams = HttpContext.Request.QueryString.Value ?? string.Empty,
                Status = "Received a new request",
                RequestId = _requestId
            };
            await _loggingService.AddLogAsync(log);

            var serviceResponse = await _catalogService.GetAllModulesAsync(
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
