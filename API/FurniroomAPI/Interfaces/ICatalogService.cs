using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Response;

namespace FurniroomAPI.Interfaces
{
    public interface ICatalogService
    {
        public Task<ServiceResponseModel> GetAllCategoriesAsync(TransferLogModel transfer);
        public Task<ServiceResponseModel> GetAllSubcategoriesAsync(TransferLogModel transfer);
        public Task<ServiceResponseModel> GetAllSetsAsync(TransferLogModel transfer);
        public Task<ServiceResponseModel> GetAllImagesAsync(TransferLogModel transfer);
        public Task<ServiceResponseModel> GetAllModulesAsync(TransferLogModel transfer);
    }
}
