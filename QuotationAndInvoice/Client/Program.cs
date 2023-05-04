using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Net.Http.Headers;
using QuotationAndInvoice.Client;
using System.Net.Http;

namespace QuotationAndInvoice.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            builder.Services.AddHttpClient("LocalApi", httpClient =>
            {
                httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);

                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.Accept, "application/json");
            });
            await builder.Build().RunAsync();
        }
    }
}