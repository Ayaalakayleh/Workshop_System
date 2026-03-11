namespace Workshop.Web.Models
{
    public class Mail
    {
        public string To { get; set; }
        public string From { get; set; }
        public string BCC { get; set; }
        public string CC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string TempBody { get; set; } // without replacing variables
                                             //  public HttpPostedFileBase[] file { get; set; }
        public int? Type { get; set; } // 1 for Reservation , 2 for Agreement
        public int Id { get; set; }
        public int EmailId { get; set; }
        public string AttachmentPath { get; set; }

        public List<string> AttachmentPaths { get; set; }
        public List<MailAttachment> Attachments { get; set; }
    }
    public class MailAttachment
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }  // e.g. "application/pdf"
        public byte[] Content { get; set; }      // raw file bytes
    }
}
