using EPiServer.PlugIn;

namespace OptiScheduledJob.ExtraParameters.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ScheduledPlugInWithExtraParametersAttribute : ScheduledPlugInAttribute
    {
        public Type ExtraParameterDefinition { get; set; }
    }
}
