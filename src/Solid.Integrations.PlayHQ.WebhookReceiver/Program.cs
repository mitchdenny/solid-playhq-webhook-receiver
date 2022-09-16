using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Solid.Integrations.PlayHQ.WebhookReceiver.Helpers;
using Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Options;

namespace Solid.Integrations.PlayHQ.WebhookReceiver;

public static class Program
{
    private static IConfigurationRefresher refresher;

    public static async Task<int> Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.ConfigureAppConfiguration(builder =>
        {
            builder.AddAzureAppConfiguration(options =>
            {
                var configUrl = Environment.GetEnvironmentVariable("CONFIG_URL") ?? throw new ApplicationException();
                options.Connect(new Uri(configUrl), new DefaultAzureCredential())
                       .ConfigureKeyVault(keyVaultOptions =>
                       {
                           keyVaultOptions.SetCredential(new DefaultAzureCredential());
                       })
                       .ConfigureRefresh(refreshOptions =>
                        {
                            refreshOptions.Register("Refresh", true);
                        });

                refresher = options.GetRefresher();
            });
            builder.Build();
        });

        builder.Services.AddLogging();
        builder.Services.AddMemoryCache();
        builder.Services.AddWebhookRouter(builder.Configuration);
        builder.Services.AddAzureAppConfiguration();

        var app = builder.Build();
        app.UseAzureAppConfiguration();

        app.MapPost("/webhook/{webhookId}", async ([FromServices]IWebhookRouter webhookRouter, [FromRoute]Guid webhookId, [FromBody]JsonDocument body, [FromHeader(Name = "Signature")]string signature, HttpResponse response, CancellationToken cancellationToken) => {

            try
            {
                await webhookRouter.RouteAsync(webhookId, body, signature, cancellationToken);
                response.StatusCode = 200;
            }
            catch (WebhookRouterException ex) when (ex.Message == "Invalid signature.")
            {
                response.StatusCode = 401;
            }
        });

        app.MapGet("/health", async (HttpResponse response) =>
        {
            await refresher.RefreshAsync();
            response.StatusCode = 200;
            await response.WriteAsync("Healthy!");
        });

        await app.RunAsync("http://*:3000");

        return 0;
    }
}

