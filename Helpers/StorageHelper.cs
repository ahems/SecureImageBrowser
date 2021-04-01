using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using WebApp_OpenIDConnect_DotNet.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using WebAppOpenIDConnectDotNet;

namespace WebApp_OpenIDConnect_DotNet.Helpers
{
    public static class StorageHelper
    {

        public static bool IsImage(string fileName)
        {
            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };
            return formats.Any(item => fileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<List<string>> GetThumbNailUrls(AzureStorageConfig _storageConfig, TokenAcquisitionTokenCredential tokenCredential, TelemetryClient telemetryClient, string pathBase)
        {
            List<string> thumbnailUrls = new List<string>();

            // Create a URI to the storage account
            Uri blobUri = new Uri(_storageConfig.FullAccountName);

            BlobServiceClient blobServiceClient = new BlobServiceClient(blobUri, tokenCredential, null);

            // Get reference to the container
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(_storageConfig.ImageContainer);

            if (container.Exists())
            {                
                foreach (BlobItem blobItem in container.GetBlobs())
                {
                    if(IsImage(blobItem.Name)) {
                        string imageUrl = string.Format("{0}api/images/thumbnail?ImageName={1}", pathBase, blobItem.Name);
                        thumbnailUrls.Add(imageUrl);
                        telemetryClient.TrackTrace("Found Thumbnail - " + imageUrl);
                    }
                }
            } else {
                telemetryClient.TrackException(new ArgumentNullException(string.Format("Container {0} not found!", container.Name)));
                container.CreateIfNotExists(PublicAccessType.None, null, null);
            }

            return await Task.FromResult(thumbnailUrls);
        }

        public static async Task<BlobDownloadInfo> GetThumbNail(AzureStorageConfig _storageConfig, TokenAcquisitionTokenCredential tokenCredential, TelemetryClient telemetryClient, string imageName) {

            Uri blobUri = new Uri(string.Format("https://{0}.blob.core.windows.net/{1}/{2}", _storageConfig.AccountName, _storageConfig.ImageContainer, imageName));
            BlobClient blobClient = new BlobClient(blobUri, tokenCredential, null);
            return await blobClient.DownloadAsync();
        }

    }
}
