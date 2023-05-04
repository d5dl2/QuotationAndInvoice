using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Annot;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace ConsoleTests
{
    internal class Program
    {
        static string fileName = "C:/Temp/Tessst.pdf";
        static string jpg = "C:/Temp/colorwise.jpg";
        static void Main(string[] args)
        {

            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
            PdfWriter writer = new PdfWriter(fs);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);
            
            AddHeader(document);
            AddImage(document);
            AddContactTable(document);
            AddTasksTable(document);
            document.Close();
        }

        private static void AddHeader(Document document)
        {
            document.Add(new Paragraph("QUOTATION").
                SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).
                SetFontSize(20).
                SetBold());
        }
        private static void AddImage(Document document)
        {
            document.Add(new Image(ImageDataFactory.Create(jpg)));
        }

        private static void AddContactTable(Document document)
        {
            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 20, 80 })).
                SetWidth(UnitValue.CreatePercentValue(50));

            foreach (var info in ContactInfo.Infos)
            {
                table.AddCell(new Cell().
                    Add(new Paragraph(info.Key).
                        SetBold().
                        SetFontSize(10)).
                    SetBorder(Border.NO_BORDER));

                table.AddCell(new Cell().
                Add(info.Value).
                SetBorder(Border.NO_BORDER));
            }

            document.Add(table);
        }
        
        private static void AddTasksTable(Document document)
        {
            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 })).
                SetWidth(UnitValue.CreatePercentValue(100)).
                SetBorder(new SolidBorder(1)).
                SetFontSize(10);

            table.AddCell(GetDefaultCell("DATE: ", 3, DateTime.Now.ToShortDateString()));
            table.AddCell(GetDefaultCell("To: ", 4, "R&P Construct"));
            table.AddCell(GetDefaultCell("Quotation No: ", 3, "11532"));
            table.AddCell(GetDefaultCell("QTY"));
            table.AddCell(GetDefaultCell("DESCRIPTION", 6));
            table.AddCell(GetDefaultCell("Qty"));
            table.AddCell(GetDefaultCell("Price"));
            table.AddCell(GetDefaultCell("Total"));

            Dictionary<string, string> giders = new Dictionary<string, string>();
            giders.Add("Gider 1", "$ 1550");
            giders.Add("Gider 2", "$ 250");
            giders.Add("Gider 3", "$ 220");
            giders.Add("Gider 4", "$ 1255");

            var counter = 1;
            foreach (var gider in giders)
            {
                table.AddCell(new Cell().
                    Add(new Paragraph(counter.ToString())));
                table.AddCell(new Cell(0, 6).
                    Add(new Paragraph(gider.Key)));
                table.AddCell(new Cell());
                table.AddCell(new Cell());
                table.AddCell(new Cell().
                    Add(new Paragraph(gider.Value)).
                    SetTextAlignment(TextAlignment.RIGHT));

                counter++;
            }

            Dictionary<string, string> footers = new Dictionary<string, string>();
            footers.Add("SUBTOTAL", "$ 3275");
            footers.Add("GST", "$ 327.5");
            footers.Add("TOTAL INCLUSIVE OF GST", "$ 3602.5");

            foreach (var footer in footers)
            {
                table.AddCell(new Cell(0, 9).
               Add(new Paragraph(footer.Key)).
               SetTextAlignment(TextAlignment.RIGHT).
               SetBorder(Border.NO_BORDER));
                table.AddCell(new Cell(0, 9).
                    Add(new Paragraph(footer.Value)).
                    SetTextAlignment(TextAlignment.RIGHT));
            }

            document.Add(table);
        }

        private static Cell GetDefaultCell(string content, int columnSpan = 0, string newParagContent ="")
        {
            if (string.IsNullOrWhiteSpace(newParagContent))
                return new Cell(0, columnSpan).
                    Add(new Paragraph(content).
                        SetBold()).
                    SetBorder(new SolidBorder(1)).
                    SetTextAlignment(TextAlignment.CENTER);
            else
                return new Cell(0, columnSpan).
                    Add(new Paragraph(content).
                        SetBold()).
                    Add(new Paragraph(newParagContent)).
                    SetBorder(new SolidBorder(1));
        }

        static class ContactInfo
        {
            static ContactInfo()
            {
                Infos = new Dictionary<string, IBlockElement>();

                Infos.Add("Adress", new Paragraph("6 Amalfi Court Roxburgh Park VIC 3064").
                    SetFontSize(10));
                Infos.Add("Tel", new Paragraph("0061 414 385 952").
                    SetFontSize(10));

                Infos.Add("Email", new Paragraph().
                    Add(CreateLink("info@colorwise.com.au", "mailto:info@colorwise.com.au")).
                    SetFontSize(10));
                Infos.Add("Web", new Paragraph().
                    Add(CreateLink("www.colorwise.com.au", "www.colorwise.com.au").
                    SetFontSize(10)));

                Infos.Add("ABN", new Paragraph("18 358 453 585"));
            }

            static Text CreateLink(string content, string url)
            {

                Rectangle rect = new Rectangle(0, 0);
                PdfLinkAnnotation annotation = new PdfLinkAnnotation(rect);
                PdfAction action = PdfAction.CreateURI(url);
                annotation.SetUriAction(action);

                return new Link(content, annotation).SetUnderline();
            }
            public static Dictionary<string, IBlockElement> Infos { get; set; }


        }      
    }

}