using FurniroomAPI.Models.Response;

namespace FurniroomAPI.Interfaces
{
    public interface IConditionsService
    {
        public Task<ServiceResponseModel> GetDeliveryConditionsAsync();
        public Task<ServiceResponseModel> GetPaymentConditionsAsync();

    }
}
