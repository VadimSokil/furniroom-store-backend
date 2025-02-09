using AuthorizationService.Models.Response;
using AuthorizationService.Models.Authorization;

namespace AuthorizationService.Interfaces
{
    public interface IAuthorizationService
    {
        public Task<ServiceResponseModel> SignUpAsync(SignUpModel signUp);
        public Task<ServiceResponseModel> SignInAsync(SignInModel signIn);
        public Task<ServiceResponseModel> CheckEmailAsync(string email);
        public Task<ServiceResponseModel> GenerateCodeAsync(string email);
        public Task<ServiceResponseModel> ResetPasswordAsync(string email);
    }
}
