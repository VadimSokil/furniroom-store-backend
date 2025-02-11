using InformationService.Models.Response;

namespace InformationService.Interfaces
{
    public interface ICatalogService
    {
        public Task<ServiceResponseModel> GetAllCategoriesAsync();
        public Task<ServiceResponseModel> GetAllSubcategoriesAsync();
        public Task<ServiceResponseModel> GetAllSetsAsync();
        public Task<ServiceResponseModel> GetAllImagesAsync();
        public Task<ServiceResponseModel> GetAllModulesAsync();
    }
}
