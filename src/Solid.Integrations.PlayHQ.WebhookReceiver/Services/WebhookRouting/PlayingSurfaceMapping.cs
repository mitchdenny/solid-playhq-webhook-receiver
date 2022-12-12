namespace Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting
{
    public class PlayingSurfaceMapping
    {
        public string? EventHubsNamespace { get; set; }
        public string? EventHubName { get; set; }
        public string? SharedSecret { get; set; }
        public string? ConnectionString { get; set;  }
    }
}
