using FurniroomAPI.Models.Account;
using FurniroomAPI.Models.Response;

namespace FurniroomAPI.Interfaces
{
    public interface IAccountService
    {
        public Task<ServiceResponseModel> GetAccountInformationAsync(int accountId, string httpMethod, string endpoint, string queryParams, string requestId);
        public Task<ServiceResponseModel> ChangeNameAsync(ChangeNameModel changeName, string httpMethod, string endpoint, string queryParams, string requestId);
        public Task<ServiceResponseModel> ChangeEmailAsync(ChangeEmailModel changeEmail, string httpMethod, string endpoint, string queryParams, string requestId);
        public Task<ServiceResponseModel> ChangePasswordAsync(ChangePasswordModel changePassword, string httpMethod, string endpoint, string queryParams, string requestId);
        public Task<ServiceResponseModel> DeleteAccountAsync(int accountId, string httpMethod, string endpoint, string queryParams, string requestId);
    }
}
