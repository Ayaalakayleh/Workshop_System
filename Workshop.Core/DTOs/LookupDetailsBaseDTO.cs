using System;

namespace Workshop.Core.DTOs
{
    #region Lookup Details
    public abstract class LookupDetailsBaseDTO
    {
        public int HeaderId { get; set; }
        public string Code { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public int CompanyId { get; set; }
    }
    public class CreateLookupDetailsDTO : LookupDetailsBaseDTO
    {
        public int CreatedBy { get; set; }

    }
    public class UpdateLookupDetailsDTO : LookupDetailsBaseDTO
    {
        public int Id { get; set; }
        public int ModifiedBy { get; set; }
    }
    public class DeleteLookupDetailsDTO
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class LookupDetailsDTO : LookupDetailsBaseDTO
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
    }

    #endregion

    #region Lookup Header

    public class LookupHeaderDTO
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public string Name { get; set; }
        public bool IsEditable { get; set; }
    }
    #endregion

}