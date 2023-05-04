using Microsoft.AspNetCore.ResponseCompression;
using QuotationAndInvoice.Server.Data;
using QuotationAndInvoice.Shared;
using System.Reflection;
using Amazon.S3;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Amazon.Extensions.NETCore.Setup;
using QuotationAndInvoice.Server.Services;
using OfficeOpenXml;

namespace QuotationAndInvoice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Add services to the container.

            builder.Services.AddControllersWithViews(c => c.AllowEmptyInputInBodyModelBinding = true)
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        if (context.HttpContext.Request.Path == "/Quotation/Save")
                        {
                            return new BadRequestObjectResult(context.ModelState);
                        }
                        else
                        {
                            return new BadRequestObjectResult(
                                new ValidationProblemDetails(context.ModelState));
                        }
                    };
                });
            builder.Services.AddRazorPages();

            builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
            builder.Services.AddAWSService<IAmazonS3>();
            builder.Services.AddAWSService<IAmazonDynamoDB>(new AWSOptions()
            {

            });
            builder.Services.AddTransient<IDynamoDBContext, DynamoDBContext>();
            builder.Services.AddScoped<IPdfGenerator, PdfGenerator>();
            builder.Services.AddScoped<IMailSender, MailSender>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                app.UseExceptionHandler("/error-development");
            }
            else
            {
                app.UseExceptionHandler("/error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();


            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}