using FurniroomAPI.Interfaces;
using FurniroomAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FurniroomAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            var connectionString = Environment.GetEnvironmentVariable("connectionString");
            var serviceEmail = Environment.GetEnvironmentVariable("serviceEmail");
            var servicePassword = Environment.GetEnvironmentVariable("servicePassword");
            var requestsSection = configuration.GetSection("Requests");

            var requests = new Dictionary<string, string>();
            foreach (var request in requestsSection.GetChildren())
            {
                requests[request.Key] = request.Value;
            }

            builder.Services.AddSingleton(() => DateTime.UtcNow);
            builder.Services.AddScoped<ICatalogService, CatalogService>(provider => new CatalogService(connectionString, requests));
            builder.Services.AddScoped<IConditionsService, ConditionsService>(provider => new ConditionsService(connectionString, requests));
            builder.Services.AddScoped<IOrdersService, OrdersService>(provider => new OrdersService(connectionString, requests));
            builder.Services.AddScoped<IAuthorizationService, AuthorizationService>(provider => new AuthorizationService(connectionString, serviceEmail, servicePassword, requests));
            builder.Services.AddScoped<IAccountService, AccountService>(provider => new AccountService(connectionString, requests));
            builder.Services.AddScoped<IValidationService, ValidationService>(provider => new ValidationService());

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseCors("AllowAll");

            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            app.Urls.Add($"http://0.0.0.0:{port}");

            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Furniroom API");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();

        }
    }
}
