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
using Microsoft.ApplicationInsights.AspNetCore.Extensions;

namespace Solid.Integrations.PlayHQ.WebhookReceiver;

public class Program
{
    private static IConfigurationRefresher? refresher;

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
                            refreshOptions.SetCacheExpiration(TimeSpan.FromSeconds(30));
                            refreshOptions.Register("Refresh", true);
                        });

                refresher = options.GetRefresher();
            });
            builder.Build();
        });

        builder.Services.AddApplicationInsightsTelemetry();
        builder.Services.AddLogging();
        builder.Services.AddMemoryCache();
        builder.Services.AddWebhookRouter(builder.Configuration);
        builder.Services.AddAzureAppConfiguration();

        builder.Logging.AddApplicationInsights();

        var app = builder.Build();
        app.UseAzureAppConfiguration();

        app.Use((context, next) =>
        {
            return next(context);
        });

        app.MapPost("/webhook/{webhookId}", async ([FromServices]ILogger<Program> logger, [FromServices]IWebhookRouter webhookRouter, [FromRoute]Guid webhookId, [FromBody]JsonDocument body, [FromHeader(Name = "Signature")]string signature, HttpResponse response, CancellationToken cancellationToken) => {

            try
            {
                await webhookRouter.RouteAsync(webhookId, body, signature, cancellationToken);
                response.StatusCode = 200;
            }
            catch (WebhookRouterException ex) when (ex.Message == "Invalid signature.")
            {
                response.StatusCode = 401;
            }
            catch (WebhookRouterException ex) when (ex.Message == "No routing rule.")
            {
                response.StatusCode = 404;
            }
            catch (WebhookRouterException ex) when (ex.Message == "No playing surface mapping.")
            {
                response.StatusCode = 200; // Eat this for now, we may want to avoid using exceptions for this in the 
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unknown error occured processing webhook.");
                response.StatusCode = 500;
            }
        });

        app.MapGet("/liveness", async ([FromServices] ILogger<Program> logger, HttpResponse response) =>
        {
            if (refresher == null)
            {
                logger.LogError("Refresher is null.");
                response.StatusCode = 500;
                return;
            }

            await refresher.RefreshAsync();
            response.StatusCode = 200;
            await response.WriteAsync("Healthy!");
        });

        app.MapGet("/readiness", async ([FromServices] ILogger<Program> logger, HttpResponse response) =>
        {
            if (refresher == null)
            {
                logger.LogError("Refresher is null.");
                response.StatusCode = 500;
                return;
            }

            await refresher.RefreshAsync();
            response.StatusCode = 200;
            await response.WriteAsync("Healthy!");
        });

        await app.RunAsync("http://*:3000");

        return 0;
    }
}

