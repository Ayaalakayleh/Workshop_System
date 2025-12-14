namespace Workshop.Core.DTOs
{
    public class Error
    {
        public Error()
        {
            Errors = new List<string>();
        }
        public int Id { get; set; }
        public List<string> Errors { get; set; }
    }
}
