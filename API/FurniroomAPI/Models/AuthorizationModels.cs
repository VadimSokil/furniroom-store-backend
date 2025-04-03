using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

namespace FurniroomAPI.Models
{
    public abstract class StrictValidationModel
    {
        public List<string> ValidateStrict(JsonElement jsonElement)
        {
            var errors = new List<string>();
            var modelType = GetType();

            var expectedProps = modelType.GetProperties()
                .Select(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name)
                .Where(name => name != null)
                .ToHashSet();

            foreach (var prop in modelType.GetProperties())
            {
                if (prop.GetCustomAttribute<RequiredAttribute>() != null &&
                    !jsonElement.TryGetProperty(
                        prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name,
                        out _))
                {
                    errors.Add($"Missing required field: {prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name}");
                }
            }

            foreach (var jsonProp in jsonElement.EnumerateObject())
            {
                if (!expectedProps.Contains(jsonProp.Name))
                {
                    errors.Add($"Unexpected field: {jsonProp.Name}");
                }
            }

            return errors;
        }
    }

    public class SignInModel : StrictValidationModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [JsonPropertyName("passwordHash")]
        public string PasswordHash { get; set; }
    }

    public class SignUpModel : StrictValidationModel
    {
        [Required(ErrorMessage = "Account ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Account ID must be positive")]
        [JsonPropertyName("accountId")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Account name is required")]
        [StringLength(50, ErrorMessage = "Account name cannot exceed 50 characters")]
        [JsonPropertyName("accountName")]
        public string AccountName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, ErrorMessage = "Password cannot exceed 128 characters")]
        [JsonPropertyName("passwordHash")]
        public string PasswordHash { get; set; }
    }

    public class EmailRequest : StrictValidationModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}