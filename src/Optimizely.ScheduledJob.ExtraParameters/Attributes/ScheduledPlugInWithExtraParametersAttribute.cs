using EPiServer.PlugIn;

namespace Optimizely.ScheduledJob.ExtraParameters.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ScheduledPlugInWithExtraParametersAttribute : ScheduledPlugInAttribute
    {
        public Type ExtraParameterDefinition { get; set; }
    }
}
