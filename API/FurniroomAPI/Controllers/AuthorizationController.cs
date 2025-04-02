using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Authorization;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Controllers
{
    [Route("authorization")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IValidationService _validationService;
        private readonly string _requestDate;
        private readonly DateTime _logDate;
        private readonly ILoggingService _loggingService;
        private readonly string _requestId;
        private readonly HttpRequest _httpRequest;

        public AuthorizationController(IAuthorizationService authorizationService, IValidationService validationService, Func<DateTime> requestDate, ILoggingService loggingService, IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _validationService = validationService;
            _logDate = requestDate();
            _requestDate = requestDate().ToString("dd/MM/yyyy HH:mm:ss") + " UTC";
            _loggingService = loggingService;
            _requestId = Guid.NewGuid().ToString();
            _httpRequest = httpContextAccessor.HttpContext.Request;
        }

        [HttpPost("sign-up")]
        public async Task<ActionResult<APIResponseModel>> SignUp([FromBody] SignUpModel signUp)
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

            if (!ModelState.IsValid)
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Structure of your request is different from what the server expects or has empty fields.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidDigit(signUp.AccountId))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Account ID must be a positive number.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Account ID must be a positive number."
                };
            }
            else if (!_validationService.IsValidLength(signUp.AccountName, 50))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Account name cannot exceed 50 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Account name cannot exceed 50 characters in length."
                };
            }
            else if (!_validationService.IsValidEmail(signUp.Email))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }
            else if (!_validationService.IsValidLength(signUp.Email, 254))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Email address cannot exceed 254 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address cannot exceed 254 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(signUp.PasswordHash, 128))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Password hash cannot exceed 128 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Password hash cannot exceed 128 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _authorizationService.SignUpAsync(
                    signUp,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    _httpRequest.QueryString.Value ?? string.Empty,
                    _requestId);
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

        [HttpPost("sign-in")]
        public async Task<ActionResult<APIResponseModel>> SignIn([FromBody] SignInModel signIn)
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

            if (!ModelState.IsValid)
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Structure of your request is different from what the server expects or has empty fields.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidEmail(signIn.Email))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }
            else if (!_validationService.IsValidLength(signIn.Email, 254))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Email address cannot exceed 254 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address cannot exceed 254 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(signIn.PasswordHash, 128))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Password hash cannot exceed 128 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Password hash cannot exceed 128 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _authorizationService.SignInAsync(
                    signIn,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    _httpRequest.QueryString.Value ?? string.Empty,
                    _requestId);
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

        [HttpPost("reset-password")]
        public async Task<ActionResult<APIResponseModel>> ResetPassword([FromBody][Required] string? email)
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

            if (!ModelState.IsValid)
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Structure of your request is different from what the server expects or has empty fields.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidEmail(email))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }
            else if (!_validationService.IsValidLength(email, 254))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Email address cannot exceed 254 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address cannot exceed 254 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _authorizationService.ResetPasswordAsync(
                    email,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    _httpRequest.QueryString.Value ?? string.Empty,
                    _requestId);
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

        [HttpGet("check-email")]
        public async Task<ActionResult<APIResponseModel>> CheckEmail([FromQuery][Required] string? email)
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

            if (!ModelState.IsValid)
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Structure of your request is different from what the server expects or has empty fields.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidEmail(email))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }
            else if (!_validationService.IsValidLength(email, 254))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Email address cannot exceed 254 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address cannot exceed 254 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _authorizationService.CheckEmailAsync(
                    email,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    _httpRequest.QueryString.Value ?? string.Empty,
                    _requestId);
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

        [HttpGet("generate-vertification-code")]
        public async Task<ActionResult<APIResponseModel>> GenerateCode([FromQuery][Required] string? email)
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

            if (!ModelState.IsValid)
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Structure of your request is different from what the server expects or has empty fields.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidEmail(email))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }
            else if (!_validationService.IsValidLength(email, 254))
            {
                var error = new LogModel
                {
                    Date = _logDate,
                    HttpMethod = _httpRequest.Method,
                    Endpoint = _httpRequest.Path,
                    QueryParams = _httpRequest.QueryString.Value ?? string.Empty,
                    Status = "Email address cannot exceed 254 characters in length.",
                    RequestId = _requestId
                };

                await _loggingService.AddLogAsync(error);

                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address cannot exceed 254 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _authorizationService.GenerateCodeAsync(
                    email,
                    _httpRequest.Method,
                    _httpRequest.Path,
                    _httpRequest.QueryString.Value ?? string.Empty,
                    _requestId);
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
