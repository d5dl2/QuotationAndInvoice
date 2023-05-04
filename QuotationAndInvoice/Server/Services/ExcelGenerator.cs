//using QuotationAndInvoice.Server.Data;
//using OfficeOpenXml;

//namespace QuotationAndInvoice.Server.Services
//{
//    public class ExcelGenerator : IPdfGenerator
//    {
//        public async Task<byte[]> GeneratePdfAsync(Quotation quotation)
//        {
//            using (var package = new ExcelPackage(@"Colorwise.xlsx"))
//            {
//                var sheet = package.Workbook.Worksheets[0];
//                sheet.Cells["A10"].Value = DateTime.Now.ToShortDateString();
//                sheet.Cells["E9"].Value = quotation.Customer;
//                sheet.Cells["F10"].Value = quotation.No;

//                var counter = 12;
//                foreach (var task in quotation.Tasks)
//                {
//                    sheet.Cells["B" + counter].Value = task.Description;
//                    sheet.Cells["H" + counter].Value = task.Total;

//                    counter++;
//                }

//                var ms = new MemoryStream();
//                await package.SaveAsAsync(ms);

//                return ms.ToArray();
//            }
//        }
//    }
//}
