using System.Text.Json;

namespace Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting
{
    public interface IWebhookRouter
    {
        Task RouteAsync(Guid webhookId, JsonDocument body);
    }
}
