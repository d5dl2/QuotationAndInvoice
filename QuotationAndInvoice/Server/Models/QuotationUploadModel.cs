using Microsoft.AspNetCore.Mvc;
using QuotationAndInvoice.Server.Filters;
using QuotationAndInvoice.Shared;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuotationAndInvoice.Server.Models
{
    public class QuotationUploadModel
    {
        public IEnumerable<IFormFile> Files { get; set; }
        [ModelBinder(BinderType = typeof(FormDataJsonBinder))]
        public QuotationDto QuotationDto { get; set; }
    }
}
