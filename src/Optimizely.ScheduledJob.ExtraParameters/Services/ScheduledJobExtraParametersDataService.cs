using EPiServer.Data.Dynamic;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using Optimizely.ScheduledJob.ExtraParameters.Models;

namespace Optimizely.ScheduledJob.ExtraParameters.Services
{
    [ServiceConfiguration(typeof(IScheduledJobExtraParametersDataService))]
    public class ScheduledJobExtraParametersDataService : IScheduledJobExtraParametersDataService
    {
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
            if (string.IsNullOrWhiteSpace(dynamicData?.Value))
            {
                return JsonConvert.DeserializeObject<T>(dynamicData.Value);
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
                return JsonConvert.DeserializeObject(dynamicData.Value, type);
            }

            return null;
        }

        public virtual void Save<T>(Guid scheduledJobInstanceId, T value) where T : class
        {
            var store = _factory.GetStore(typeof(ScheduledJobExtraParametersData))
               ?? _factory.CreateStore(typeof(ScheduledJobExtraParametersData));

            var dynamicData = store.Find<ScheduledJobExtraParametersData>(nameof(ScheduledJobExtraParametersData.ScheduledJobId), scheduledJobInstanceId).FirstOrDefault();

            if (dynamicData != null)
            {
                dynamicData.Value = JsonConvert.SerializeObject(value);
                store.Save(dynamicData);
            }
            else
            {
                store.Save(new ScheduledJobExtraParametersData() { ScheduledJobId = scheduledJobInstanceId, Value = JsonConvert.SerializeObject(value) });
            }
        }
    }
}
