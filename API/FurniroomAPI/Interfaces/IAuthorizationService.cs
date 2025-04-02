using FurniroomAPI.Models.Authorization;
using FurniroomAPI.Models.Response;

namespace FurniroomAPI.Interfaces
{
    public interface IAuthorizationService
    {
        public Task<ServiceResponseModel> CheckEmailAsync(string email, string httpMethod, string endpoint, string queryParams, string requestId);
        public Task<ServiceResponseModel> GenerateCodeAsync(string email, string httpMethod, string endpoint, string queryParams, string requestId);
        public Task<ServiceResponseModel> ResetPasswordAsync(string email, string httpMethod, string endpoint, string queryParams, string requestId);
        public Task<ServiceResponseModel> SignUpAsync(SignUpModel signUp, string httpMethod, string endpoint, string queryParams, string requestId);
        public Task<ServiceResponseModel> SignInAsync(SignInModel signIn, string httpMethod, string endpoint, string queryParams, string requestId);

    }
}
