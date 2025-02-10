using AccountService.Interfaces;
using AccountService.Models.Account;
using AccountService.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Controllers
{
    [Route("account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("get-account-information")]
        public async Task<ActionResult<ServiceResponseModel>> GetAccountInformation([FromQuery] int accountId)
        {

            var result = await _accountService.GetAccountInformationAsync(accountId);
            return Ok(result);

        }

        [HttpPut("change-name")]
        public async Task<ActionResult<ServiceResponseModel>> ChangeName([FromBody] ChangeNameModel changeName)
        {
            var result = await _accountService.ChangeNameAsync(changeName);
            return Ok(result);
        }

        [HttpPut("change-email")]
        public async Task<ActionResult<ServiceResponseModel>> ChangeEmail([FromBody] ChangeEmailModel changeEmail)
        {
            var result = await _accountService.ChangeEmailAsync(changeEmail);
            return Ok(result);
        }

        [HttpPut("change-password")]
        public async Task<ActionResult<ServiceResponseModel>> ChangePassword([FromBody] ChangePasswordModel changePassword)
        {
            var result = await _accountService.ChangePasswordAsync(changePassword);
            return Ok(result);
        }

        [HttpDelete("delete-account")]
        public async Task<ActionResult<ServiceResponseModel>> DeleteAccount([FromQuery] int accountId)
        {
            var result = await _accountService.DeleteAccountAsync(accountId);
            return Ok(result);
        }
    }
}
