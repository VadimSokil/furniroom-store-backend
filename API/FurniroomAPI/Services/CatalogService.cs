using FurniroomAPI.Interfaces;
using FurniroomAPI.Models.Catalog;
using FurniroomAPI.Models.Log;
using FurniroomAPI.Models.Response;
using MySql.Data.MySqlClient;

namespace FurniroomAPI.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, string> _requests;
        private readonly ILoggingService _loggingService;
        private readonly DateTime _logDate;

        public CatalogService(
            string connectionString,
            Dictionary<string, string> requests,
            ILoggingService loggingService,
            Func<DateTime> logDate)
        {
            _connectionString = connectionString;
            _requests = requests;
            _loggingService = loggingService;
            _logDate = logDate();
        }

        public async Task<ServiceResponseModel> GetAllCategoriesAsync(
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            return await GetInformationAsync<CategoryModel>(
                "GetAllCategories",
                reader => new CategoryModel
                {
                    CategoryId = reader.GetInt32("CategoryId"),
                    CategoryName = reader.GetString("CategoryName")
                },
                httpMethod,
                endpoint,
                queryParams,
                requestId
            );
        }

        public async Task<ServiceResponseModel> GetAllSubcategoriesAsync(
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            return await GetInformationAsync<SubcategoryModel>(
                "GetAllSubcategories",
                reader => new SubcategoryModel
                {
                    SubcategoryId = reader.GetInt32("SubcategoryId"),
                    CategoryId = reader.GetInt32("CategoryId"),
                    SubcategoryName = reader.GetString("SubcategoryName")
                },
                httpMethod,
                endpoint,
                queryParams,
                requestId
            );
        }

        public async Task<ServiceResponseModel> GetAllSetsAsync(
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            return await GetInformationAsync<SetModel>(
                "GetAllSets",
                reader => new SetModel
                {
                    SetId = reader.GetInt32("SetId"),
                    SubcategoryId = reader.GetInt32("SubcategoryId"),
                    SetName = reader.GetString("SetName"),
                    SetDescription = reader.GetString("SetDescription"),
                    SetImageUrl = reader.GetString("SetImageUrl")
                },
                httpMethod,
                endpoint,
                queryParams,
                requestId
            );
        }

        public async Task<ServiceResponseModel> GetAllImagesAsync(
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            return await GetInformationAsync<ImageModel>(
                "GetAllImages",
                reader => new ImageModel
                {
                    ImageId = reader.GetInt32("ImageId"),
                    ProductId = reader.GetInt32("ProductId"),
                    ImageUrl = reader.GetString("ImageUrl")
                },
                httpMethod,
                endpoint,
                queryParams,
                requestId
            );
        }

        public async Task<ServiceResponseModel> GetAllModulesAsync(
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            return await GetInformationAsync<ModuleModel>(
                "GetAllModules",
                reader => new ModuleModel
                {
                    ModuleId = reader.GetInt32("ModuleId"),
                    ProductId = reader.GetInt32("ProductId"),
                    ModuleName = reader.GetString("ModuleName"),
                    ModuleDescription = reader.GetString("ModuleDescription"),
                    ModuleImageUrl = reader.GetString("ModuleImageUrl")
                },
                httpMethod,
                endpoint,
                queryParams,
                requestId
            );
        }

        private async Task<ServiceResponseModel> GetInformationAsync<T>(
            string requestKey,
            Func<MySqlDataReader, T> mapFunc,
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            try
            {
                await LogActionAsync("Request started", httpMethod, endpoint, queryParams, requestId);

                var items = new List<T>();

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand(_requests[requestKey], connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                items.Add(mapFunc((MySqlDataReader)reader));
                            }
                        }
                    }
                }

                await LogActionAsync("Request completed successfully", httpMethod, endpoint, queryParams, requestId);

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Data retrieved successfully",
                    Data = items
                };
            }
            catch (MySqlException ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return CreateErrorResponse($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, httpMethod, endpoint, queryParams, requestId);
                return CreateErrorResponse($"Unexpected error: {ex.Message}");
            }
        }

        private async Task LogActionAsync(
            string status,
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            try
            {
                await _loggingService.AddLogAsync(new LogModel
                {
                    Date = _logDate,
                    HttpMethod = httpMethod,
                    Endpoint = endpoint,
                    QueryParams = queryParams,
                    Status = status,
                    RequestId = requestId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log action: {ex.Message}");
            }
        }

        private async Task LogErrorAsync(
            Exception ex,
            string httpMethod,
            string endpoint,
            string queryParams,
            string requestId)
        {
            await LogActionAsync($"ERROR: {ex.GetType().Name} - {ex.Message}",
                httpMethod, endpoint, queryParams, requestId);
        }

        private ServiceResponseModel CreateErrorResponse(string message)
        {
            return new ServiceResponseModel
            {
                Status = false,
                Message = message,
                Data = null
            };
        }
    }
}