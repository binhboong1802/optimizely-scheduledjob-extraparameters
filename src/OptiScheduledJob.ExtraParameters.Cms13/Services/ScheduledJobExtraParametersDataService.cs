using EPiServer.Data.Dynamic;
using EPiServer.ServiceLocation;
using OptiScheduledJob.ExtraParameters.Models;
using System.Text.Json;

namespace OptiScheduledJob.ExtraParameters.Services
{
    [ServiceConfiguration(typeof(IScheduledJobExtraParametersDataService))]
    public class ScheduledJobExtraParametersDataService : IScheduledJobExtraParametersDataService
    {
        // CMS 13 moved the whole framework off Newtonsoft.Json, so it is no longer available as a
        // transitive dependency and this line uses System.Text.Json instead.
        //
        // Rows written by the 1.x/CMS 12 line were serialized by Newtonsoft. Newtonsoft wrote CLR
        // property names verbatim, which is also STJ's default when no naming policy is set, so the
        // written shape is identical. Reading stays case-insensitive as a safety net for blobs whose
        // casing was influenced by a custom Newtonsoft setting on the consuming site.
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = true
        };

        private readonly DynamicDataStoreFactory _factory;

        public ScheduledJobExtraParametersDataService(DynamicDataStoreFactory factory)
        {
            _factory = factory;
        }

        public virtual T Get<T>(Guid scheduledJobInstanceId) where T : class
        {
            var store = _factory.GetStore(typeof(ScheduledJobExtraParametersData))
                ?? _factory.CreateStore(typeof(ScheduledJobExtraParametersData));

            var dynamicData = store.Find<ScheduledJobExtraParametersData>(nameof(ScheduledJobExtraParametersData.ScheduledJobId), scheduledJobInstanceId).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(dynamicData?.Value))
            {
                return JsonSerializer.Deserialize<T>(dynamicData.Value, SerializerOptions);
            }

            return default;
        }

        public virtual object Get(Guid scheduledJobInstanceId,  Type type)
        {
            var store = _factory.GetStore(typeof(ScheduledJobExtraParametersData))
                ?? _factory.CreateStore(typeof(ScheduledJobExtraParametersData));

            var dynamicData = store.Find<ScheduledJobExtraParametersData>(nameof(ScheduledJobExtraParametersData.ScheduledJobId), scheduledJobInstanceId).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(dynamicData?.Value))
            {
                return JsonSerializer.Deserialize(dynamicData.Value, type, SerializerOptions);
            }

            return null;
        }

        public virtual void Save<T>(Guid scheduledJobInstanceId, T value) where T : class
        {
            var store = _factory.GetStore(typeof(ScheduledJobExtraParametersData))
               ?? _factory.CreateStore(typeof(ScheduledJobExtraParametersData));

            var dynamicData = store.Find<ScheduledJobExtraParametersData>(nameof(ScheduledJobExtraParametersData.ScheduledJobId), scheduledJobInstanceId).FirstOrDefault();

            // Serialize against the runtime type: `value` is declared as T but the caller passes the
            // consumer's parameter class boxed as ScheduledJobExtraParametersBase, and STJ would
            // otherwise serialize only the (empty) base type's properties.
            var json = JsonSerializer.Serialize(value, value.GetType(), SerializerOptions);

            if (dynamicData != null)
            {
                dynamicData.Value = json;
                store.Save(dynamicData);
            }
            else
            {
                store.Save(new ScheduledJobExtraParametersData() { ScheduledJobId = scheduledJobInstanceId, Value = json });
            }
        }
    }
}
