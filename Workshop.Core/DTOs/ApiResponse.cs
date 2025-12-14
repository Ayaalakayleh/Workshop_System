namespace Workshop.Core.DTOs
{
    public class ApiResponse<T>
    {
        public int? Id;
        public bool IsSuccess { get; set; }
        public T ResponseDetails { get; set; }
    }

    public class ApiErrorResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object Errors { get; set; }
    }
}
