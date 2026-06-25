namespace OptiScheduledJob.ExtraParameters.Services
{
    public interface IScheduledJobExtraParametersDataService
    {
        T Get<T>(Guid scheduledJobInstanceId) where T : class;
        object Get(Guid scheduledJobInstanceId, Type type);
        void Save<T>(Guid scheduledJobInstanceId, T value) where T : class;
    }
}
