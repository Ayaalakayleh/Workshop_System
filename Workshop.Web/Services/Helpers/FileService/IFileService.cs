namespace Workshop.Web.Interfaces.Services
{
    public interface IFileService
    {
        Task<(string FilePath, string FileName)> SaveFileAsync(IFormFile file, string subFolder);
        Task<(string FilePath, string FileName)> SaveFileAsync(IFormFile file, string subFolder, string customFileName);
        void DeleteFile(string filePath, string fileName);
        bool FileExists(string filePath, string fileName);
        string GetFileFullPath(string relativeFilePath, string fileName);
        string GetFileFullPath(string relativeFilePath);
        string GetFileUrl(string relativeFilePath, string fileName);
        Task<(string FilePath, string FileName)> SaveBase64FileAsync(string base64String, string subFolder, string customFileName = null);


    }
}