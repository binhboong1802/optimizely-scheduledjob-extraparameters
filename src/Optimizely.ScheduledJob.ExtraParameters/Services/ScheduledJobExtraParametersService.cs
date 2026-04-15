using Castle.Core.Internal;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Http;
using Optimizely.ScheduledJob.ExtraParameters.Attributes;
using Optimizely.ScheduledJob.ExtraParameters.Models;
using Optimizely.ScheduledJob.ExtraParameters.ViewModels;
using System.ComponentModel;

namespace Optimizely.ScheduledJob.ExtraParameters.Services
{

    [ServiceConfiguration(typeof(IScheduledJobExtraParametersService))]
    public class ScheduledJobExtraParametersService : IScheduledJobExtraParametersService
    {
        private readonly IScheduledJobRepository _scheduledJobRepository;
        private readonly IScheduledJobExtraParametersDataService _extraParametersDataService;

        public ScheduledJobExtraParametersService(IScheduledJobRepository scheduledJobRepository, IScheduledJobExtraParametersDataService extraParametersDataService)
        {
            _scheduledJobRepository = scheduledJobRepository;
            _extraParametersDataService = extraParametersDataService;
        }
        public ExtraParametersViewModel GetExtraParametersViewModel(Guid scheduledJobId)
        {
            ScheduledPlugInWithExtraParametersAttribute? attribute = GetScheduledJobExtraParametersAttribute(scheduledJobId);

            if (attribute != null)
            {
                var extraParameterDefType = attribute.ExtraParameterDefinition;

                var extraValue = _extraParametersDataService.Get(scheduledJobId, extraParameterDefType) ?? extraParameterDefType.CreateInstance<ScheduledJobExtraParametersBase>();

                var model = new ExtraParametersViewModel() { ExtraParameterSettings = new List<ExtraParameterSetting>() };

                foreach (var prop in extraValue.GetType().GetProperties())
                {
                    var displayAttribute = prop.GetAttribute<ExtraParametersPropertyDisplayAttribute>();

                    model.ExtraParameterSettings.Add(new ExtraParameterSetting()
                    {
                        Name = prop.Name,
                        DisplayName = displayAttribute?.DisplayName ?? prop.Name,
                        Description = displayAttribute?.Description,
                        Options = displayAttribute?.Options,
                        Value = prop.GetValue(extraValue)
                    });
                }

                return model;
            }
            return null;
        }
        public void SaveExtraParameters(Guid scheduledJobId, IFormCollection form)
        {
            ScheduledPlugInWithExtraParametersAttribute? attribute = GetScheduledJobExtraParametersAttribute(scheduledJobId);

            if (attribute != null)
            {
                var extraParameterDefType = attribute.ExtraParameterDefinition;

                var extraValue = extraParameterDefType.CreateInstance<ScheduledJobExtraParametersBase>();

                foreach (var field in form)
                {
                    var property = extraParameterDefType.GetProperty(field.Key);

                    if (property != null && field.Value.Count > 0)
                    {
                        if (property.PropertyType == typeof(string))
                        {
                            property.SetValue(extraValue, field.Value.ToString());
                        }
                        else
                        {
                            try
                            {
                                var singleValue = field.Value.FirstOrDefault()?.ToString();


                                if (property.PropertyType == typeof(bool) && singleValue == "on")
                                {
                                    property.SetValue(extraValue, true);
                                }
                                else if (property.PropertyType == typeof(bool) && string.IsNullOrWhiteSpace(singleValue))
                                {
                                    property.SetValue(extraValue, false);
                                }
                                else
                                {
                                    TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);

                                    if (!string.IsNullOrWhiteSpace(singleValue) && converter != null && converter.CanConvertFrom(typeof(string)))
                                    {
                                        var value = converter.ConvertFromString(singleValue);
                                        property.SetValue(extraValue, value);
                                    }
                                    else
                                    {
                                        property.SetValue(extraValue, property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null);
                                    }
                                }

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }

                _extraParametersDataService.Save(scheduledJobId, extraValue);
            }
        }
        public ScheduledPlugInWithExtraParametersAttribute GetScheduledJobExtraParametersAttribute(Guid scheduledJobId)
        {
            var scheduledJob = _scheduledJobRepository.Get(scheduledJobId);
            var attribute = Type.GetType($"{scheduledJob.TypeName}, {scheduledJob.AssemblyName}")?.GetCustomAttributes(false)?.OfType<ScheduledPlugInWithExtraParametersAttribute>()?.FirstOrDefault();
            return attribute;
        }
    }
}
