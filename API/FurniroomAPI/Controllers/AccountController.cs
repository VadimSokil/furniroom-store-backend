using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Account;
using FurniroomAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Controllers
{
    [Route("account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IValidationService _validationService;
        public string currentDateTime = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") + " UTC";
        public AccountController(IAccountService accountService, IValidationService validationService)
        {
            _accountService = accountService;
            _validationService = validationService;
        }

        [HttpGet("get-account-information")]
        public async Task<ActionResult<APIResponseModel>> AccountInformation([FromQuery][Required] int? accountId)
        {
            if (!ModelState.IsValid)
            {
                return new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidDigit(accountId))
            {
                return new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Account ID must be a positive number."
                };
            }
            else
            {
                var serviceResponse = await _accountService.GetAccountInformationAsync((int)accountId);
                var gatewayResponse = new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = serviceResponse.Status,
                    Message = serviceResponse.Message,
                    Data = serviceResponse.Data
                };
                return Ok(gatewayResponse);
            }
        }

        [HttpPut("change-name")]
        public async Task<ActionResult<APIResponseModel>> ChangeName([FromBody] ChangeNameModel changeName)
        {
            if (!ModelState.IsValid)
            {
                return new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidLength(changeName.OldName, 50))
            {
                return new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Old name cannot exceed 50 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(changeName.NewName, 50))
            {
                return new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "New name cannot exceed 50 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _accountService.ChangeNameAsync(changeName);
                var gatewayResponse = new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = serviceResponse.Status,
                    Message = serviceResponse.Message,
                    Data = serviceResponse.Data
                };
                return Ok(gatewayResponse);
            }
        }

        [HttpPut("change-email")]
        public async Task<ActionResult<APIResponseModel>> ChangeEmail([FromBody] ChangeEmailModel changeEmail)
        {
            if (!ModelState.IsValid)
            {
                return new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidEmail(changeEmail.OldEmail))
            {
                return new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Old email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }
            else if (!_validationService.IsValidEmail(changeEmail.NewEmail))
            {
                return new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "New email address should be in the format: example@domain.com, where example is the username and domain.com is the domain."
                };
            }
            else if (!_validationService.IsValidLength(changeEmail.OldEmail, 254))
            {
                return new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Old email address cannot exceed 254 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(changeEmail.NewEmail, 254))
            {
                return new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "New email address cannot exceed 254 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _accountService.ChangeEmailAsync(changeEmail);
                var gatewayResponse = new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = serviceResponse.Status,
                    Message = serviceResponse.Message,
                    Data = serviceResponse.Data
                };
                return Ok(gatewayResponse);
            }
        }

        [HttpPut("change-password")]
        public async Task<ActionResult<APIResponseModel>> ChangePassword([FromBody] ChangePasswordModel changePassword)
        {
            if (!ModelState.IsValid)
            {
                return new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Some fields of the request are missing or empty."
                };
            }
            else if (!_validationService.IsValidLength(changePassword.OldPasswordHash, 128))
            {
                return new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Old password hash cannot exceed 128 characters in length."
                };
            }
            else if (!_validationService.IsValidLength(changePassword.NewPasswordHash, 128))
            {
                return new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "New password hash cannot exceed 128 characters in length."
                };
            }
            else
            {
                var serviceResponse = await _accountService.ChangePasswordAsync(changePassword);
                var gatewayResponse = new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = serviceResponse.Status,
                    Message = serviceResponse.Message,
                    Data = serviceResponse.Data
                };
                return Ok(gatewayResponse);
            }
        }

        [HttpDelete("delete-account")]
        public async Task<ActionResult<APIResponseModel>> DeleteAccount([FromQuery][Required] int? accountId)
        {
            if (!ModelState.IsValid)
            {
                return new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Structure of your request is different from what the server expects or has empty fields."
                };
            }
            else if (!_validationService.IsValidDigit(accountId))
            {
                return new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = false,
                    Message = "Account ID must be a positive number."
                };
            }
            else
            {
                var serviceResponse = await _accountService.DeleteAccountAsync((int)accountId);
                var gatewayResponse = new APIResponseModel
                {
                    Date = currentDateTime,
                    Status = serviceResponse.Status,
                    Message = serviceResponse.Message,
                    Data = serviceResponse.Data
                };
                return Ok(gatewayResponse);
            }
        }
    }
}
