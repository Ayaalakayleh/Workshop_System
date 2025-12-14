

using Workshop.Domain.Entities;

namespace Workshop.Core.DTOs
{
    public class GroupedServicesDTO
    {
        public int WIPId { get; set; }
        public double TotalItemsPerWIPId { get; set; }


        public double TotalTimeTaken { get; set; }
        public double TotalStandardHours { get; set; }
        public string Items { get; set; }

    }
}