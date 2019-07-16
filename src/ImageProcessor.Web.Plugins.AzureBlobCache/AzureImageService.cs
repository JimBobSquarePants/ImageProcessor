using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ImageProcessor.Web.Caching;
using ImageProcessor.Web.Helpers;
using ImageProcessor.Web.Services;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace ImageProcessor.Web.Plugins.AzureBlobCache
{
    /// <summary>
    /// An image service for retrieving images from Azure.
    /// </summary>
    public class AzureImageService : IImageService
    {
        private CloudBlobContainer blobContainer;
        private Dictionary<string, string> settings = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the prefix for the given implementation.
        /// <remarks>
        /// This value is used as a prefix for any image requests that should use this service.
        /// </remarks>
        /// </summary>
        public string Prefix { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the image service requests files from
        /// the locally based file system.
        /// </summary>
        public bool IsFileLocalService => false;

        /// <summary>
        /// Gets or sets any additional settings required by the service.
        /// </summary>
        public Dictionary<string, string> Settings
        {
            get => this.settings;
            set
            {
                this.settings = value;
                this.InitService();
            }
        }

        /// <summary>
        /// Gets or sets the white list of <see cref="Uri" />. 
        /// </summary>
        public Uri[] WhiteList { get; set; }

        /// <summary>
        /// Gets the image using the given identifier.
        /// </summary>
        /// <param name="id">The value identifying the image to fetch.</param>
        /// <returns>
        /// The <see cref="byte" /> array containing the image data.
        /// </returns>
        public async Task<byte[]> GetImage(object id)
        {
            CloudBlockBlob blockBlob = this.blobContainer.GetBlockBlobReference(id.ToString());

            if (blockBlob.Exists())
            {
                using (MemoryStream memoryStream = MemoryStreamPool.Shared.GetStream())
                {
                    await blockBlob.DownloadToStreamAsync(memoryStream).ConfigureAwait(false);
                    return memoryStream.ToArray();
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a value indicating whether the current request passes sanitizing rules.
        /// </summary>
        /// <param name="path">The image path.</param>
        /// <returns>
        /// <c>True</c> if the request is valid; otherwise, <c>False</c>.
        /// </returns>
        public bool IsValidRequest(string path) => ImageHelpers.IsValidImageExtension(path);

        /// <summary>
        /// Initialise the service.
        /// </summary>
        private void InitService()
        {
            // Retrieve storage accounts from connection string.
            var cloudCachedStorageAccount = CloudStorageAccount.Parse(this.Settings["StorageAccount"]);

            // Create the blob client.
            CloudBlobClient blobClient = cloudCachedStorageAccount.CreateCloudBlobClient();

            string container = this.Settings.ContainsKey("Container")
                ? this.Settings["Container"]
                : string.Empty;

            BlobContainerPublicAccessType accessType = this.Settings.ContainsKey("AccessType")
                ? (BlobContainerPublicAccessType)Enum.Parse(typeof(BlobContainerPublicAccessType), this.Settings["AccessType"])
                : BlobContainerPublicAccessType.Blob;

            this.blobContainer = CreateContainer(blobClient, container, accessType);
        }

        /// <summary>
        /// Returns the cache container, creating a new one if none exists.
        /// </summary>
        /// <param name="cloudBlobClient"><see cref="CloudBlobClient"/> where the container is stored.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="accessType"><see cref="BlobContainerPublicAccessType"/> indicating the access permissions.</param>
        /// <returns>The <see cref="CloudBlobContainer"/></returns>
        private static CloudBlobContainer CreateContainer(CloudBlobClient cloudBlobClient, string containerName, BlobContainerPublicAccessType accessType)
        {
            CloudBlobContainer container = cloudBlobClient.GetContainerReference(containerName);

            if (!container.Exists())
            {
                container.Create();
                container.SetPermissions(new BlobContainerPermissions { PublicAccess = accessType });
            }

            return container;
        }
    }
}
