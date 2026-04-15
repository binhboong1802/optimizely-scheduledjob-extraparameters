using EPiServer.Framework.Initialization;
using EPiServer.Framework;
using EPiServer.Initialization.Internal;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Modules;
using EPiServer.Web;

namespace Optimizely.ScheduledJob.ExtraSettings.Initialization
{
    [ModuleDependency(typeof(InitializationModule))]
    [ModuleDependency(typeof(CmsRuntimeInitialization))]
    internal class ScheduledJobExtraParametersInitializationModule : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            var services = context.Services;

            services.Configure<ProtectedModuleOptions>(module =>
            {
                if (!module.Items.Any(i => i.Name.Equals(Constants.ModuleName, StringComparison.OrdinalIgnoreCase)))
                {
                    module.Items.Add(new ModuleDetails { Name = Constants.ModuleName });
                }
            });
        }

        public void Initialize(InitializationEngine context)
        {
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
