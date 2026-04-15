using EPiServer.ServiceLocation;
using EPiServer.Shell.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace Optimizely.ScheduledJob.ExtraParameters.Infrastructure.Configuration
{    
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScheduledJobExtraParameters(this IServiceCollection services)
        {
            services.Configure<ProtectedModuleOptions>(module =>
            {
                if (!module.Items.Any(i => i.Name.Equals(Constants.ModuleName, StringComparison.OrdinalIgnoreCase)))
                {
                    module.Items.Add(new ModuleDetails { Name = Constants.ModuleName });
                }
            });

            return services;
        }       
    }

}
