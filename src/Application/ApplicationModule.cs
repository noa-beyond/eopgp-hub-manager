using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class Application
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            // Register FluentValidation
            //services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
