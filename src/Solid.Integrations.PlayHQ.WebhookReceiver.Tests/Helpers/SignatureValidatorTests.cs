using Xunit;
using Solid.Integrations.PlayHQ.WebhookReceiver.Helpers;
using System.Runtime.CompilerServices;

namespace Solid.Integrations.PlayHQ.WebhookReceiver.Tests.Helpers;

public class SignatureValidatorTests
{
    [Fact]
    public void ValidSignaturePasses()
    {
        var payload = File.ReadAllText("Helpers\\TestData\\score-event.json");
        var secret = "open sesame";
        var signature = "b76c0da9a85299fb3dc35dfdabeb95114caf5c297be0fe0b2f5d7e10f8dee1fa";

        var result = SignatureValidator.IsValidSignature(payload, secret, signature);
        Assert.True(result, "Signature was invalid when it was expected to be valid.");
    }

    [Fact]
    public void InvalidSignatureFails()
    {
        var payload = File.ReadAllText("Helpers\\TestData\\formatted-score-event.json");
        var secret = "open sesame";
        var signature = "b76c0da9a85299fb3dc35dfdabeb95114caf5c297be0fe0b2f5d7e10f8dee1fa";

        var result = SignatureValidator.IsValidSignature(payload, secret, signature);
        Assert.False(result, "Signature was valid when it was expected to be invalid.");
    }

    [Fact]
    public void NullPayloadThrowsArgumentNullException()
    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        string payload = null; // Ooops!
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        var secret = "open sesame";
        var signature = "b76c0da9a85299fb3dc35dfdabeb95114caf5c297be0fe0b2f5d7e10f8dee1fa";

#pragma warning disable CS8604 // Possible null reference argument.
        Assert.Throws<ArgumentNullException>(
            "payload",
            () => SignatureValidator.IsValidSignature(payload, secret, signature));
#pragma warning restore CS8604 // Possible null reference argument.
    }

    [Fact]
    public void NullSecretThrowsArgumentNullException()
    {
        var payload = File.ReadAllText("Helpers\\TestData\\formatted-score-event.json");
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        string secret = null; // Ooops!
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        var signature = "b76c0da9a85299fb3dc35dfdabeb95114caf5c297be0fe0b2f5d7e10f8dee1fa";

#pragma warning disable CS8604 // Possible null reference argument.
        Assert.Throws<ArgumentNullException>(
            "secret",
            () => {
                SignatureValidator.IsValidSignature(payload, secret, signature);
            });
#pragma warning restore CS8604 // Possible null reference argument.
    }

    [Fact]
    public void NullSignatureThrowsArgumentNullException()
    {
        var payload = File.ReadAllText("Helpers\\TestData\\formatted-score-event.json");
        var secret = "open sesame"; // Ooops!
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        string signature = null; // Ooops!
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

#pragma warning disable CS8604 // Possible null reference argument.
        Assert.Throws<ArgumentNullException>(
            "signature",
            () => {
                SignatureValidator.IsValidSignature(payload, secret, signature);
            });
#pragma warning restore CS8604 // Possible null reference argument.
    }
}