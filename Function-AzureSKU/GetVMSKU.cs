using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using System.Net.Http;
using System.Text;

namespace Function_AzureSKU
{
    public static class GetVMSKU
    {
        [FunctionName("GetVMSKU")]
        [StorageAccount("BlobConnection")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            IBinder binder,
            ILogger log)
        {
            //Authenticate with SP
            var credential = new EnvironmentCredential();
            var token = credential.GetToken(new Azure.Core.TokenRequestContext(
                new[] { "https://management.azure.com/.default" }));

            //Call Azure Management API
            string endpoint = "<Azure Management API>";
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", " bearer " + token.Token);
            var response = await httpClient.GetAsync(endpoint);
            var result = await response.Content.ReadAsStringAsync();

            //Dynamic output binding for Azure Storage
            //Define file name and return file name
            var outputId = Guid.NewGuid().ToString() + ".json";
            var outputBlob = new BlobAttribute($"raw-data/{outputId}", FileAccess.Write);
            using var writer = binder.Bind<Stream>(outputBlob);
            await writer.WriteAsync(Encoding.ASCII.GetBytes(result));

            return new OkObjectResult(outputId);
        }
    }
}
