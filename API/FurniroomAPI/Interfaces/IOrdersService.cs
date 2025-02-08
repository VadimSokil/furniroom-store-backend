using FurniroomAPI.Models.Orders;
using FurniroomAPI.Models.Response;

namespace FurniroomAPI.Interfaces
{
    public interface IOrdersService
    {
        public Task<ServiceResponseModel> GetAccountOrdersAsync(int accountId);
        public Task<ServiceResponseModel> AddOrderAsync(OrderModel order);
        public Task<ServiceResponseModel> AddQuestionAsync(QuestionModel question);
    }
}
