using System;
using System.Security.Cryptography;
using System.Text;

namespace Solid.Integrations.PlayHQ.Common;

public static class SignatureHelper
{
    public static readonly string BypassSignature = Guid.NewGuid().ToString();

    public static bool IsValidSignature(string payload, string secret, string signature)
    {
        if (signature == BypassSignature) return true;

        if (string.IsNullOrEmpty(payload)) throw new ArgumentNullException(nameof(payload));
        if (string.IsNullOrEmpty(secret)) throw new ArgumentNullException(nameof(secret));
        if (string.IsNullOrEmpty(signature)) throw new ArgumentNullException(nameof(signature));

        var computedSignature = GenerateSignature(payload, secret);

        return computedSignature == signature;
    }

    public static string GenerateSignature(string payload, string secret)
    {
        if (string.IsNullOrEmpty(payload)) throw new ArgumentNullException(nameof(payload));
        if (string.IsNullOrEmpty(secret)) throw new ArgumentNullException(nameof(secret));

        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var secretBytes = Encoding.UTF8.GetBytes(secret);

        var hmac = new HMACSHA256(secretBytes);
        var computedSignatureBytes = hmac.ComputeHash(payloadBytes);
        var computedSignature = Convert.ToHexString(computedSignatureBytes).ToLower();

        return computedSignature;
    }
}