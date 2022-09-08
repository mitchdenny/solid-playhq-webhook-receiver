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
        builder.Services.AddSingleton<IWebhookRouter, WebhookRouter>();

        var app = builder.Build();

        app.MapPost("/webhook/{webhookId}", async ([FromServices]IWebhookRouter webhookRouter, [FromRoute]Guid webhookId, [FromBody]JsonDocument body, [FromHeader(Name = "Signature")]string signature, HttpResponse response) => {

            var payload = body.RootElement.ToString();
            var isSignatureValid = SignatureValidator.IsValidSignature(payload, "open sesame", signature);

            if (isSignatureValid)
            {
                await webhookRouter.RouteAsync(webhookId, body);
                response.StatusCode = 200;
            }
            else
            {
                response.StatusCode = 401;
            }
        });

        await app.RunAsync("http://localhost:3000");

        return 0;
    }
}

