using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Integrations.PlayHQ.WebhookClient
{
    public class EventEntry
    {
        public EventEntry(DateTimeOffset received, string payload)
        {
            Received = received;
            Payload = payload;
        }

        public DateTimeOffset Received { get; private set; }
        public string Payload { get; private set; }
    }
}
