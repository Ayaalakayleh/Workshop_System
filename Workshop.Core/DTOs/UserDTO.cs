namespace Workshop.Core.DTOs
{
    public class UserDTO
    {
        public int UserID { get; set; }
        public int CompanyId { get; set; }
        public int CompanyBranchId { get; set; }
        public string Username { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string FirstName { get; set; }
        public string MidName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNo { get; set; }
        public string UserPermission { get; set; }
        public bool IsPasswordReset { get; set; }
        public int GroupOfUserID { get; set; }
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int DepartmentId { get; set; }
        public string JobTitle { get; set; }
        public int Gender { get; set; }
        public int Role_Id { get; set; }
        public int Type { get; set; }
        public string ProfilePicture { get; set; }
        public string JobDescription { get; set; }
        public string ConfirmPassword { get; set; }
        public DateTime PasswordLastModifyDate { get; set; }
        //public List<GroupOfUserPermission> ColGroupsOfPermission { get; set; }
        public List<Department> ColDepartment { get; set; }
        public Department RefDepartment { get; set; }
        public string Name { get; set; }
        public int UserGroupId { get; set; }
    }
    public class User
    {
        public int UserID { get; set; }
        public int CompanyId { get; set; }
        public int CompanyBranchId { get; set; }
        public string Username { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string FirstName { get; set; }
        public string MidName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNo { get; set; }
        public string UserPermission { get; set; }
        public bool IsPasswordReset { get; set; }
        public int GroupOfUserID { get; set; }
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int DepartmentId { get; set; }
        public string JobTitle { get; set; }
        public int Gender { get; set; }
        public int Role_Id { get; set; }
        public int Type { get; set; }
        public string ProfilePicture { get; set; }
        public string JobDescription { get; set; }
        public string ConfirmPassword { get; set; }
        public DateTime PasswordLastModifyDate { get; set; }
        public List<GroupOfUserPermission> ColGroupsOfPermission { get; set; }
        public List<Department> ColDepartment { get; set; }
        public Department RefDepartment { get; set; }
        public string Name { get; set; }
        public int UserGroupId { get; set; }
    }
}
