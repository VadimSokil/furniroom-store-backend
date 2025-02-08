namespace FurniroomAPI.Services
{
    public class ValidationService
    {
        public bool IsNotEmptyValue(object value)
        {
            if (value == null) return false;

            if (value is string str && string.IsNullOrWhiteSpace(str))
                return false;

            return true;
        }

        public bool IsValidDigit(object value)
        {
            if (value is string str && int.TryParse(str, out int result))
            {
                return result > 0;
            }

            if (value is int intValue)
            {
                return intValue > 0;
            }

            return false;
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public bool IsValidLength(string value, int lengthLimit)
        {
            if (value == null) return false;

            return value.Length <= lengthLimit;
        }

        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return false;

            var phoneRegex = new System.Text.RegularExpressions.Regex(@"^\+?\d{10,15}$");
            return phoneRegex.IsMatch(phoneNumber);
        }

    }
}
