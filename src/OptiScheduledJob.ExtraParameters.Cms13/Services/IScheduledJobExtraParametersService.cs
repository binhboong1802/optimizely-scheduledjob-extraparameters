using Microsoft.AspNetCore.Http;
using OptiScheduledJob.ExtraParameters.Attributes;
using OptiScheduledJob.ExtraParameters.ViewModels;

namespace OptiScheduledJob.ExtraParameters.Services
{
    public interface IScheduledJobExtraParametersService
    {
        ExtraParametersViewModel GetExtraParametersViewModel(Guid scheduledJobId);
        void SaveExtraParameters(Guid scheduledJobId, IFormCollection form);
        ScheduledPlugInWithExtraParametersAttribute GetScheduledJobExtraParametersAttribute(Guid scheduledJobId);
    }
}
