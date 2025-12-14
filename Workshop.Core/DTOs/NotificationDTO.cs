namespace Workshop.Core.DTOs
{
    public class Notification
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public int? RoleId { get; set; }
        public int? UserId { get; set; }
        public int Type { get; set; }
        public int? RelatedItemId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
        public string PrimaryMessage { get; set; }
        public string SecondaryMessage { get; set; }
        public string Message { get; set; }
        public int? DeletedBy { get; set; }
        public int? AgreementId { get; set; }
        public string Link { get; set; }
        public string strType { get; set; }
        public int ModuleId { get; set; } // For link
        public string PrimaryType { get; set; }
        public string SecondaryType { get; set; }
        public int ToModuleId { get; set; }
    }
}
