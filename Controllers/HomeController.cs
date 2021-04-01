using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Models;
using WebApp_OpenIDConnect_DotNet.Helpers;
using Microsoft.Identity.Web;
using WebAppOpenIDConnectDotNet;
using Azure.Storage.Blobs;
using System.Text;
using System.IO;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        ITokenAcquisition _tokenAcquisition;
        
        // make sure that appsettings.json is filled with the necessary details of the azure storage
        private readonly AzureStorageConfig storageConfig = null;
        private TelemetryClient telemetry;

        public HomeController(IOptions<AzureStorageConfig> config, TelemetryClient telemetry, ITokenAcquisition tokenAcquisition)
        {
            _tokenAcquisition = tokenAcquisition;
            storageConfig = config.Value;
            this.telemetry = telemetry;
        }

        [AuthorizeForScopes(Scopes = new string[] { "https://storage.azure.com/user_impersonation" })]
        public IActionResult Index()
        {
            return View(new IndexViewModel() { 
                    StorageAccountName  = storageConfig.FullAccountName,
                    ImageUrls = StorageHelper.GetThumbNailUrls(storageConfig, new TokenAcquisitionTokenCredential(_tokenAcquisition), this.telemetry, string.Format("https://{0}/", Request.Host)).GetAwaiter().GetResult() 
                } );
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }

        [AuthorizeForScopes(Scopes = new string[] { "https://storage.azure.com/user_impersonation" })]
        public async Task<IActionResult> Blob()
        {
            string message = await CreateBlob(new TokenAcquisitionTokenCredential(_tokenAcquisition));
            ViewData["Message"] = message;
            return View();
        }

        private static async Task<string> CreateBlob(TokenAcquisitionTokenCredential tokenCredential)
        {
            Uri blobUri = new Uri("https://sportsleaguestorage.blob.core.windows.net/test/Blob1.txt");
            BlobClient blobClient = new BlobClient(blobUri, tokenCredential);

            string blobContents = "Blob created by Azure AD authenticated user.";
            byte[] byteArray = Encoding.ASCII.GetBytes(blobContents);

            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                await blobClient.UploadAsync(stream);
            }
            return "Blob successfully created";
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
