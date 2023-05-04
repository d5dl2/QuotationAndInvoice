using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuotationAndInvoice.Shared
{
    public class QuotationTaskDto
    {
        public string? Description { get; set; }
        public decimal Total { get; set; }
    }
}
