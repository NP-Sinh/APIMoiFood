using System.Text;

namespace APIMoiFood.Services
{
    public class CommonServices
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        //Tạo mã OTP
        public static string GenerateOTP(int length = 6)
        {
            var random = new Random();
            var otp = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                otp.Append(random.Next(0, 10));
            }
            return otp.ToString();
        }
    }
}
