namespace Workshop.Web.Models
{
    public class WorkflowHistory
    {
        public int? ActionId { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public int StateId { get; set; }
        public string UserName { get; set; }
        public string ActionName { get; set; }
        public string StateName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string Reason { get; set; }
        public string GroupName { get; set; }
    }
}
