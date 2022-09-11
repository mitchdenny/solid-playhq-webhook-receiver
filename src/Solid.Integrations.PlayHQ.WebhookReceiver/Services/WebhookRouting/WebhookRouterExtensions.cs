namespace Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting
{
    public static class WebhookRouterExtensions
    {
        public static IServiceCollection AddWebhookRouter(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IWebhookRouter, WebhookRouter>();

            return services;
        }
    }
}
