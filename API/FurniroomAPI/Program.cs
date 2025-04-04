using FurniroomAPI.Interfaces;
using FurniroomAPI.Services;

namespace FurniroomAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder);
            var app = builder.Build();
            ConfigureMiddleware(app);
            app.Run();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            var configuration = builder.Configuration;

            var connectionString = Environment.GetEnvironmentVariable("connectionString");
            var serviceEmail = Environment.GetEnvironmentVariable("serviceEmail");
            var servicePassword = Environment.GetEnvironmentVariable("servicePassword");

            var requests = configuration.GetSection("Requests").GetChildren().ToDictionary(x => x.Key, x => x.Value!);

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IValidationService, ValidationService>();
            builder.Services.AddScoped<ILoggingService>(provider => new LoggingService(connectionString, requests));

            builder.Services.AddScoped<ICatalogService>(provider => new CatalogService(connectionString, requests, provider.GetRequiredService<ILoggingService>()));
            builder.Services.AddScoped<IConditionsService>(provider => new ConditionsService(connectionString, requests, provider.GetRequiredService<ILoggingService>()));
            builder.Services.AddScoped<IOrdersService>(provider => new OrdersService(connectionString, requests, provider.GetRequiredService<ILoggingService>()));
            builder.Services.AddScoped<IAuthorizationService>(provider => new AuthorizationService(connectionString, serviceEmail, servicePassword, requests, provider.GetRequiredService<ILoggingService>()));
            builder.Services.AddScoped<IAccountService>(provider => new AccountService(connectionString, requests, provider.GetRequiredService<ILoggingService>()));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddControllers().ConfigureApiBehaviorOptions(options => { options.SuppressModelStateInvalidFilter = true; });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => {c.SwaggerDoc("v1", new() { Title = "Furniroom API", Version = "v1" }); });
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            app.UseCors("AllowAll");

            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            app.Urls.Add($"http://0.0.0.0:{port}");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Furniroom API v1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
        }
    }
}