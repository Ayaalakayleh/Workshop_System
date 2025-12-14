using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Domain.Enum;

namespace Workshop.Core.DTOs
{
    public enum Basis { RTS = 1, Skill = 2, Franchise = 3, All = 0 }
    public enum AccountType { Customer = 0, Internal = 1, Warranty = 2 }


    public class PriceMatrixDTO
    {
        public int Id { get; set; }
        public string AppliesTo { get; set; } = string.Empty;

        //public int? AccountType { get; set; }
        public int? AccountId { get; set; }
        public AccountTypeEnum AccountType { get; set; }
        public int BasisId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<int> MatchValue { get; set; } = new List<int>();
        public List<int> Customers { get; set; } = new List<int>();
        public List<KeyValuePair<int, string>> MatchValueTextPair { get; set; } = new List<KeyValuePair<int, string>>();
        public decimal RatePerHour { get; set; }
        public decimal Markup { get; set; }
        public Basis Basis { get; set; } = Basis.RTS;
        public int TotalPages { get; set; }

    }

    public class CreatePriceMatrixDTO : PriceMatrixDTO
    {
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdatePriceMatrixDTO : PriceMatrixDTO
    {
        public int UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    public class GetPriceMatrixDTO : PriceMatrixDTO
    {
        public int PageName { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PriceMatrixFilter
    {
        public string? Name { get; set; }
        public string? AppliesTo { get; set; }
        public Basis? Basis { get; set; }
        // paging
        public int? PageNumber { get; set; } = 1;
        public int? PageSize { get; set; } = 25;
    }

    public class PagedPriceMatrixResultDTO
    {
        public IEnumerable<GetPriceMatrixDTO> Items { get; set; } = new List<GetPriceMatrixDTO>();
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

}
