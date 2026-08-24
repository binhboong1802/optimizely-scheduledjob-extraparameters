using EPiServer.Scheduler.Internal;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Modules;
using Microsoft.Extensions.DependencyInjection;
using OptiScheduledJob.ExtraParameters.Infrastructure.Scheduling;

namespace OptiScheduledJob.ExtraParameters.Infrastructure.Configuration
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the add-on. Must be called after AddCms(): the scheduled job locator below has
        /// to be registered after EPiServer's own, since the container resolves the last registration.
        /// </summary>
        public static IServiceCollection AddScheduledJobExtraParameters(this IServiceCollection services)
        {
            services.Configure<ProtectedModuleOptions>(module =>
            {
                if (!module.Items.Any(i => i.Name.Equals(Constants.ModuleName, StringComparison.OrdinalIgnoreCase)))
                {
                    module.Items.Add(new ModuleDetails { Name = Constants.ModuleName });
                }
            });

            services.AddSingleton<IScheduledJobLocator, ExtraParametersAwareScheduledJobLocator>();

            return services;
        }
    }

}
