using System.Text.Json;

namespace Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting
{
    public class WebhookRouter : IWebhookRouter
    {
        public async Task RouteAsync(Guid webhookId, JsonDocument body)
        {
        }
    }
}
