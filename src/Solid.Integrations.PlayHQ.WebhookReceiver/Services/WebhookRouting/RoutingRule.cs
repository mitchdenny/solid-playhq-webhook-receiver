namespace Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting
{
    public record RoutingRule
    {
        public string? WebhookSecret { get; set; }

        public Dictionary<string, PlayingSurfaceMapping>? Mappings { get; set; }
    }
}
