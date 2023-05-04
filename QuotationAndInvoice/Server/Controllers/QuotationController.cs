using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3.Model;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using QuotationAndInvoice.Server.Data;
using QuotationAndInvoice.Server.Filters;
using QuotationAndInvoice.Shared;
using System.Net;
using QuotationAndInvoice.Server.Models;
using Microsoft.VisualBasic;
using System.Xml.Linq;
using QuotationAndInvoice.Server.Services;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace QuotationAndInvoice.Server.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class QuotationController : ControllerBase
    {
        private readonly IDynamoDBContext dBContext;
        private readonly IWebHostEnvironment env;
        private readonly IAmazonS3 amazonS3;
        private readonly ILogger<QuotationController> logger;
        private readonly IPdfGenerator fileGenerator;
        private readonly IMailSender mailSender;
        private const string BucketName = "quotationandinvoice";
        private readonly string[] _extensions = new string[] { ".jpg", ".png", ".jpeg", ".bmp" };

        public QuotationController(
            IDynamoDBContext dBContext,
            IWebHostEnvironment env,
            IAmazonS3 amazonS3,
            ILogger<QuotationController> logger,
            IPdfGenerator fileGenerator,
            IMailSender mailSender)
        {
            this.dBContext = dBContext;
            this.env = env;
            this.amazonS3 = amazonS3;
            this.logger = logger;
            this.fileGenerator = fileGenerator;
            this.mailSender = mailSender;
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromForm] QuotationUploadModel quotationModel)
        {
            var quotationDto = quotationModel.QuotationDto;

            var quotation = new Quotation();
            quotation.No = quotationDto.No;
            quotation.Customer = quotationDto.Customer;
            quotation.Images = new List<string>();
            quotation.Tasks = quotationDto.Tasks.Select(x => new QuotationTask() { Description = x.Description, Total = x.Total }).ToList();
            quotation.Date = quotationDto.Date;
            quotation.EmailAddress = quotationDto.ToEmail;

            await dBContext.SaveAsync(quotation);

            var attachments = new List<AttachmentContent>();

            var maxAllowedFiles = 40;
            long maxFileSize = 1024 * 1024 * 10;
            var filesProcessed = 0;

            foreach (var file in quotationModel.Files)
            {
                if (filesProcessed < maxAllowedFiles)
                {
                    if (file.Length == 0)
                    {
                        logger.LogInformation("{FileName} length is 0 (Err: 1)",
                            file.FileName);
                        // err
                    }
                    else if (file.Length > maxFileSize)
                    {
                        logger.LogInformation("{FileName} of {Length} bytes is " +
                            "larger than the limit of {Limit} bytes (Err: 2)",
                            file.FileName, file.Length, maxFileSize);
                        // err
                    }
                    else
                    {
                        try
                        {
                            attachments.Add(new AttachmentContent(file.OpenReadStream(), file.FileName));
                            logger.LogInformation("{FileName} saved added to attachments", file.FileName);
                        }
                        catch (IOException ex)
                        {
                            logger.LogError("{FileName} error on upload (Err: 3): {Message}",
                                file.FileName, ex.Message);
                            // err
                        }
                    }

                    filesProcessed++;
                }
                else
                {
                    logger.LogInformation("{FileName} not uploaded because the " +
                        "request exceeded the allowed {Count} of files (Err: 4)",
                        file.FileName, maxAllowedFiles);
                    // err
                }
            }
            var fn = string.Format("Quotation{0}.pdf", quotation.No);
            var bytes = await fileGenerator.GeneratePdfAsync(quotation);
            attachments.Add(new AttachmentContent(bytes, fn));
            logger.LogInformation("{FileName} added to attachments", fn);


            await mailSender.SendMailAsync(quotation.EmailAddress, attachments);


            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetPdf([FromQuery] int quotationNo)
        {
            var quotation = await dBContext.LoadAsync<Quotation>(quotationNo);

            if (quotation == null)
                return NotFound();

            var bytes = await fileGenerator.GeneratePdfAsync(quotation);
            var fn = string.Format("Quotation{0}.pdf", quotation.No);
            return File(bytes, "application/octet-stream", fn);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var quotations = await dBContext.ScanAsync<Quotation>(null).GetRemainingAsync();

            var dtos = quotations.Select(q => new QuotationDto()
            {
                Customer = q.Customer,
                Images = q.Images,
                No = q.No,
                Date = q.Date,
                Tasks = q.Tasks.Select(task => new QuotationTaskDto()
                {
                    Description = task.Description,
                    Total = task.Total
                }).ToList(),
                ToEmail = q.EmailAddress
            }).OrderByDescending(x => x.No);

            return Ok(dtos);
        }

        [HttpGet]
        public async Task<IActionResult> GetMaxQuotationNo()
        {
            var quotations = await dBContext.ScanAsync<Quotation>(null).GetRemainingAsync();
            if (quotations.Any())
                return Ok(quotations.Max(x => x.No));
            else
                return Ok(0);
        }


        /* put s3
           //var fn = string.Format("Quotation{0}.pdf", q.No);

            //PutObjectRequest putObjectRequest = new PutObjectRequest()
            //{
            //    BucketName = BucketName,
            //    Key = fn,
            //    InputStream = new MemoryStream(bytes)
            //};

            //// Saving PDF to S3
            //var putObjectResult = await amazonS3.PutObjectAsync(putObjectRequest);
            
            //if (putObjectResult.HttpStatusCode == HttpStatusCode.OK)
            //{
            //    q.PdfFileName = string.Format("https://{0}.s3.amazonaws.com/{1}", BucketName, fn);
            //    // Saving all to Dynamo
            //    await dBContext.SaveAsync(q);
            //}
            //else
            //    throw new Exception("Error putting object to s3 bucket: " + putObjectResult.HttpStatusCode.ToString());
         */
    }
}
