using QuotationAndInvoice.Server.Data;

namespace QuotationAndInvoice.Server.Services
{
    public interface IPdfGenerator
    {
        Task<byte[]> GeneratePdfAsync(Quotation quotation);
    }
}