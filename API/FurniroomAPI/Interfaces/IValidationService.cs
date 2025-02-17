namespace FurniroomAPI.Interfaces
{
    public interface IValidationService
    {
        public bool IsValidDigit(object value);
        public bool IsValidEmail(string email);
        public bool IsValidLength(string value, int lengthLimit);
        public bool IsValidPhoneNumber(string phoneNumber);
    }
}
