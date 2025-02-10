using AccountService.Models.Account;
using AccountService.Models.Response;

namespace AccountService.Interfaces
{
    public interface IAccountService
    {
        public Task<ServiceResponseModel> GetAccountInformationAsync(int accountId);
        public Task<ServiceResponseModel> ChangeNameAsync(ChangeNameModel changeName);
        public Task<ServiceResponseModel> ChangeEmailAsync(ChangeEmailModel changeEmail);
        public Task<ServiceResponseModel> ChangePasswordAsync(ChangePasswordModel changePassword);
        public Task<ServiceResponseModel> DeleteAccountAsync(int accountId);
    }
}
