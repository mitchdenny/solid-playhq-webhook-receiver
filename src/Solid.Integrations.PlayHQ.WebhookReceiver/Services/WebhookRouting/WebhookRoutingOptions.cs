namespace Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting
{
    public class WebhookRoutingOptions
    {
        public const string WebhookRoutingOptionsSectionKey = "WebhookRouting";
        public string? WebhookSecret { get; set; }

        public int EventHubClientCacheSlidingExpirationSeconds { get; set; } = 3600;

        public Dictionary<string, RoutingRule>? Rules { get; set; }

        public int ClockDriftAllowanceInSeconds { get; set; } = 600;
    }
}
