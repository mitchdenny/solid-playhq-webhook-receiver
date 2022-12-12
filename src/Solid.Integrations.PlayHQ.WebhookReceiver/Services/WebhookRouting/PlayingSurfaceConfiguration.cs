using System.Text.Json.Serialization;

namespace Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting
{
    public class PlayingSurfaceConfiguration
    {
        [JsonPropertyName("connectionString")]
        public string? ConnectionString { get; set; }
    }
}
