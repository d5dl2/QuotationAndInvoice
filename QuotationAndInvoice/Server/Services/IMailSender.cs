namespace QuotationAndInvoice.Server.Services
{
    public interface IMailSender
    {
        Task SendMailAsync(string to, IEnumerable<AttachmentContent> attachments);
    }
}
