using InformationService.Models.Response;

namespace InformationService.Interfaces
{
    public interface IConditionsService
    {
        public Task<ServiceResponseModel> GetDeliveryConditionsAsync();
        public Task<ServiceResponseModel> GetPaymentConditionsAsync();
    }
}
