using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Solid.Integrations.PlayHQ.WebhookReceiver.Helpers;
using System.Data;
using System.Security.Cryptography.Xml;
using System.Text.Json;

namespace Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting
{
    public class WebhookRouter : IWebhookRouter
    {
        private ILogger<WebhookRouter> logger;
        private IOptionsMonitor<WebhookRoutingOptions> options;
        private IMemoryCache memoryCache;

        public WebhookRouter(ILogger<WebhookRouter> logger, IOptionsMonitor<WebhookRoutingOptions> options, IMemoryCache memoryCache)
        {
            this.logger = logger;
            this.options = options;
            this.memoryCache = memoryCache;
            this.options.OnChange(OnWebhookRoutingOptionsChange);
        }

        private void OnWebhookRoutingOptionsChange(WebhookRoutingOptions options, string data)
        {
            logger.LogInformation("Configuration updated!");
        }

        private RoutingRule GetRoutingRule(Guid webhookId)
        {
            if (options.CurrentValue.Rules == null || !options.CurrentValue.Rules.ContainsKey(webhookId.ToString()))
            {
                logger.LogWarning("Routing rule not found for {webhookId}", webhookId);
                throw new WebhookRouterException("No routing rule.");
            }

            var rule = options.CurrentValue.Rules[webhookId.ToString()];
            logger.LogInformation("Found routing rule for {webhookId}", webhookId);
            return rule;
        }

        private bool TryGetPlayingSurfaceMapping(RoutingRule rule, JsonDocument body, out PlayingSurfaceMapping? mapping)
        {
            // TODO: Need to add code here to translate from game ID to playing surface
            //       to figure out the right way to route. For the time being we are just
            //       going to ignore this and return the Guid.Empty() matching rule.
            Guid playingSurfaceId = Guid.Empty;

            if (rule.Mappings == null || !rule.Mappings.ContainsKey(playingSurfaceId.ToString()))
            {
                logger.LogInformation(
                    "Could not find playing surface mapping {playingSurfaceId}.", playingSurfaceId);
                mapping = null;
                return false;
            }

            mapping = rule.Mappings[playingSurfaceId.ToString()];
            logger.LogInformation(
                "Found playing surface mapping {playingSurfaceId} with namespace {eventHubsNamespace} and name {eventHubName}.",
                playingSurfaceId,
                mapping.EventHubsNamespace,
                mapping.EventHubName
                );
            return true;
        }

        private EventHubProducerClient GetEventHubProducerClient(string eventHubsNamespace, string eventHubName)
        {
            var cacheKey = $"EventHubProducerClientCacheKey_{eventHubsNamespace}_{eventHubName}";

            logger.LogInformation("Getting EventHubProducerClient for cache key {cacheKey}", cacheKey);
            var client = memoryCache.GetOrCreate(cacheKey, (entry) =>
            {
                var expires = TimeSpan.FromSeconds(options.CurrentValue.EventHubClientCacheSlidingExpirationSeconds);
                entry.SetSlidingExpiration(expires);

                logger.LogInformation(
                    "Creating new EventHubProducerClient with cache key {cacheKey} and sliding expiration timespan {expires}",
                    cacheKey,
                    expires
                    );
                return new EventHubProducerClient(eventHubsNamespace, eventHubName, new DefaultAzureCredential()); // TODO: Should we reuse the credential?
            });

            return client;
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
                
                var rule = GetRoutingRule(webhookId);
                var payload = body.RootElement.ToString();

                var isSignatureValid = SignatureValidator.IsValidSignature(
                    payload,
                    rule.WebhookSecret ?? throw new WebhookRouterException("Webhook secret was null"),
                    signature
                    );                
                if (isSignatureValid)
                {
                    logger.LogInformation("Webhook signature was valid");
                }
                else
                {
                    logger.LogError("Webhook signature was invalid");
                    throw new WebhookRouterException("Invalid signature.");
                }

                if (TryGetPlayingSurfaceMapping(rule, body, out var mapping))
                {
                    if (mapping == null) throw new WebhookRouterException("Mapping was unexpectedly null.");
                    var client = GetEventHubProducerClient(
                        mapping.EventHubsNamespace ?? throw new WebhookRouterException("EventHubsNamespace was null"),
                        mapping.EventHubName ?? throw new WebhookRouterException("EventHubName was null")
                        ) ;

                    var messageId = body.RootElement.GetProperty("messageId").GetString();
                    logger.LogInformation("Forwarding event for webhook message {messageId}", messageId);
                    var eventData = new EventData(payload);
                    await client.SendAsync(new[] { eventData });
                }
                else
                {
                    logger.LogInformation("No mapping for incoming event was found so silently dropping webhook notifcation.");
                }
            }    
        }
    }
}
