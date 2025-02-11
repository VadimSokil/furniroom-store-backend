using InformationService.Interfaces;
using InformationService.Models.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace InformationService.Controllers
{
    [Route("catalog")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogService _catalogService;
        public CatalogController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpGet("get-categories-list")]
        public async Task<ActionResult<List<CategoryModel>>> GetAllCategories()
        {
            var result = await _catalogService.GetAllCategoriesAsync();
            return Ok(result);
        }

        [HttpGet("get-subcategories-list")]
        public async Task<ActionResult<List<SubcategoryModel>>> GetAllSubcategories()
        {
            var result = await _catalogService.GetAllSubcategoriesAsync();
            return Ok(result);
        }

        [HttpGet("get-sets-list")]
        public async Task<ActionResult<List<SetModel>>> GetAllSets()
        {
            var result = await _catalogService.GetAllSetsAsync();
            return Ok(result);
        }

        [HttpGet("get-images-list")]
        public async Task<ActionResult<List<ImageModel>>> GetAllImages()
        {
            var result = await _catalogService.GetAllImagesAsync();
            return Ok(result);
        }

        [HttpGet("get-modules-list")]
        public async Task<ActionResult<List<ModuleModel>>> GetAllModules()
        {
            var result = await _catalogService.GetAllModulesAsync();
            return Ok(result);
        }

    }
}
