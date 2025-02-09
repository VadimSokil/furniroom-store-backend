namespace AuthorizationService.Models.Authorization
{
    public class SignInModel
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
