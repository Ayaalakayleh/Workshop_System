namespace Workshop.Web.Interfaces.Services
{
    public interface IFileValidationService
    {
        (bool IsSuccess, string Message) CheckFileTypeAndSize(IFormFile file);
        (bool IsSuccess, string Message) CheckFileTypeAndSize(IFormFile file, long maxSizeInBytes, string[] allowedExtensions);
    }
}

