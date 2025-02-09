using FurniroomAPI.Models.Authorization;
using FurniroomAPI.Models.Response;

namespace FurniroomAPI.Interfaces
{
    public interface IAuthorizationService
    {
        public Task<ServiceResponseModel> CheckEmailAsync(string email);
        public Task<ServiceResponseModel> GenerateCodeAsync(string email);
        public Task<ServiceResponseModel> ResetPasswordAsync(string email);
        public Task<ServiceResponseModel> SignUpAsync(SignUpModel signUp);
        public Task<ServiceResponseModel> SignInAsync(SignInModel signIn);

    }
}
