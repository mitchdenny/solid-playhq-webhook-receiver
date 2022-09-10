namespace Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting
{

	[Serializable]
	public class WebhookRouterException : Exception
	{
		public WebhookRouterException() { }
		public WebhookRouterException(string message) : base(message) { }
		public WebhookRouterException(string message, Exception inner) : base(message, inner) { }
		protected WebhookRouterException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
