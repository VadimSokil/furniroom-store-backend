using FurniroomAPI.Models.Response;

namespace FurniroomAPI.Interfaces
{
    public interface IConditionsService
    {
        public Task<ServiceResponseModel> GetDeliveryConditionsAsync(string httpMethod, string endpoint, string queryParams, string requestId);
        public Task<ServiceResponseModel> GetPaymentConditionsAsync(string httpMethod, string endpoint, string queryParams, string requestId);

    }
}
