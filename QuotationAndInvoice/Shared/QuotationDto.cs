using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuotationAndInvoice.Shared
{
    public class QuotationDto
    {
        public QuotationDto()
        {
            Tasks = new List<QuotationTaskDto>();
        }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Customer field required")]
        public string Customer { get; set; }
        public int No { get; set; }
        public DateTime Date { get; set; }

        [CannotBeEmptyAttribute]
        public List<QuotationTaskDto> Tasks { get; set; }
        
        public List<string>? Images { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Email adres Missing")]
        [EmailAddress(ErrorMessage = "Unvalid Email Address")]
        public string ToEmail { get; set; }

        [JsonIgnore]
        public decimal Total { get {
                return Tasks.Sum(x => x.Total);
            } 
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CannotBeEmptyAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            var list = value as IEnumerable;
            return list != null && list.GetEnumerator().MoveNext();
        }
    }
}
