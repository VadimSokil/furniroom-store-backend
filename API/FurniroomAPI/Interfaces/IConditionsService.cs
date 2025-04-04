using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Response;

namespace FurniroomAPI.Interfaces
{
    public interface IConditionsService
    {
        public Task<ServiceResponseModel> GetDeliveryConditionsAsync(TransferLogModel transfer);
        public Task<ServiceResponseModel> GetPaymentConditionsAsync(TransferLogModel transfer);

    }
}
