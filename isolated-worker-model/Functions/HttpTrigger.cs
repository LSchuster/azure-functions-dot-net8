using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using System.Text;
using Azure.Messaging.ServiceBus;

namespace Functions.Isoladed;

public class HttpTrigger(ILogger<HttpTrigger> logger, IAzureClientFactory<BlobServiceClient> blobClientFactory, IAzureClientFactory<ServiceBusClient> serviceBusClientFactory)
{
    private readonly ILogger<HttpTrigger> _logger = logger;
    private readonly BlobContainerClient _healthCheckContainerClient = blobClientFactory.CreateClient("healthCheckBlobClient").GetBlobContainerClient("health-check-data");
    private readonly ServiceBusClient _serviceBusClient = serviceBusClientFactory.CreateClient("serviceBusClient");

    [Function("HealthCheck")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest request)
    {
        _logger.LogInformation("HealthCheck requested.");

        try
        {
            // Blob Storage example
            await _healthCheckContainerClient.CreateIfNotExistsAsync();
            var blobName = $"{Guid.NewGuid()}.txt";
            string content = "Health check data";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            await _healthCheckContainerClient.UploadBlobAsync(blobName, stream);

            // Service Bus example
            var sender = _serviceBusClient.CreateSender("health-check-queue");
            var message = new ServiceBusMessage("Health check completed");
            await sender.SendMessageAsync(message);

            return new OkObjectResult($"Blob '{blobName}' uploaded and Service Bus message sent.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while uploading the blob.");
            return new StatusCodeResult(500);
        }
    }
}
