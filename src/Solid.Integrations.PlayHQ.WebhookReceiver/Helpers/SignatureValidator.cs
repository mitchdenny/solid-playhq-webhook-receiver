using System;
using System.Security.Cryptography;
using System.Text;

namespace Solid.Integrations.PlayHQ.WebhookReceiver.Helpers
{
    public static class SignatureValidator
    {
        public static bool IsValidSignature(string payload, string secret, string signature)
        {
            if (string.IsNullOrEmpty(payload)) throw new ArgumentNullException(nameof(payload));
            if (string.IsNullOrEmpty(secret)) throw new ArgumentNullException(nameof(secret));
            if (string.IsNullOrEmpty(signature)) throw new ArgumentNullException(nameof(signature));

            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            var secretBytes = Encoding.UTF8.GetBytes(secret);

            var hmac = new HMACSHA256(secretBytes);
            var computedSignatureBytes = hmac.ComputeHash(payloadBytes);
            var computedSignature = Convert.ToHexString(computedSignatureBytes).ToLower();

            return computedSignature == signature;
        }
    }
}