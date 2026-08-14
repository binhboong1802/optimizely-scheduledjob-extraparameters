using EPiServer.Data.Dynamic;

namespace OptiScheduledJob.ExtraParameters.Models
{
    // The Dynamic Data Store survives in CMS 13 (EPiServer.Data.Dynamic is intact), and the store name
    // is derived from this type, which is unchanged - so parameter values saved by the 1.x/CMS 12 line
    // are read back as-is after an upgrade.
    [EPiServerDataStore(AutomaticallyRemapStore = true)]
    public class ScheduledJobExtraParametersData
    {
        [EPiServerDataIndex]
        public Guid ScheduledJobId { get; set; }
        public string Value { get; set; }
    }
}
