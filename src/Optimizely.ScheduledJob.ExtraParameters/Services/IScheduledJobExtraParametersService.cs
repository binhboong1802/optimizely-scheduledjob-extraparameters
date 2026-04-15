using Microsoft.AspNetCore.Http;
using Optimizely.ScheduledJob.ExtraParameters.Attributes;
using Optimizely.ScheduledJob.ExtraParameters.ViewModels;

namespace Optimizely.ScheduledJob.ExtraParameters.Services
{
    public interface IScheduledJobExtraParametersService
    {
        ExtraParametersViewModel GetExtraParametersViewModel(Guid scheduledJobId);
        void SaveExtraParameters(Guid scheduledJobId, IFormCollection form);
        ScheduledPlugInWithExtraParametersAttribute GetScheduledJobExtraParametersAttribute(Guid scheduledJobId);
    }
}
