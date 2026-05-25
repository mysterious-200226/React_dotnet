using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace PHP.QARAdjustmentTool.API.Services
{
    public class AzureBlobStorageService
    {
            private readonly BlobContainerClient _containerClient;

            public AzureBlobStorageService(IConfiguration configuration)
            {
                var connectionString =
                    configuration["Storage:ConnectionString"];

                var containerName =
                    configuration["Storage:ContainerName"];

                _containerClient = new BlobContainerClient(
                    connectionString,
                    containerName
                );
            }

            // ============================================
            // GET FILE
            // ============================================

            public async Task<Stream?> GetFileAsync(string blobPath)
            {
                var blobClient =
                    _containerClient.GetBlobClient(blobPath);

                if (!await blobClient.ExistsAsync())
                    return null;

                var response =
                    await blobClient.DownloadStreamingAsync();

                return response.Value.Content;
            }
    }
}
