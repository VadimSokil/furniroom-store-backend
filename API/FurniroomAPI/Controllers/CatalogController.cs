using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace FurniroomAPI.Controllers
{
    [Route("catalog")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogService _catalogService;
        public string currentDateTime = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") + " UTC";

        public CatalogController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpGet("get-categories-list")]
        public async Task<ActionResult<APIResponseModel>> GetCategories()
        {
            var serviceResponse = await _catalogService.GetAllCategoriesAsync();
            var gatewayResponse = new APIResponseModel
            {
                Date = currentDateTime,
                Status = serviceResponse.Status,
                Message = serviceResponse.Message,
                Data = serviceResponse.Data
            };
            return Ok(gatewayResponse);
        }

        [HttpGet("get-subcategories-list")]
        public async Task<ActionResult<APIResponseModel>> GetSubcategories()
        {
            var serviceResponse = await _catalogService.GetAllSubcategoriesAsync();
            var gatewayResponse = new APIResponseModel
            {
                Date = currentDateTime,
                Status = serviceResponse.Status,
                Message = serviceResponse.Message,
                Data = serviceResponse.Data
            };
            return Ok(gatewayResponse);
        }

        [HttpGet("get-sets-list")]
        public async Task<ActionResult<APIResponseModel>> GetSets()
        {
            var serviceResponse = await _catalogService.GetAllSetsAsync();
            var gatewayResponse = new APIResponseModel
            {
                Date = currentDateTime,
                Status = serviceResponse.Status,
                Message = serviceResponse.Message,
                Data = serviceResponse.Data
            };
            return Ok(gatewayResponse);
        }

        [HttpGet("get-images-list")]
        public async Task<ActionResult<APIResponseModel>> GetImages()
        {
            var serviceResponse = await _catalogService.GetAllImagesAsync();
            var gatewayResponse = new APIResponseModel
            {
                Date = currentDateTime,
                Status = serviceResponse.Status,
                Message = serviceResponse.Message,
                Data = serviceResponse.Data
            };
            return Ok(gatewayResponse);
        }

        [HttpGet("get-modules-list")]
        public async Task<ActionResult<APIResponseModel>> GetModules()
        {
            var serviceResponse = await _catalogService.GetAllModulesAsync();
            var gatewayResponse = new APIResponseModel
            {
                Date = currentDateTime,
                Status = serviceResponse.Status,
                Message = serviceResponse.Message,
                Data = serviceResponse.Data
            };
            return Ok(gatewayResponse);
        }
    }
}
