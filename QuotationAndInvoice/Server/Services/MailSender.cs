using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace QuotationAndInvoice.Server.Services
{
    public class MailSender : IMailSender
    {
        public async Task SendMailAsync(string to, IEnumerable<AttachmentContent> attachments)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Ahmet Burak", "burak_ahm@hotmail.com"));
            
            message.To.Add(new MailboxAddress("", to));
            

            var builder = new BodyBuilder();

            message.Subject = "No-Reply - Quotation";
            builder.TextBody = @"Hello, 
Our quotation and related photos are attached.";
            foreach (var attachment in attachments)
            {
                builder.Attachments.Add(attachment.FileName, attachment.Stream);
            }

            // Now we just need to set the message body and we're done
            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp-mail.outlook.com", 587, SecureSocketOptions.StartTls);

                // Note: only needed if the SMTP server requires authentication
                await client.AuthenticateAsync("burak_ahm@hotmail.com", "Password41*");
                
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }

    public class AttachmentContent
    {
        public AttachmentContent(byte[] bytes, string fileName) : this(new MemoryStream(bytes), fileName)
        {
        }
        public AttachmentContent(Stream stream, string fileName)
        {
            Stream = stream;
            FileName = fileName;
        }


        public Stream Stream { get; set; }
        public string FileName { get; set; }
    }
}
