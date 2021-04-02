using WebApp_OpenIDConnect_DotNet.Helpers;
using WebApp_OpenIDConnect_DotNet.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Identity.Web;
using WebAppOpenIDConnectDotNet;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        // make sure that appsettings.json is filled with the necessary details of the azure storage
        private readonly AzureStorageConfig storageConfig = null;
        private TelemetryClient telemetry;
        ITokenAcquisition _tokenAcquisition;

        public ImagesController(IOptions<AzureStorageConfig> config, TelemetryClient telemetry, ITokenAcquisition tokenAcquisition)
        {
            _tokenAcquisition = tokenAcquisition;
            storageConfig = config.Value;
            this.telemetry = telemetry;
        }

        // GET /api/images/thumbnails
        [AuthorizeForScopes(ScopeKeySection = "AzureAd:Scopes")]
        [HttpGet("thumbnails")]
        public async Task<IActionResult> GetThumbNails()
        {
            try
            {
                if (storageConfig.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in Azure blob storage.");

                List<string> thumbnailUrls = await StorageHelper.GetThumbNailUrls(storageConfig, new TokenAcquisitionTokenCredential(_tokenAcquisition), this.telemetry, Request.PathBase);
                return new ObjectResult(thumbnailUrls);
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                await _tokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeaderAsync(new string[] { storageConfig.Scope }, ex.MsalUiRequiredException);
                return new OkResult();
            }
            catch (Microsoft.Identity.Client.MsalUiRequiredException ex)
            {
                await _tokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeaderAsync(new string[] { storageConfig.Scope }, ex);
                return new OkResult();
            }
            catch (Exception ex)
            {
                this.telemetry.TrackException(ex);
                return BadRequest(ex.Message);
            }
        }
        // GET /api/images/thumbnail
        [AuthorizeForScopes(ScopeKeySection = "AzureAd:Scopes")]
        [HttpGet("thumbnail")]
        public async Task<IActionResult> GetThumbNail()
        {
            try
            {
                if (storageConfig.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in Azure blob storage.");
                
                if (!Request.QueryString.HasValue)
                    return BadRequest("Missing ImageName");
                
                string imageName = Request.QueryString.Value.Split('=')[1];

                var result = await StorageHelper.GetThumbNail(storageConfig, new TokenAcquisitionTokenCredential(_tokenAcquisition), this.telemetry, imageName);
                return new ObjectResult(result.Content);                     
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                await _tokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeaderAsync(new string[] { storageConfig.Scope }, ex.MsalUiRequiredException);
                return new OkResult();
            }
            catch (Microsoft.Identity.Client.MsalUiRequiredException ex)
            {
                await _tokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeaderAsync(new string[] { storageConfig.Scope }, ex);
                return new OkResult();
            }
            catch (Exception ex)
            {
                this.telemetry.TrackException(ex);
                return BadRequest(ex.Message);
            }
        }
    }
}