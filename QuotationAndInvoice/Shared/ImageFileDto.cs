using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuotationAndInvoice.Shared
{
    public class ImageFileDto
    {
        public string FileName { get; set; }
        public byte[] Bytes { get; set; }
    }
}
