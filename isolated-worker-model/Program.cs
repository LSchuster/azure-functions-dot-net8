using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddBlobServiceClient(builder.Configuration.GetSection("HealthCheckStorageConnection"))
                .WithName("healthCheckBlobClient");

            clientBuilder.AddServiceBusClient(builder.Configuration.GetSection("ServiceBusConnection"))
                .WithName("serviceBusClient");
        });

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();