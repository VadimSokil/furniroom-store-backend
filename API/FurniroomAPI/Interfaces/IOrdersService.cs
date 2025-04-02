using FurniroomAPI.Models.Orders;
using FurniroomAPI.Models.Response;

namespace FurniroomAPI.Interfaces
{
    public interface IOrdersService
    {
        public Task<ServiceResponseModel> GetAccountOrdersAsync(int accountId, string httpMethod, string endpoint, string queryParams, string requestId);
        public Task<ServiceResponseModel> AddOrderAsync(OrderModel order, string httpMethod, string endpoint, string queryParams, string requestId);
        public Task<ServiceResponseModel> AddQuestionAsync(QuestionModel question, string httpMethod, string endpoint, string queryParams, string requestId);
    }
}
