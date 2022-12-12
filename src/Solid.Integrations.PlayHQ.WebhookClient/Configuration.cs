using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Solid.Integrations.PlayHQ.WebhookClient
{
    internal class Configuration
    {
        [JsonPropertyName("endpoint")]
        public required Uri Endpoint { get; set; }
        
        [JsonPropertyName("tenantId")]
        public required Guid TenantId { get; set; }
        
        [JsonPropertyName("playingSurfaceId")]
        public required Guid PlayingSurfaceId { get; set; }
        
        [JsonPropertyName("secret")]
        public required string Secret { get; set; }

        [JsonPropertyName("outputPath")]
        public required string OutputPath { get; set; }
    }
}
