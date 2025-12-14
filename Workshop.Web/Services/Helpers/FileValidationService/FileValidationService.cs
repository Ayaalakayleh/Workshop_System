

using Workshop.Web.Interfaces.Services;

namespace Workshop.Web.Services
{
    public class FileValidationService : IFileValidationService
    {
        private readonly long _defaultMaxSize;
        private readonly string[] _defaultAllowedExtensions;

        public FileValidationService(IConfiguration configuration)
        {
            _defaultMaxSize = configuration.GetValue<long>("FileUpload:MaxFileSize", 10 * 1024 * 1024); // 10MB default
            _defaultAllowedExtensions = configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>()
                ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
        }

        public (bool IsSuccess, string Message) CheckFileTypeAndSize(IFormFile file)
        {
            return CheckFileTypeAndSize(file, _defaultMaxSize, _defaultAllowedExtensions);
        }

        public (bool IsSuccess, string Message) CheckFileTypeAndSize(IFormFile file, long maxSizeInBytes, string[] allowedExtensions)
        {
            if (file == null || file.Length == 0)
                return (false, "File is required");

            // Check file size
            if (file.Length > maxSizeInBytes)
            {
                var maxSizeMB = maxSizeInBytes / (1024 * 1024);
                return (false, $"File size exceeds the maximum allowed size of {maxSizeMB}MB");
            }

            // Check file extension
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                var allowedExtensionsString = string.Join(", ", allowedExtensions);
                return (false, $"File type not allowed. Allowed types: {allowedExtensionsString}");
            }

            // Additional checks can be added here (e.g., magic number validation)

            return (true, "File is valid");
        }
    }
}