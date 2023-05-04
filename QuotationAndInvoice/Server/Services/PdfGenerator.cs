using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Annot;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using QuotationAndInvoice.Server.Data;

namespace QuotationAndInvoice.Server.Services
{
    public class PdfGenerator : IPdfGenerator
    {
        string jpg = "colorwise.jpg";
        const string decimalFormat = "${0:0.00}";
        public async Task<byte[]> GeneratePdfAsync(Quotation quotation)
        {
            MemoryStream ms = new MemoryStream();
            PdfWriter writer = new PdfWriter(ms);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

            AddHeader(document);
            AddImage(document);
            AddContactTable(document);
            AddTasksTable(document, quotation);
            document.Close();

            return ms.ToArray();
        }

        private void AddHeader(Document document)
        {
            document.Add(new Paragraph("QUOTATION").
                SetTextAlignment(TextAlignment.CENTER).
                SetFontSize(15).
                SetBold());
        }
        private void AddImage(Document document)
        {
            document.Add(new Image(ImageDataFactory.Create(jpg)).SetWidth(UnitValue.CreatePercentValue(40)));
        }

        private void AddContactTable(Document document)
        {
            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 15, 85 })).
                SetWidth(UnitValue.CreatePercentValue(40));

            foreach (var info in new ContactInfo().Infos)
            {
                table.AddCell(new Cell().SetPadding(0).
                    Add(new Paragraph(info.Key).
                        SetBold().
                        SetFontSize(7)).
                    SetBorder(Border.NO_BORDER));

                table.AddCell(new Cell().SetPadding(0).
                Add(info.Value).
                SetBorder(Border.NO_BORDER));
            }

            document.Add(table);
        }

        private void AddTasksTable(Document document, Quotation quotation)
        {
            PdfFont pdfFont = PdfFontFactory.CreateFont(StandardFonts.COURIER);
            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 })).
                SetWidth(UnitValue.CreatePercentValue(100)).
                SetBorder(new SolidBorder(1)).
                SetFont(pdfFont).
                SetFontSize(12);
            table.SetMarginTop(15);
            table.AddCell(GetDefaultCell("DATE: ", 3, quotation.Date.ToShortDateString()));
            table.AddCell(GetDefaultCell("To: " + quotation.Customer, 4, textAlignment: false));
            table.AddCell(GetDefaultCell("Quotation No: ", 3, quotation.No.ToString()));
            table.AddCell(GetDefaultCell("QTY"));
            table.AddCell(GetDefaultCell("DESCRIPTION", 6));
            table.AddCell(GetDefaultCell("Qty"));
            table.AddCell(GetDefaultCell("Price"));
            table.AddCell(GetDefaultCell("Total"));

            Dictionary<string, string> giders =
                quotation.Tasks.ToDictionary(
                    keySelector: x => string.IsNullOrEmpty(x.Description) ? "" : x.Description,
                    elementSelector: x => x.Total == 0 ? " " : string.Format(decimalFormat, x.Total));

            foreach (var gider in giders)
            {
                table.AddCell(new QtyCell());
                table.AddCell(new DescriptionCell().
                    Add(new Paragraph(gider.Key)));
                table.AddCell(new QtyCell());
                table.AddCell(new QtyCell());                
                table.AddCell(new QtyCell().
                    Add(new Paragraph(gider.Value)).
                    SetTextAlignment(TextAlignment.RIGHT));
            }
            
            for (int i = 0; i < 23 - giders.Count; i++)
            {
                table.AddCell(new QtyCell().SetHeight(18));
                table.AddCell(new DescriptionCell().SetHeight(18));
                table.AddCell(new QtyCell().SetHeight(18));
                table.AddCell(new QtyCell().SetHeight(18));
                table.AddCell(new QtyCell().SetHeight(18));
            }

            var subtotal = quotation.Tasks.Sum(x => x.Total);
            var gst = subtotal / 10;
            var fullTotal = subtotal + gst;
            Dictionary<string, string> footers = new Dictionary<string, string>();
            footers.Add("SUBTOTAL", string.Format(decimalFormat, subtotal));
            footers.Add("GST", string.Format(decimalFormat, gst));
            footers.Add("TOTAL INCLUSIVE OF GST", string.Format(decimalFormat, fullTotal));

            var firstLine = true;
            foreach (var footer in footers)
            {
                table.AddFooterCell(new FooterCell(0, 9, firstLine).
                   Add(new Paragraph(footer.Key)).
                   SetTextAlignment(TextAlignment.RIGHT));
                table.AddFooterCell(new FooterCell().
                    Add(new Paragraph(footer.Value)).
                    SetTextAlignment(TextAlignment.RIGHT));
                firstLine = false;
            }

            document.Add(table);
        }

        private Cell GetDefaultCell(string content, int columnSpan = 0, string newParagContent = "", bool textAlignment = true)
        {
            if (string.IsNullOrWhiteSpace(newParagContent))
            {
                var cell = new BorderlessCell(0, columnSpan).
                    Add(new Paragraph(content).
                        SetBold()).
                    SetBorder(new SolidBorder(1));

                if (textAlignment)
                    cell.SetTextAlignment(TextAlignment.CENTER);
                return cell;
            }

            else
                return new BorderlessCell(0, columnSpan).
                    Add(new Paragraph(content).
                        SetBold()).
                    Add(new Paragraph(newParagContent).SetTextAlignment(TextAlignment.CENTER).SetBold()).
                    SetBorder(new SolidBorder(1));
        }

        class ContactInfo
        {
            public Dictionary<string, IBlockElement> Infos { get; set; }

            public ContactInfo()
            {
                var fontSize = 7;
                Infos = new Dictionary<string, IBlockElement>();

                Infos.Add("Adress", new Paragraph("6 Amalfi Court Roxburgh Park VIC 3064").
                    SetFontSize(fontSize));
                Infos.Add("Tel", new Paragraph("+61 414 385 952").
                    SetFontSize(fontSize));

                Infos.Add("Email", new Paragraph().
                    Add(CreateLink("info@colorwise.com.au", "mailto:info@colorwise.com.au")).
                    SetFontSize(fontSize));
                Infos.Add("Web", new Paragraph().
                    Add(CreateLink("www.colorwise.com.au", "www.colorwise.com.au").
                    SetFontSize(fontSize)));

                Infos.Add("ABN", new Paragraph("18 358 453 585").
                    SetFontSize(fontSize));
            }

            Text CreateLink(string content, string url)
            {
                Rectangle rect = new Rectangle(0, 0);
                PdfLinkAnnotation annotation = new PdfLinkAnnotation(rect);
                PdfAction action = PdfAction.CreateURI(url);
                annotation.SetUriAction(action);

                return new Link(content, annotation).SetUnderline();
            }
        }

        public class BorderlessCell : Cell
        {
            public BorderlessCell(int rowSpan, int colSpan) : base(rowSpan, colSpan) { }
            public BorderlessCell() : base() { }
            public override T1 GetDefaultProperty<T1>(int property)
            {
                switch (property)
                {
                    case Property.BORDER:
                        return (T1)(Object)(Border.NO_BORDER);
                    default:
                        return base.GetDefaultProperty<T1>(property);
                }
            }
        }

        public class QtyCell : Cell
        {
            public QtyCell() : base()
            {
                SetBorder(Border.NO_BORDER);
                SetBorderRight(new SolidBorder(1));
            }
        }

        public class DescriptionCell : Cell
        {
            public DescriptionCell() : base(0, 6)
            {
                SetBorder(Border.NO_BORDER);
                SetBorderRight(new SolidBorder(1));
            }
        }

        public class FooterCell : Cell
        {
            public FooterCell()
            {
                SetBorder(new SolidBorder(1));
                SetBorderBottom(Border.NO_BORDER);
            }

            public FooterCell(int rowspan, int columnspan, bool topBorder) : base(rowspan, columnspan)
            {
                SetBorder(new SolidBorder(1));
                SetBorderBottom(Border.NO_BORDER);
                if (!topBorder)
                    SetBorderTop(Border.NO_BORDER);
            }
        }

    }
}
