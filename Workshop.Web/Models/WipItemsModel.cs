using Workshop.Core.DTOs;

namespace Workshop.Web.Models
{
    public class WipItemsModel
    {
        public List<CreateItemDTO> Items { get; set; } = new List<CreateItemDTO>();
        public bool AllowActions { get; set; }
    }
}
