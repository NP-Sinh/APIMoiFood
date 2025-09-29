using System.Text;
using System.Threading.Tasks;

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
        // Lưu ảnh vào wwwroot
        public static async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);

            var originalName = Path.GetFileNameWithoutExtension(file.FileName);
            var safeName = string.Concat(originalName.Split(Path.GetInvalidFileNameChars()));
            var extension = Path.GetExtension(file.FileName);

            // Nếu trùng tên, tự động thêm hậu tố
            var fileName = safeName + extension;
            var filePath = Path.Combine(rootPath, fileName);
            int count = 1;
            while (File.Exists(filePath))
            {
                fileName = $"{safeName}_{count}{extension}";
                filePath = Path.Combine(rootPath, fileName);
                count++;
            }

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{folderName}/{fileName}";
        }

        public static void DeleteFileIfExists(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/'));
            if (File.Exists(path)) File.Delete(path);
        }

    }
}
