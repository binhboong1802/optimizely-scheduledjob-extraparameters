namespace Optimizely.ScheduledJob.ExtraSettings.Services
{
    public interface IScheduledJobExtraParametersService
    {
        T Get<T>(Guid scheduledJobInstanceId) where T : class;
        object Get(Guid scheduledJobInstanceId, Type type);
        void Save<T>(Guid scheduledJobInstanceId, T value) where T : class;

    }
}
