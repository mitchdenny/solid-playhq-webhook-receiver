using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Solid.Integrations.PlayHQ.Common;
using Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Options;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Azure;
using System.Security.Cryptography.Xml;
using System.Threading;
using System.Globalization;

namespace Solid.Integrations.PlayHQ.WebhookReceiver;

public class Program
{
    private static IConfigurationRefresher? refresher;

    public static async Task<int> Main(string[] args)

    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddAzureAppConfiguration(options =>
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
            builder.Services.AddSingleton<IConfigurationRefresher>(refresher);
        });

        builder.Services.AddApplicationInsightsTelemetry();
        builder.Services.AddLogging();
        builder.Services.AddMemoryCache();
        builder.Services.AddWebhookRouter(builder.Configuration);
        builder.Services.AddAzureAppConfiguration();

        builder.Logging.AddApplicationInsights();

        var app = builder.Build();
        app.UseAzureAppConfiguration();

        app.MapPost("/tenants/{webhookId}/events", RouteEventAsync);
        app.MapGet("/tenants/{tenantId}/playing-surfaces/{playingSurfaceId}/config", GetPlayingSurfaceConfigAsync);
        app.MapGet("/tenants/{tenantId}/playing-surfaces/{playingSurfaceId}/ping", PingAsync);
        app.MapPost("/webhook/{webhookId}", RouteEventAsync);
        app.MapGet("/liveness", GetHealthAsync);
        app.MapGet("/readiness", GetHealthAsync);

        await app.RunAsync("http://*:3000");

        return 0;
    }

    private static async Task<IResult> PingAsync([FromServices]ILogger<Program> logger, [FromServices]IWebhookRouter webhookRouter, [FromRoute]Guid tenantId, [FromRoute]Guid playingSurfaceId, CancellationToken cancellationToken)
    {
        var eventTimeStamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

        var pingEventJson = $@"
{{
    ""messageId"": ""{Guid.NewGuid()}"",
    ""eventType"": ""TESTING.PING_EVENT"",
    ""entityId"": ""{Guid.Empty}"",
    ""eventRaisedDateTime"": ""{eventTimeStamp}"",
    ""filters"": [
        {{
            ""entityType"": ""PLAYING_SURFACE"",
            ""entityId"": ""{playingSurfaceId}""
        }}
    ]
}}
";

        var body = JsonDocument.Parse(pingEventJson);
        return await RouteEventAsync(
            logger,
            webhookRouter,
            tenantId,
            body,
            SignatureHelper.BypassSignature,
            cancellationToken);
    }

    private static async Task<IResult> GetPlayingSurfaceConfigAsync([FromServices] ILogger<Program> logger, [FromServices] IWebhookRouter webhookRouter, HttpRequest request, [FromHeader]string signature, [FromRoute] Guid tenantId, [FromRoute]Guid playingSurfaceId, [FromQuery]string nonce, [FromQuery]int expiry, CancellationToken cancellationToken)
    {
        try
        {
            var expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(expiry);
            var playingSurfaceConfiguration = await webhookRouter.GetPlayingSurfaceConfigurationAsync(tenantId, playingSurfaceId, request.GetUri().AbsoluteUri, nonce, expiryDateTime, signature, cancellationToken);
            return TypedResults.Ok(playingSurfaceConfiguration);
        }
        catch (WebhookRouterException ex) when (ex.Message == "Invalid signature.")
        {
            return Results.Unauthorized();
        }
        catch (WebhookRouterException ex) when (ex.Message == "No playing surface mapping.")
        {
            return Results.NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unknown error occured processing webhook.");
            throw;
        }
    }

    private static async Task<IResult> GetHealthAsync([FromServices]IConfigurationRefresher refresher, [FromServices] ILogger<Program> logger, HttpResponse response)
    {
        await refresher.RefreshAsync();
        response.StatusCode = 200;
        return Results.Ok("Healthy!");
    }

    private static async Task<IResult> RouteEventAsync([FromServices] ILogger<Program> logger, [FromServices] IWebhookRouter webhookRouter, [FromRoute] Guid webhookId, [FromBody] JsonDocument body, [FromHeader(Name = "Signature")] string signature, CancellationToken cancellationToken)
    {
        try
        {
            await webhookRouter.RouteAsync(webhookId, body, signature, cancellationToken);
            return Results.Ok();
        }
        catch (WebhookRouterException ex) when(ex.Message == "Invalid signature.")
        {
            return Results.Unauthorized();
        }
        catch (WebhookRouterException ex) when(ex.Message == "No routing rule.")
        {
            return Results.NotFound();
        }
        catch (WebhookRouterException ex) when(ex.Message == "No playing surface mapping.")
        {
            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unknown error occured processing webhook.");
            throw;
        }
    }
}

