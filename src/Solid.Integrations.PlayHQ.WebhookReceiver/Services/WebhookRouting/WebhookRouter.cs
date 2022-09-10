using Solid.Integrations.PlayHQ.WebhookReceiver.Helpers;
using System.Security.Cryptography.Xml;
using System.Text.Json;

namespace Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting
{
    public class WebhookRouter : IWebhookRouter
    {
        public async Task RouteAsync(Guid webhookId, JsonDocument body, string signature, CancellationToken cancellationToken = default)
        {
            var payload = body.RootElement.ToString();
            var isSignatureValid = SignatureValidator.IsValidSignature(payload, "open sesame", signature);
            if (!isSignatureValid) throw new WebhookRouterException("Invalid signature.");

        }
    }
}
