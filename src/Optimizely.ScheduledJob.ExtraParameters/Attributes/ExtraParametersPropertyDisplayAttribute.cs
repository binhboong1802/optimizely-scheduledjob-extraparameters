using Microsoft.AspNetCore.Mvc.Rendering;

namespace Optimizely.ScheduledJob.ExtraParameters.Attributes
{
    [AttributeUsage(
       AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Class,
       AllowMultiple = false)]
    public class ExtraParametersPropertyDisplayAttribute: Attribute
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public IList<SelectListItem> Options { get; set; }

    }
}
