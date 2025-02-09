namespace AuthorizationService.Models.Authorization
{
    public class SignUpModel
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
