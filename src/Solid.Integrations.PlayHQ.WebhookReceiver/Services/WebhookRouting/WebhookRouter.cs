using Microsoft.Extensions.Options;
using Solid.Integrations.PlayHQ.WebhookReceiver.Helpers;
using System.Security.Cryptography.Xml;
using System.Text.Json;

namespace Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting
{
    public class WebhookRouter : IWebhookRouter
    {
        private ILogger<WebhookRouter> logger;
        private IOptionsMonitor<WebhookRoutingOptions> options;

        public WebhookRouter(ILogger<WebhookRouter> logger, IOptionsMonitor<WebhookRoutingOptions> options)
        {
            this.logger = logger;
            this.options = options;
            this.options.OnChange(OnWebhookRoutingOptionsChange);
        }

        private void OnWebhookRoutingOptionsChange(WebhookRoutingOptions options, string data)
        {
            logger.LogInformation("Configuration updated!");
        }

        public async Task RouteAsync(Guid webhookId, JsonDocument body, string signature, CancellationToken cancellationToken = default)
        {
            var routeAsyncScopeProperties = new Dictionary<string, object>()
            {
                { "WebhookId", webhookId },
                { "Body", body },
                { "Signature", signature }
            };

            using (logger.BeginScope(routeAsyncScopeProperties))
            {
                logger.LogInformation("Routing webhook {webhookId}", webhookId);
                var payload = body.RootElement.ToString();

                var isSignatureValid = SignatureValidator.IsValidSignature(payload, options.CurrentValue.WebhookSecret, signature);                
                if (isSignatureValid)
                {
                    logger.LogInformation("Webhook signature was valid");
                }
                else
                {
                    logger.LogError("Webhook signature was invalid");
                    throw new WebhookRouterException("Invalid signature.");
                }
            }    
        }
    }
}
