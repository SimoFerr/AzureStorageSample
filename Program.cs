using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using System.IO;

namespace BlobManager
{
    class Program
    {
        static IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

        static async Task Main(string[] args)
        {
            StorageSharedKeyCredential accountCredentials = new StorageSharedKeyCredential(config["StorageAccountName"], config["StorageAccountKey"]);

            BlobServiceClient serviceClient = new BlobServiceClient(new Uri(config["ServiceEndpoint"]), accountCredentials);

            AccountInfo info = await serviceClient.GetAccountInfoAsync();
            await EnumerateContainersAsync(serviceClient);

            string existingContainerName = "raster-graphics";
            await EnumerateBlobsAsync(serviceClient, existingContainerName);

            string newContainerName = "vector-graphics";
            BlobContainerClient containerClient = await GetContainerAsync(serviceClient, newContainerName);

            string uploadedBlobName = "graph.svg";
            BlobClient blobClient = await GetBlobAsync(containerClient, uploadedBlobName);
            await Console.Out.WriteLineAsync($"Blob Url:\t{blobClient.Uri}");
        }

        private static async Task EnumerateContainersAsync(BlobServiceClient client)
        {        
            await foreach (BlobContainerItem container in client.GetBlobContainersAsync())
            {
                await Console.Out.WriteLineAsync($"Container:\t{container.Name}");
            }
        }

        private static async Task EnumerateBlobsAsync(BlobServiceClient client, string containerName)
        {      
            BlobContainerClient container = client.GetBlobContainerClient(containerName);
            await Console.Out.WriteLineAsync($"Searching:\t{container.Name}");
            
            await foreach (BlobItem blob in container.GetBlobsAsync())
            {
                await Console.Out.WriteLineAsync($"Existing Blob:\t{blob.Name}");
            }
        }

        private static async Task<BlobContainerClient> GetContainerAsync(BlobServiceClient client, string containerName)
        {      
            BlobContainerClient container = client.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

            await Console.Out.WriteLineAsync($"New Container:\t{container.Name}");

            return container;
        }

        private static async Task<BlobClient> GetBlobAsync(BlobContainerClient client, string blobName)
        {
            BlobClient blob = client.GetBlobClient(blobName);      
             await Console.Out.WriteLineAsync($"Blob Found:\t{blob.Name}");
             return blob;
        }
    }

    public class MySettingsConfig
    {
        public string ServiceEndpoint { get; set; }
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
    }
}
