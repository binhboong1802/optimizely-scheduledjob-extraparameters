using Microsoft.AspNetCore.Mvc.Rendering;

namespace OptiScheduledJob.ExtraParameters.ViewModels
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
