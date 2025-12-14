using Workshop.Core.DTOs;

namespace Workshop.Web.Models
{
    public class TeamModel
    {
        public List<GetTeamDTO>? TeamsList { get; set; }

        public FilterTeamDTO? TeamFilter { get; set; }

        public int TotalPages { get; set; } = 1;

        public int? PageSize { get; set; } = 25;
        public int PageNumber { get; set; } = 1;
    }
}

