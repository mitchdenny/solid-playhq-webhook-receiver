using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Solid.Integrations.PlayHQ.WebhookReceiver.Helpers;
using Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting;

namespace Solid.Integrations.PlayHQ.WebhookReceiver;

public static class Program
{


    public static async Task<int> Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddLogging();
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IWebhookRouter, WebhookRouter>();

        var app = builder.Build();

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

        await app.RunAsync("http://localhost:3000");

        return 0;
    }
}

