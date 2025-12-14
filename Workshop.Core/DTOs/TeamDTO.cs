using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public abstract class TeamDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string PrimaryName { get; set; } = string.Empty;
        public string SecondaryName { get; set; } = string.Empty;
        public string Short { get; set; } = string.Empty;
        public int Color { get; set; }
        public int CompanyId { get; set; }
        public List<int> Technicians { get; set; } = new List<int>();

    }

    public class AddTeamDTO : TeamDTO
    {
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateTeamDTO : TeamDTO
    {
        public int ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
    }

    public class GetTeamDTO : TeamDTO {
        //public int Id { get; set; }
        public int? TotalPages { get; set; }

    }

    public class FilterTeamDTO 
    {
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; } = 25;
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
    }

    public class PagedTeamResultDTO
    {
        public IEnumerable<GetTeamDTO> Teams { get; set; } = new List<GetTeamDTO>();
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

}
