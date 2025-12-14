using System;

namespace Workshop.Domain.Entities
{
    public partial class Team
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string PrimaryName { get; set; } = string.Empty;
        public string SecondaryName { get; set; } = string.Empty;
        public string Short { get; set; } = string.Empty;
        public string? Color { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int? CompanyId { get; set; }
    }
}
