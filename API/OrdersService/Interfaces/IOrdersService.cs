using OrdersService.Models.Orders;
using OrdersService.Models.Response;

namespace OrdersService.Interfaces
{
    public interface IOrdersService
    {
        public Task<ServiceResponseModel> GetAccountOrdersAsync(int accountId);
        public Task<ServiceResponseModel> AddOrderAsync(OrderModel order);
        public Task<ServiceResponseModel> AddQuestionAsync(QuestionModel question);

    }
}
