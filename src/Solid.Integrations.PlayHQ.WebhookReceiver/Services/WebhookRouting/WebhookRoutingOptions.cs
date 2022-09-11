namespace Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting
{
    public class WebhookRoutingOptions
    {
        public const string WebhookRoutingOptionsSectionKey = "WebhookRouting";
        public string Option1 { get; set; }
        public string Option2 { get; set; }

        public string WebhookSecret { get; set; }
    }
}
