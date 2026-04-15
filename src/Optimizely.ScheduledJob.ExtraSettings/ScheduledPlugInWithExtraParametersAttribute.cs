using EPiServer.PlugIn;

namespace Optimizely.ScheduledJob.ExtraSettings
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ScheduledPlugInWithExtraParametersAttribute : ScheduledPlugInAttribute
    {
        public Type ExtraParameterDefinition { get; set; }
    }
}
