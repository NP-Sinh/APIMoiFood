using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Globalization;
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

        // Nén ảnh
        public static async Task<string> CompressedImage(IFormFile formFile, string folderName, int maxWidth = 800, int maxHeight = 800, int quality = 80, int minSizeToCompressKB = 200)
        {
            if (formFile == null || formFile.Length == 0)
                throw new ArgumentException("File ảnh không hợp lệ");

            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);

            var originalName = Path.GetFileNameWithoutExtension(formFile.FileName);
            var safeName = string.Concat(originalName.Split(Path.GetInvalidFileNameChars()));

            var fileName = safeName + ".jpg";
            var filePath = Path.Combine(rootPath, fileName);
            int count = 1;
            while (File.Exists(filePath))
            {
                fileName = $"{safeName}_{count}.jpg";
                filePath = Path.Combine(rootPath, fileName);
                count++;
            }

            // nếu ảnh nhỏ hơn minSizeToCompressKB thì không nén
            if (formFile.Length < minSizeToCompressKB * 1024)
            {
                await using var fs = new FileStream(filePath, FileMode.Create);
                await formFile.CopyToAsync(fs);
                return $"/{folderName}/{fileName}";
            }

            using var image = await Image.LoadAsync(formFile.OpenReadStream());
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxWidth, maxHeight)
            }));

            await using var stream = new FileStream(filePath, FileMode.Create);
            await image.SaveAsJpegAsync(stream, new JpegEncoder 
            { 
                Quality = quality 
            });
            return $"/{folderName}/{fileName}";
        }
        // bỏ dấu tiếng việt
        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var normalized = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
