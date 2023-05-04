using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QuotationAndInvoice.Shared;

namespace QuotationAndInvoice.Server.Controllers
{
    public class ErrorController : Controller
    {
        public ILogger<ErrorController> Logger { get; }

        public ErrorController(ILogger<ErrorController> logger)
        {
            Logger = logger;
        }

        [Route("/error-development")]
        public IActionResult HandleErrorDevelopment([FromServices] IHostEnvironment hostEnvironment)
        {
            if (!hostEnvironment.IsDevelopment())
            {
                return NotFound();
            }

            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;
            ApiErrorDto apiError = new ApiErrorDto();
            apiError.Message = exceptionHandlerFeature.Error.ToString();

            return StatusCode(401, apiError);
        }

        [Route("/error")]
        public IActionResult HandleError() {
            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;

            Logger.LogError(exceptionHandlerFeature.Error.ToString());

            ApiErrorDto apiError = new ApiErrorDto();
            apiError.Message = exceptionHandlerFeature.Error.Message;

            return StatusCode(401, apiError);
        }
    }
}
