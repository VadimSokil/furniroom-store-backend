using FurniroomAPI.Models.Response;

namespace FurniroomAPI.Interfaces
{
    public interface ICatalogService
    {
        public Task<ServiceResponseModel> GetAllCategoriesAsync(string httpMethod, string endpoint, string queryParams,string requestId);
        public Task<ServiceResponseModel> GetAllSubcategoriesAsync(string httpMethod, string endpoint, string queryParams, string requestId);
        public Task<ServiceResponseModel> GetAllSetsAsync(string httpMethod, string endpoint, string queryParams, string requestId);
        public Task<ServiceResponseModel> GetAllImagesAsync(string httpMethod, string endpoint, string queryParams, string requestId);
        public Task<ServiceResponseModel> GetAllModulesAsync(string httpMethod, string endpoint, string queryParams, string requestId);
    }
}
