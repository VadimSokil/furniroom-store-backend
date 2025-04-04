using FurniroomAPI.Models.Orders;
using FurniroomAPI.Models.Response;
using FurniroomAPI.Models.Log;

namespace FurniroomAPI.Interfaces
{
    public interface IOrdersService
    {
        public Task<ServiceResponseModel> GetAccountOrdersAsync(int accountId, TransferLogModel transfer);
        public Task<ServiceResponseModel> AddOrderAsync(OrderModel order, TransferLogModel transfer);
        public Task<ServiceResponseModel> AddQuestionAsync(QuestionModel question, TransferLogModel transfer);
    }
}
