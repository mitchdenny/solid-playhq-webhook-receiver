namespace Solid.Integrations.PlayHQ.WebhookReceiver.Services.WebhookRouting
{
    public static class WebhookRouterExtensions
    {
        public static IServiceCollection AddWebhookRouter(this IServiceCollection services, IConfiguration configuration)
        {
            var webhookRoutingOptionsConfigurationSection = configuration.GetSection(WebhookRoutingOptions.WebhookRoutingOptionsSectionKey);
            services.Configure<WebhookRoutingOptions>(webhookRoutingOptionsConfigurationSection);
            services.AddSingleton<IWebhookRouter, WebhookRouter>();

            return services;
        }
    }
}
