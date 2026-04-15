using EPiServer.Data.Dynamic;

namespace Optimizely.ScheduledJob.ExtraSettings
{
    [EPiServerDataStore(AutomaticallyRemapStore = true)]
    public class ScheduledJobExtraParametersData
    {
        [EPiServerDataIndex]
        public Guid ScheduledJobId { get; set; }
        public string Value { get; set; }
    }
}
