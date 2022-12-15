using Azure.Messaging.EventHubs.Consumer;
using Solid.Integrations.PlayHQ.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Solid.Integrations.PlayHQ.WebhookClient
{
    internal class EventPump
    {
        public Uri endpoint;
        private Guid tenantId;
        private Guid playingSurfaceId;
        private string secret;
        private FileInfo outputPath;

        public EventPump(Uri endpoint, Guid tenantId, Guid playingSurfaceId, string secret, FileInfo outputPath)
        {
            this.endpoint= endpoint;
            this.tenantId = tenantId;
            this.playingSurfaceId = playingSurfaceId;
            this.secret = secret;
            this.outputPath = outputPath;
        }

        public static async Task<EventPump> FromConfigurationAsync(string path)
        {
            using var stream = new FileStream(path, FileMode.Open);
            var configuration = await JsonSerializer.DeserializeAsync<Configuration>(stream);

            var eventPump = new EventPump(
                configuration!.Endpoint,
                configuration!.TenantId,
                configuration!.PlayingSurfaceId,
                configuration!.Secret,
                new FileInfo(configuration.OutputPath));

            return eventPump;
        }

        private async Task<string> GetConnectionStringAsync(Uri endpoint, Guid tenantId, Guid playingSurfaceId, string secret, CancellationToken cancellationToken)
        {
            var nonce = Guid.NewGuid();
            var expiry = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds(); // allow for 5 minutes drift
            var signedContent = $"{tenantId} {playingSurfaceId} {nonce} {expiry}";
            var signature = SignatureHelper.GenerateSignature(signedContent, secret);

            using var client = new HttpClient();

            var requestUrl = $"{endpoint}tenants/{tenantId}/playing-surfaces/{playingSurfaceId}/config?nonce={nonce}&expiry={expiry}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Signature", signature);
            var response = await client.SendAsync(request);

            var playingSurfaceConfiguration = await response.Content.ReadFromJsonAsync<PlayingSurfaceConfiguration>(new JsonSerializerOptions(), cancellationToken);

            return playingSurfaceConfiguration!.ConnectionString;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var connectionString = await GetConnectionStringAsync(
                    endpoint,
                    tenantId,
                    playingSurfaceId,
                    secret,
                    cancellationToken
                    );

                var client = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, connectionString);
                var events = client.ReadEventsAsync(false, new ReadEventOptions(), cancellationToken);

                await foreach (var @event in events)
                {
                    var payload = @event.Data.EventBody.ToString();
                    Console.WriteLine(payload);
                    await File.WriteAllTextAsync(outputPath.FullName, payload, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                // Swallow!
            }
        }
    }
}
