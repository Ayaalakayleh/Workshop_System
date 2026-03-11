namespace Workshop.Web.Models
{
    public class WorkflowEmailRequest
    {
        public Guid MasterId { get; set; }
        public int WipId { get; set; }
        public int WipItemId { get; set; }
        public int KeyId { get; set; }
        public int CompanyId { get; set; }
        public int Action { get; set; }   // 1 approve, 2 reject, 3 review, 0 new
        public string Lang { get; set; } = "en";
        public int CreatedBy { get; set; }
    }
}
