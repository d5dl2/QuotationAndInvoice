using Amazon.DynamoDBv2.DataModel;
using System.ComponentModel.DataAnnotations;

namespace QuotationAndInvoice.Server.Data
{
    [DynamoDBTable("Quotations")]
    public class Quotation
    {
        [DynamoDBProperty("No")]
        public int No { get; set; }
        public string? Customer { get; set; }

        public List<QuotationTask> Tasks { get; set; }
        public List<string> Images { get; set; }
        public string EmailAddress { get; set; }
        public DateTime Date { get; set; }
    }
}
