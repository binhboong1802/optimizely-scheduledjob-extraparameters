using System.Collections.Specialized;

namespace Optimizely.ScheduledJob.ExtraSettings.ViewModels
{
    public class ExtraParametersViewModel
    {
        public IList<ExtraParameterSetting> ExtraParameterSettings { get; set; }
    }

    public class ExtraParameterSetting
    {
        public string Name { get; set; }

        public object? Value { get; set; }

        public NameValueCollection Options { get; set; }
    }
}
