using EPiServer.PlugIn;

namespace OptiScheduledJob.ExtraParameters.Attributes
{    
    [AttributeUsage(AttributeTargets.Class)]
    public class ScheduledPlugInWithExtraParametersAttribute : EPiServer.Scheduler.ScheduledJobAttribute
    {
        public Type ExtraParameterDefinition { get; set; }
    }
}
