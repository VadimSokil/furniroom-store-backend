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

        public CatalogService(string connectionString,Dictionary<string, string> requests, ILoggingService loggingService)
        {
            _connectionString = connectionString;
            _requests = requests;
            _loggingService = loggingService;
        }

        public async Task<ServiceResponseModel> GetAllCategoriesAsync(TransferLogModel transfer)
        {
            return await GetInformationAsync<CategoryModel>(
                "GetAllCategories",
                reader => new CategoryModel
                {
                    CategoryId = reader.GetInt32("CategoryId"),
                    CategoryName = reader.GetString("CategoryName")
                },
                transfer);
        }

        public async Task<ServiceResponseModel> GetAllSubcategoriesAsync(TransferLogModel transfer)
        {
            return await GetInformationAsync<SubcategoryModel>(
                "GetAllSubcategories",
                reader => new SubcategoryModel
                {
                    SubcategoryId = reader.GetInt32("SubcategoryId"),
                    CategoryId = reader.GetInt32("CategoryId"),
                    SubcategoryName = reader.GetString("SubcategoryName")
                },
                transfer);
        }

        public async Task<ServiceResponseModel> GetAllSetsAsync(TransferLogModel transfer)
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
                transfer);
        }

        public async Task<ServiceResponseModel> GetAllImagesAsync(TransferLogModel transfer)
        {
            return await GetInformationAsync<ImageModel>(
                "GetAllImages",
                reader => new ImageModel
                {
                    ImageId = reader.GetInt32("ImageId"),
                    ProductId = reader.GetInt32("ProductId"),
                    ImageUrl = reader.GetString("ImageUrl")
                },
                transfer);
        }

        public async Task<ServiceResponseModel> GetAllModulesAsync(TransferLogModel transfer)
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
                transfer);
        }

        private async Task<ServiceResponseModel> GetInformationAsync<T>(string requestKey, Func<MySqlDataReader, T> mapFunc, TransferLogModel transfer)
        {
            try
            {
                await LogActionAsync($"{requestKey} started", transfer);

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

                await LogActionAsync($"{requestKey} completed successfully", transfer);

                return new ServiceResponseModel
                {
                    Status = true,
                    Message = "Data retrieved successfully",
                    Data = items
                };
            }
            catch (MySqlException ex)
            {
                await LogErrorAsync(ex, transfer);
                return CreateErrorResponse($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, transfer);
                return CreateErrorResponse($"Unexpected error: {ex.Message}");
            }
        }

        private async Task LogActionAsync(string status, TransferLogModel transfer)
        {
            try
            {
                await _loggingService.AddLogAsync(new LogModel
                {
                    Date = DateTime.UtcNow,
                    HttpMethod = transfer.HttpMethod,
                    Endpoint = transfer.Endpoint,
                    QueryParams = transfer.QueryParams,
                    Status = status,
                    RequestId = transfer.RequestId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log action: {ex.Message}");
            }
        }

        private async Task LogErrorAsync(Exception ex, TransferLogModel transfer)
        {
            await LogActionAsync($"ERROR: {ex.GetType().Name} - {ex.Message}", transfer);
        }

        private ServiceResponseModel CreateErrorResponse(string message)
        {
            return new ServiceResponseModel
            {
                Status = false,
                Message = message
            };
        }
    }
}