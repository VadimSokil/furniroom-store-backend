using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Authorization;
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
        public AuthorizationController(IAuthorizationService authorizationService, Func<DateTime> requestDate, IValidationService validationService)
        {
            _authorizationService = authorizationService;
            _requestDate = requestDate().ToString("dd/MM/yyyy HH:mm:ss") + " UTC";
            _validationService = validationService;
        }

        [HttpPost("sign-up")]
        public async Task<ActionResult<APIResponseModel>> SignUp([FromBody] SignUpModel signUp)
        {
            if (!ModelState.IsValid)
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidDigit(signUp.AccountId))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Account ID must be a positive number."
                };
            }
            else if (!_validationService.IsValidLength(signUp.AccountName, 50))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Account name cannot exceed 50 characters in length."
                };
            }
            else if (!_validationService.IsValidEmail(signUp.Email))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }
            else if (!_validationService.IsValidLength(signUp.Email, 254))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address cannot exceed 254 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(signUp.PasswordHash, 128))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Password hash cannot exceed 128 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _authorizationService.SignUpAsync(signUp);
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
            if (!ModelState.IsValid)
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidEmail(signIn.Email))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }
            else if (!_validationService.IsValidLength(signIn.Email, 254))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address cannot exceed 254 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(signIn.PasswordHash, 128))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Password hash cannot exceed 128 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _authorizationService.SignInAsync(signIn);
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
            if (!ModelState.IsValid)
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidEmail(email))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }
            else if (!_validationService.IsValidLength(email, 254))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address cannot exceed 254 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _authorizationService.ResetPasswordAsync(email);
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
            if (!ModelState.IsValid)
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidEmail(email))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }
            else if (!_validationService.IsValidLength(email, 254))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address cannot exceed 254 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _authorizationService.CheckEmailAsync(email);
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
            if (!ModelState.IsValid)
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidEmail(email))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }
            else if (!_validationService.IsValidLength(email, 254))
            {
                return new APIResponseModel
                {
                    Date = _requestDate,
                    Status = false,
                    Message = "Email address cannot exceed 254 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _authorizationService.GenerateCodeAsync(email);
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
