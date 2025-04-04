using FurniroomAPI.Models.Account;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Response;

namespace FurniroomAPI.Interfaces
{
    public interface IAccountService
    {
        public Task<ServiceResponseModel> GetAccountInformationAsync(int accountId, TransferLogModel transfer);
        public Task<ServiceResponseModel> ChangeNameAsync(ChangeNameModel changeName, TransferLogModel transfer);
        public Task<ServiceResponseModel> ChangeEmailAsync(ChangeEmailModel changeEmail, TransferLogModel transfer);
        public Task<ServiceResponseModel> ChangePasswordAsync(ChangePasswordModel changePassword, TransferLogModel transfer);
        public Task<ServiceResponseModel> DeleteAccountAsync(int accountId, TransferLogModel transfer);
    }
}
