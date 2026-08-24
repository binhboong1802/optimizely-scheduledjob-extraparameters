using EPiServer.PlugIn;
using EPiServer.Scheduler.Internal;
using OptiScheduledJob.ExtraParameters.Attributes;

namespace OptiScheduledJob.ExtraParameters.Infrastructure.Scheduling
{
    /// <summary>
    /// Replaces EPiServer's built-in scheduled job locator (PluginScheduledJobLocator).
    ///
    /// The built-in locator asks <see cref="IPlugInDescriptorRepository"/> for descriptors keyed by
    /// the exact <see cref="ScheduledPlugInAttribute"/> type, and the repository stores descriptors
    /// keyed by each attribute instance's concrete type. A job decorated with the subclassed
    /// <see cref="ScheduledPlugInWithExtraParametersAttribute"/> is therefore stored under a key the
    /// scheduler never queries, so the job is never registered in tblScheduledItem on a cold start
    /// and never shows up under Admin → Scheduled Jobs (GitHub issue #1).
    ///
    /// This locator additionally queries the subclass attribute's key. Note that a further subclass
    /// of <see cref="ScheduledPlugInWithExtraParametersAttribute"/> would be keyed under its own type
    /// and remain invisible again — the descriptor repository offers no inheritance-aware lookup, and
    /// enumerating all descriptors is unsafe (<see cref="PlugInDescriptor.PlugInType"/> throws for
    /// stale rows whose type no longer resolves).
    ///
    /// <see cref="IScheduledJobLocator"/> lives in an internal EPiServer namespace, but the package
    /// pins EPiServer.CMS to [12.x,13.0) and the contract is stable across the 12.x line. CMS 13
    /// discovers jobs inheritance-aware, so the 2.x package line does not need this.
    /// </summary>
    internal class ExtraParametersAwareScheduledJobLocator : IScheduledJobLocator
    {
        private readonly IPlugInDescriptorRepository _plugInDescriptorRepository;

        public ExtraParametersAwareScheduledJobLocator(IPlugInDescriptorRepository plugInDescriptorRepository)
        {
            _plugInDescriptorRepository = plugInDescriptorRepository;
        }

        public IEnumerable<PlugInDescriptor> ListScheduledJobTypes()
        {
            return _plugInDescriptorRepository.List(typeof(ScheduledPlugInAttribute))
                .Concat(_plugInDescriptorRepository.List(typeof(ScheduledPlugInWithExtraParametersAttribute)))
                .Where(p => p.Enabled)
                // A type carrying both attributes would show up once per key; the scanner would then
                // register it twice, so collapse to one descriptor per job type.
                .GroupBy(p => p.PlugInType)
                .Select(g => g.First());
        }
    }
}
