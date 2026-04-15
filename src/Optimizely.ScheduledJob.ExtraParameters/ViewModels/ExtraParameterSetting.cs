using Microsoft.AspNetCore.Mvc.Rendering;

namespace Optimizely.ScheduledJob.ExtraParameters.ViewModels
{
    public class ExtraParameterSetting
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public object? Value { get; set; }

        public IList<SelectListItem> Options { get; set; }
    }
}
