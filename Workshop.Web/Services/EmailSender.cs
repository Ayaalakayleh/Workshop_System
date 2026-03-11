using DocumentFormat.OpenXml.Spreadsheet;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Workshop.Core.DTOs.General;
using Workshop.Web.Models;

namespace Workshop.Web.Services
{
    public class EmailSender 
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendAsync(Mail omail, CompanyInfo company)
        {
            using (var omailMessage = new MailMessage())
            using (var osmtpClient = new SmtpClient())
            {
                try
                {
                    omailMessage.From = new MailAddress(company.SMTPEmail);

                    foreach (var addr in (omail.To ?? "").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                        omailMessage.To.Add(addr.Trim());

                    string htmlEmail = string.Empty;
                    omailMessage.Body = omail.Body;

                    if (!string.IsNullOrEmpty(htmlEmail))
                    {
                        htmlEmail = htmlEmail.Replace("[Body]", omail.Body ?? string.Empty)
                                             .Replace("[CompanyName]", company.CompanySecondaryName ?? string.Empty)
                                             .Replace("[Fax]", company.Fax ?? string.Empty)
                                             .Replace("[Address]", company.Address ?? string.Empty);
                        omailMessage.Body = htmlEmail;
                    }

                    omailMessage.BodyEncoding = Encoding.UTF8;
                    omailMessage.Subject = omail.Subject ?? string.Empty;
                    omailMessage.IsBodyHtml = true;

                    if (!string.IsNullOrEmpty(omail.AttachmentPath) && File.Exists(omail.AttachmentPath))
                        omailMessage.Attachments.Add(new Attachment(omail.AttachmentPath));

                    if (omail.Attachments != null)
                    {
                        foreach (var a in omail.Attachments)
                        {
                            if (a?.Content == null || a.Content.Length == 0) continue;

                            var stream = new MemoryStream(a.Content);
                            var ct = string.IsNullOrWhiteSpace(a.ContentType)
                                ? MediaTypeNames.Application.Octet
                                : a.ContentType;

                            var fileName = string.IsNullOrWhiteSpace(a.FileName)
                                ? "attachment"
                                : a.FileName;

                            var att = new Attachment(stream, fileName, ct);
                            omailMessage.Attachments.Add(att);
                        }
                    }

                    osmtpClient.Host = company.Host?.Trim();
                    osmtpClient.Port = Convert.ToInt32(company.Port);
                    osmtpClient.EnableSsl = company.Ssl;
                    osmtpClient.UseDefaultCredentials = false;
                    osmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    osmtpClient.Credentials = new NetworkCredential(
                        company.SMTPEmail?.Trim(),
                        company.Password?.Trim()
                    );

                    await osmtpClient.SendMailAsync(omailMessage);

                    _logger.LogInformation("Email sent to {To} with subject {Subject}", omail.To, omail.Subject);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to {To}", omail.To);
                    return false;
                }
            }
        }
    }
}
