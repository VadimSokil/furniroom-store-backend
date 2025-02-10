using FurniroomAPI.Models.Account;
using FurniroomAPI.Models.Response;

namespace FurniroomAPI.Interfaces
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
