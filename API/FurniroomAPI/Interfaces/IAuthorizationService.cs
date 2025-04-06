using FurniroomAPI.Models.Authorization;
using FurniroomAPI.Models.Response;
using FurniroomAPI.Models.Log;

namespace FurniroomAPI.Interfaces
{
    public interface IAuthorizationService
    {
        public Task<ServiceResponseModel> CheckEmailAsync(string email, TransferLogModel transfer);
        public Task<ServiceResponseModel> CheckNameAsync(string name, TransferLogModel transfer);
        public Task<ServiceResponseModel> GenerateCodeAsync(string email, TransferLogModel transfer);
        public Task<ServiceResponseModel> ResetPasswordAsync(string email, TransferLogModel transfer);
        public Task<ServiceResponseModel> SignUpAsync(SignUpModel signUp, TransferLogModel transfer);
        public Task<ServiceResponseModel> SignInAsync(SignInModel signIn, TransferLogModel transfer);
    }
}