using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Solid.Integrations.PlayHQ.Common;
using System.Data;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.Xml;
using System.Text.Json;

namespace Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting
{
    public class WebhookRouter : IWebhookRouter
    {
        private ILogger<WebhookRouter> logger;
        private IOptionsMonitor<WebhookRoutingOptions> options;
        private IMemoryCache memoryCache;
        private HashSet<string> noncesWithExpiry = new HashSet<string>();

        public WebhookRouter(ILogger<WebhookRouter> logger, IOptionsMonitor<WebhookRoutingOptions> options, IMemoryCache memoryCache)
        {
            this.logger = logger;
            this.options = options;
            this.memoryCache = memoryCache;
            this.options.OnChange(OnWebhookRoutingOptionsChange);
        }

        private void OnWebhookRoutingOptionsChange(WebhookRoutingOptions options, string? data)
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

        private bool TryGetPlayingSurfaceMapping(RoutingRule rule, string playingSurfaceId, out PlayingSurfaceMapping? mapping)
        {
            if (rule.Mappings != null && rule.Mappings.ContainsKey(playingSurfaceId))
            {
                mapping = rule.Mappings[playingSurfaceId];
                return true;
            }
            else
            {
                mapping = null;
                return false;
            }
        }

        private bool TryGetPlayingSurfaceMapping(RoutingRule rule, JsonDocument body, out PlayingSurfaceMapping? mapping)
        {
            mapping = null;

            if (!body.RootElement.TryGetProperty("messageId", out var messageId))
            {
                logger.LogWarning("Could not find messageId for webhook event payload.");
                return false;
            }

            if (!body.RootElement.TryGetProperty("filters", out var filtersProperty))
            {
                logger.LogWarning("Filter property missing from messageId: {messageId} ", messageId);
                return false;
            }

            if (filtersProperty.ValueKind != JsonValueKind.Array)
            {
                logger.LogWarning("Filter property not an array on messageId: {messageId}", messageId);
                return false;
            }

            if (filtersProperty.GetArrayLength() == 0)
            {
                logger.LogWarning("Filter property array has no items on messageId: {messageId}", messageId);
                return false;
            }

            foreach (var filter in filtersProperty.EnumerateArray())
            {
                if (!filter.TryGetProperty("entityType", out var entityTypeProperty))
                {
                    logger.LogWarning("Filter does not contain entityType property on messageId: {messageId}", messageId);
                    continue;
                }

                if (entityTypeProperty.GetString() != "PLAYING_SURFACE")
                {
                    logger.LogInformation("Filter entityType property is not PLAYING_SURFACE.");
                    continue;
                }

                if (!filter.TryGetProperty("entityId", out var entityId))
                {
                    logger.LogWarning("Filter does not contain an entityId on messageId: {messageId}", messageId);
                    continue;
                }

                if (string.IsNullOrEmpty(entityId.GetString()))
                {
                    logger.LogWarning("Filter entityId property is null or empty on messageId: {messageId}", messageId);
                    continue;
                }

                var playingSurfaceId = entityId.GetString()!;

                if (!TryGetPlayingSurfaceMapping(rule, playingSurfaceId, out mapping))
                {
                    logger.LogInformation(
                        "Could not find playing surface mapping {playingSurfaceId}.", playingSurfaceId);
                    mapping = null;
                    continue;
                }

                logger.LogInformation(
                    "Found playing surface mapping {playingSurfaceId} with namespace {eventHubsNamespace} and name {eventHubName}.",
                    playingSurfaceId,
                    mapping.EventHubsNamespace,
                    mapping.EventHubName
                    );
                return true;
            }

            return false;
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

            return client!;
        }

        private bool IsValidNonce(string nonce)
        {
            if (noncesWithExpiry.Contains(nonce))
            {
                return false;
            }
            else
            {
                noncesWithExpiry.Add(nonce);
                return true;
            }
        }

        public bool IsValidExpiry(int expiry)
        {
            var clockDriftAllowanceTimeSpan = TimeSpan.FromSeconds(options.CurrentValue.ClockDriftAllowanceInSeconds);
            var expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(expiry);
            var validAfter = DateTimeOffset.UtcNow.Subtract(clockDriftAllowanceTimeSpan);
            var validBefore = DateTimeOffset.UtcNow.Add(clockDriftAllowanceTimeSpan);

            return expiryDateTime > validAfter && expiryDateTime < validBefore;
        }

        public async Task<PlayingSurfaceConfiguration> GetPlayingSurfaceConfigurationAsync(Guid tenantId, Guid playingSurfaceId, string nonce, int expiry, string signature, CancellationToken cancellationToken)
        {
            var routeAsyncScopeProperties = new Dictionary<string, object>()
            {
                { "TenantId", tenantId },
                { "PlayingSurfaceId", playingSurfaceId },
                { "Signature", signature }
            };

            using (logger.BeginScope(routeAsyncScopeProperties))
            {
                if (!IsValidNonce(nonce)) throw new WebhookRouterException("Invalid nonce.");
                if (!IsValidExpiry(expiry)) throw new WebhookRouterException("Invalid expiry.");

                var rule = GetRoutingRule(tenantId);

                if (!TryGetPlayingSurfaceMapping(rule, playingSurfaceId.ToString(), out var mapping))
                {
                    throw new WebhookRouterException("No playing surface mapping.");
                }

                var signedContent = $"{tenantId} {playingSurfaceId} {nonce} {expiry}";
                if (!SignatureHelper.IsValidSignature(signedContent, mapping.SharedSecret, signature))
                {
                    throw new WebhookRouterException("Invalid signature.");
                }

                var playingSurfaceConfiguration = new PlayingSurfaceConfiguration()
                {
                    ConnectionString = mapping.ConnectionString
                };

                return playingSurfaceConfiguration;
            }
        }

        public async Task RouteAsync(Guid webhookId, JsonDocument body, string signature, CancellationToken cancellationToken = default)
        {
            var routeAsyncScopeProperties = new Dictionary<string, object>()
            {
                { "WebhookId", webhookId },
                { "Body", body.RootElement.ToString() },
                { "Signature", signature }
            };

            using (logger.BeginScope(routeAsyncScopeProperties))
            {
                logger.LogInformation("Routing webhook {webhookId}", webhookId);
                
                var rule = GetRoutingRule(webhookId);
                var payload = body.RootElement.ToString();

                var isSignatureValid = SignatureHelper.IsValidSignature(
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
