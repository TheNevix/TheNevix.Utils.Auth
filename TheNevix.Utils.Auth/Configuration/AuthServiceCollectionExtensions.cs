using Microsoft.Extensions.DependencyInjection;

namespace TheNevix.Utils.Auth.Configuration
{
    public static class AuthServiceCollectionExtensions
    {
        public static IServiceCollection AddAuth(this IServiceCollection services, Action<AuthOptions> configureOptions)
        {
            services.Configure(configureOptions);

            return services;
        }
    }

}
