using AuthorizationService.Interfaces;
using AuthorizationService.Models.Authorization;
using AuthorizationService.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationService.Controllers
{
    [Route("authorization")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        public AuthorizationController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HttpPost("sign-up")]
        public async Task<ActionResult<ServiceResponseModel>> Register([FromBody] SignUpModel signUp)
        {
            var result = await _authorizationService.SignUpAsync(signUp);
            return Ok(result);
        }

        [HttpPost("sign-in")]
        public async Task<ActionResult<ServiceResponseModel>> Login([FromBody] SignInModel signIn)
        {
            var result = await _authorizationService.SignInAsync(signIn);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ServiceResponseModel>> ResetPassword([FromBody] string email)
        {
            var result = await _authorizationService.ResetPasswordAsync(email);
            return Ok(result);
        }

        [HttpGet("check-email")]
        public async Task<ActionResult<ServiceResponseModel>> CheckEmail([FromQuery] string email)
        {
            var result = await _authorizationService.CheckEmailAsync(email);
            return Ok(result);
        }

        [HttpGet("generate-vertification-code")]
        public async Task<ActionResult<ServiceResponseModel>> GenerateCode([FromQuery] string email)
        {
            var result = await _authorizationService.GenerateCodeAsync(email);
            return Ok(result);
        }

    }
}
