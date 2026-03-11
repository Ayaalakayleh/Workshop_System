namespace Workshop.Web.Models
{
    public class EmailTemplate
    {
        public int Id { get; set; }
        public string PrimarySubject { get; set; }
        public string SecondarySubject { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
       
        public string PrimaryBody { get; set; }
      
        public string SecondaryBody { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<EmailVariable> EmailVariablesList { get; set; }
        public int? RelatedId { get; set; }
    }
    public class EmailVariable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int EmailId { get; set; }
        public string PrimaryDescription { get; set; }
        public string SecondaryDescription { get; set; }

    }

}
