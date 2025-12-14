
using Workshop.Web.Interfaces.Services;

namespace Workshop.Web.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly string _baseUploadPath;

        public FileService(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
            _baseUploadPath = _configuration["FileUpload:DirectoryPath"] ?? "Uploads";
        }

        public async Task<(string FilePath, string FileName)> SaveFileAsync(IFormFile file, string subFolder)
        {
            return await SaveFileAsync(file, subFolder, null);
        }

        public async Task<(string FilePath, string FileName)> SaveFileAsync(IFormFile file, string subFolder, string customFileName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is null or empty", nameof(file));

            if (string.IsNullOrEmpty(subFolder))
                throw new ArgumentException("SubFolder is required", nameof(subFolder));

            string guid = Guid.NewGuid().ToString();
            var fileName = customFileName ?? $"{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";

            // Create folder path: wwwroot/Uploads/subFolder/guid
            var folderPath = Path.Combine(_env.WebRootPath, _baseUploadPath, subFolder, guid);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for database storage
            var relativePath = Path.Combine(subFolder, guid).Replace("\\", "/");
            return (relativePath, fileName);
        }

        public void DeleteFile(string relativeFilePath, string fileName)
        {
            if (string.IsNullOrEmpty(relativeFilePath) || string.IsNullOrEmpty(fileName))
                return;

            var fullPath = GetFileFullPath(relativeFilePath, fileName);

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            // Optionally: Delete the folder if it's empty
            var folderPath = Path.GetDirectoryName(fullPath);
            if (Directory.Exists(folderPath) && !Directory.EnumerateFileSystemEntries(folderPath).Any())
                Directory.Delete(folderPath);
        }

        public bool FileExists(string relativeFilePath, string fileName)
        {
            if (string.IsNullOrEmpty(relativeFilePath) || string.IsNullOrEmpty(fileName))
                return false;

            var fullPath = GetFileFullPath(relativeFilePath, fileName);
            return File.Exists(fullPath);
        }

        public string GetFileFullPath(string relativeFilePath, string fileName)
        {
            if (string.IsNullOrEmpty(relativeFilePath) || string.IsNullOrEmpty(fileName))
                return string.Empty;

            return Path.Combine(_env.WebRootPath, _baseUploadPath, relativeFilePath, fileName).Replace("\\", "/");
        }

        public string GetFileFullPath(string relativeFilePath)
        {
            if (string.IsNullOrEmpty(relativeFilePath))
                return string.Empty;

            return Path.Combine(_env.WebRootPath, _baseUploadPath, relativeFilePath).Replace("\\", "/");
        }

        public string GetFileUrl(string relativeFilePath, string fileName)
        {
            if (string.IsNullOrEmpty(relativeFilePath) || string.IsNullOrEmpty(fileName))
                return string.Empty;

            var baseUrl = _configuration["FileUpload:BaseUrl"]?.TrimEnd('/') ?? "";
            return $"{baseUrl}/{_baseUploadPath}/{relativeFilePath}/{fileName}";
        }


        #region Base64 Files Handling
        private IFormFile ConvertBase64ToIFormFile(string base64String, string fileName)
        {
            try
            {
                // Remove the data URL prefix if present
                string base64Data = base64String.Replace("image/png;base64,", string.Empty);

                byte[] bytes = Convert.FromBase64String(base64Data);
                var stream = new MemoryStream(bytes);

                // Create IFormFile using FormFile class
                var formFile = new FormFile(stream, 0, bytes.Length, "signature", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/png"
                };

                return formFile;
            }
            catch
            {
                return null;
            }
        }

        public async Task<(string FilePath, string FileName)> SaveBase64FileAsync(string base64String, string subFolder, string customFileName = null)
        {
            if (string.IsNullOrEmpty(base64String))
                throw new ArgumentException("Base64 string is null or empty", nameof(base64String));
            if (string.IsNullOrEmpty(subFolder))
                throw new ArgumentException("SubFolder is required", nameof(subFolder));

            string fileName = customFileName ?? $"{DateTime.Now.Ticks}.png";

            var formFile = ConvertBase64ToIFormFile(base64String, fileName);
            if (formFile == null)
                throw new ArgumentException("Invalid Base64 string", nameof(base64String));

            return await SaveFileAsync(formFile, subFolder, fileName);
        }
        #endregion
    }
}