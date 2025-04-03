using FurniroomAPI.Interfaces;
using FurniroomAPI.Models;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FurniroomAPI.Controllers
{
    [Route("authorization")]
    [ApiController]
    [Produces("application/json")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILoggingService _loggingService;
        private readonly HttpRequest _httpRequest;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public AuthorizationController(
            IAuthorizationService authorizationService,
            ILoggingService loggingService,
            IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _loggingService = loggingService;
            _httpRequest = httpContextAccessor.HttpContext.Request;
        }

        private async Task<ActionResult<APIResponseModel>> ProcessRequest<T>(
            T requestData,
            JsonElement rawJson,
            Func<T, string, Task<ServiceResponseModel>> serviceCall,
            Func<T, string> getQueryParams) where T : StrictValidationModel
        {
            var requestId = Guid.NewGuid().ToString();
            var formattedTime = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") + " UTC";

            var strictErrors = requestData.ValidateStrict(rawJson);
            if (strictErrors?.Count > 0)
            {
                await LogActionAsync($"Validation failed: {string.Join(", ", strictErrors)}",
                                   rawJson.ToString(), requestId);
                return BadRequest(new APIResponseModel
                {
                    Date = formattedTime,
                    Status = false,
                    Message = "Validation error",
                    Data = strictErrors
                });
            }

            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrEmpty(m))
                    .ToList();

                await LogActionAsync($"Validation failed: {string.Join(", ", modelErrors)}",
                                   rawJson.ToString(), requestId);
                return BadRequest(new APIResponseModel
                {
                    Date = formattedTime,
                    Status = false,
                    Message = "Validation error",
                    Data = modelErrors
                });
            }

            var queryParams = getQueryParams(requestData);
            await LogActionAsync("Request started", queryParams, requestId);

            try
            {
                var serviceResponse = await serviceCall(requestData, requestId);
                await LogActionAsync("Request completed", queryParams, requestId);

                return Ok(new APIResponseModel
                {
                    Date = formattedTime,
                    Status = serviceResponse.Status,
                    Message = serviceResponse.Message,
                    Data = serviceResponse.Data
                });
            }
            catch (Exception ex)
            {
                await LogActionAsync($"Error: {ex.Message}", queryParams, requestId);
                return StatusCode(500, new APIResponseModel
                {
                    Date = formattedTime,
                    Status = false,
                    Message = "Internal server error",
                    Data = ex.Message
                });
            }
        }

        private async Task LogActionAsync(string status, string queryParams, string requestId)
        {
            await _loggingService.AddLogAsync(new LogModel
            {
                Date = DateTime.UtcNow,
                HttpMethod = _httpRequest.Method,
                Endpoint = _httpRequest.Path,
                QueryParams = queryParams,
                Status = status,
                RequestId = requestId
            });
        }

        [HttpPost("sign-up")]
        public async Task<ActionResult<APIResponseModel>> SignUp()
        {
            using var doc = await JsonDocument.ParseAsync(Request.Body);
            var request = JsonSerializer.Deserialize<SignUpModel>(doc.RootElement, _jsonOptions);
            return await ProcessRequest(
                request,
                doc.RootElement,
                (data, requestId) => _authorizationService.SignUpAsync(
                    data,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    JsonSerializer.Serialize(data, _jsonOptions),
                    requestId),
                data => JsonSerializer.Serialize(data, _jsonOptions));
        }

        [HttpPost("sign-in")]
        public async Task<ActionResult<APIResponseModel>> SignIn()
        {
            using var doc = await JsonDocument.ParseAsync(Request.Body);
            var request = JsonSerializer.Deserialize<SignInModel>(doc.RootElement, _jsonOptions);
            return await ProcessRequest(
                request,
                doc.RootElement,
                (data, requestId) => _authorizationService.SignInAsync(
                    data,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    JsonSerializer.Serialize(data, _jsonOptions),
                    requestId),
                data => JsonSerializer.Serialize(data, _jsonOptions));
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<APIResponseModel>> ResetPassword()
        {
            using var doc = await JsonDocument.ParseAsync(Request.Body);
            var request = JsonSerializer.Deserialize<EmailRequest>(doc.RootElement, _jsonOptions);
            return await ProcessRequest(
                request,
                doc.RootElement,
                (data, requestId) => _authorizationService.ResetPasswordAsync(
                    data.Email,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    JsonSerializer.Serialize(data, _jsonOptions),
                    requestId),
                data => JsonSerializer.Serialize(data, _jsonOptions));
        }

        [HttpGet("check-email")]
        public async Task<ActionResult<APIResponseModel>> CheckEmail([FromQuery] string email)
        {
            var request = new EmailRequest { Email = email };
            var jsonElement = JsonSerializer.SerializeToElement(request, _jsonOptions);
            return await ProcessRequest(
                request,
                jsonElement,
                (data, requestId) => _authorizationService.CheckEmailAsync(
                    data.Email,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    JsonSerializer.Serialize(data, _jsonOptions),
                    requestId),
                data => JsonSerializer.Serialize(data, _jsonOptions));
        }

        [HttpGet("generate-verification-code")]
        public async Task<ActionResult<APIResponseModel>> GenerateCode([FromQuery] string email)
        {
            var request = new EmailRequest { Email = email };
            var jsonElement = JsonSerializer.SerializeToElement(request, _jsonOptions);
            return await ProcessRequest(
                request,
                jsonElement,
                (data, requestId) => _authorizationService.GenerateCodeAsync(
                    data.Email,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    JsonSerializer.Serialize(data, _jsonOptions),
                    requestId),
                data => JsonSerializer.Serialize(data, _jsonOptions));
        }
    }
}