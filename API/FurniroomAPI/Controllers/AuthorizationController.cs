using FurniroomAPI.Interfaces;
using FurniroomAPI.Models;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace FurniroomAPI.Controllers
{
    [Route("authorization")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILoggingService _loggingService;
        private readonly HttpRequest _httpRequest;

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
            Func<T, string, Task<ServiceResponseModel>> serviceCall,
            Func<T, string> getQueryParams)
        {
            var requestId = Guid.NewGuid().ToString();
            var formattedTime = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") + " UTC";
            var queryParams = getQueryParams(requestData);

            await LogActionAsync("Request started", queryParams, requestId);

            if (!ModelState.IsValid)
            {
                return await HandleValidationError("Invalid request structure", queryParams, requestId, formattedTime);
            }

            var serviceResponse = await serviceCall(requestData, requestId);
            var gatewayResponse = new APIResponseModel
            {
                Date = formattedTime,
                Status = serviceResponse.Status,
                Message = serviceResponse.Message,
                Data = serviceResponse.Data
            };

            await LogActionAsync("Request completed", queryParams, requestId);
            return Ok(gatewayResponse);
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

        private async Task<ActionResult<APIResponseModel>> HandleValidationError(
            string message,
            string queryParams,
            string requestId,
            string formattedTime)
        {
            await LogActionAsync(message, queryParams, requestId);
            return new APIResponseModel
            {
                Date = formattedTime,
                Status = false,
                Message = message,
                Data = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrEmpty(m))
            };
        }

        [HttpPost("sign-up")]
        public async Task<ActionResult<APIResponseModel>> SignUp([FromBody] SignUpModel signUp)
        {
            return await ProcessRequest(
                signUp,
                (data, requestId) => _authorizationService.SignUpAsync(
                    data,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    JsonSerializer.Serialize(data),
                    requestId),
                data => JsonSerializer.Serialize(data));
        }

        [HttpPost("sign-in")]
        public async Task<ActionResult<APIResponseModel>> SignIn([FromBody] SignInModel signIn)
        {
            return await ProcessRequest(
                signIn,
                (data, requestId) => _authorizationService.SignInAsync(
                    data,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    JsonSerializer.Serialize(data),
                    requestId),
                data => JsonSerializer.Serialize(data));
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<APIResponseModel>> ResetPassword([FromBody] EmailModel email)
        {
            return await ProcessRequest(
                email,
                (data, requestId) => _authorizationService.ResetPasswordAsync(
                    data.Email,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    string.Empty,
                    requestId),
                data => string.Empty);
        }

        [HttpGet("check-email")]
        public async Task<ActionResult<APIResponseModel>> CheckEmail([FromQuery] EmailModel email)
        {
            return await ProcessRequest(
                email,
                (data, requestId) => _authorizationService.CheckEmailAsync(
                    data.Email,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    $"email={WebUtility.UrlEncode(data.Email)}",
                    requestId),
                data => $"email={WebUtility.UrlEncode(data.Email)}");
        }

        [HttpGet("generate-verification-code")]
        public async Task<ActionResult<APIResponseModel>> GenerateCode([FromQuery] EmailModel email)
        {
            return await ProcessRequest(
                email,
                (data, requestId) => _authorizationService.GenerateCodeAsync(
                    data.Email,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    string.Empty,
                    requestId),
                data => string.Empty);
        }
    }
}