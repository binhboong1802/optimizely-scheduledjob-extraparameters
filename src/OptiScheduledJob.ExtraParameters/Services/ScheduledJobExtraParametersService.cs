using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using OptiScheduledJob.ExtraParameters.Attributes;
using OptiScheduledJob.ExtraParameters.Models;
using OptiScheduledJob.ExtraParameters.ViewModels;
using System.ComponentModel;
using System.Reflection;

namespace OptiScheduledJob.ExtraParameters.Services
{

    [ServiceConfiguration(typeof(IScheduledJobExtraParametersService))]
    public class ScheduledJobExtraParametersService : IScheduledJobExtraParametersService
    {
        private readonly IScheduledJobRepository _scheduledJobRepository;
        private readonly IScheduledJobExtraParametersDataService _extraParametersDataService;
        private readonly ILogger<ScheduledJobExtraParametersService> _logger;

        public ScheduledJobExtraParametersService(IScheduledJobRepository scheduledJobRepository, IScheduledJobExtraParametersDataService extraParametersDataService, ILogger<ScheduledJobExtraParametersService> logger)
        {
            _scheduledJobRepository = scheduledJobRepository;
            _extraParametersDataService = extraParametersDataService;
            _logger = logger;
        }
        public ExtraParametersViewModel GetExtraParametersViewModel(Guid scheduledJobId)
        {
            ScheduledPlugInWithExtraParametersAttribute? attribute = GetScheduledJobExtraParametersAttribute(scheduledJobId);

            if (attribute != null)
            {
                var extraParameterDefType = attribute.ExtraParameterDefinition;

                var extraValue = _extraParametersDataService.Get(scheduledJobId, extraParameterDefType) ?? Activator.CreateInstance(extraParameterDefType);

                var model = new ExtraParametersViewModel() { ExtraParameterSettings = new List<ExtraParameterSetting>() };

                foreach (var prop in extraValue.GetType().GetProperties())
                {
                    var displayAttribute = prop.GetCustomAttribute<ExtraParametersPropertyDisplayAttribute>();

                    model.ExtraParameterSettings.Add(new ExtraParameterSetting()
                    {
                        Name = prop.Name,
                        DisplayName = displayAttribute?.DisplayName ?? prop.Name,
                        Description = displayAttribute?.Description,
                        Options = displayAttribute?.Options?.Select(o => new SelectListItem(o, o)).ToList(),
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

                var extraValue = (ScheduledJobExtraParametersBase)Activator.CreateInstance(extraParameterDefType);

                foreach (var field in form)
                {
                    var property = extraParameterDefType.GetProperty(field.Key);

                    if (property == null || field.Value.Count == 0)
                    {
                        continue;
                    }

                    var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                    try
                    {
                        if (underlyingType == typeof(bool))
                        {
                            // The view renders a hidden "false" input before the checkbox (value "true"),
                            // so a checked box posts both values for the same key. Treat any "true"/"on" as checked.
                            var isChecked = field.Value.Any(v =>
                                string.Equals(v, "true", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(v, "on", StringComparison.OrdinalIgnoreCase));
                            property.SetValue(extraValue, isChecked);
                        }
                        else if (property.PropertyType == typeof(string))
                        {
                            property.SetValue(extraValue, field.Value.ToString());
                        }
                        else
                        {
                            var singleValue = field.Value.FirstOrDefault();
                            TypeConverter converter = TypeDescriptor.GetConverter(underlyingType);

                            if (!string.IsNullOrWhiteSpace(singleValue) && converter != null && converter.CanConvertFrom(typeof(string)))
                            {
                                // The number/date inputs always post culture-invariant values (e.g. "3.14", "2026-06-25"),
                                // so parse invariantly to stay correct on servers whose culture uses a different separator.
                                property.SetValue(extraValue, converter.ConvertFromInvariantString(singleValue));
                            }
                            else
                            {
                                property.SetValue(extraValue, property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to convert extra parameter '{Field}' for scheduled job {ScheduledJobId}.", field.Key, scheduledJobId);
                    }
                }

                _extraParametersDataService.Save(scheduledJobId, extraValue);
            }
        }
        public ScheduledPlugInWithExtraParametersAttribute GetScheduledJobExtraParametersAttribute(Guid scheduledJobId)
        {
            var scheduledJob = _scheduledJobRepository.Get(scheduledJobId);
            if (scheduledJob == null)
            {
                return null;
            }

            var attribute = Type.GetType($"{scheduledJob.TypeName}, {scheduledJob.AssemblyName}")?.GetCustomAttributes(false)?.OfType<ScheduledPlugInWithExtraParametersAttribute>()?.FirstOrDefault();
            return attribute;
        }
    }
}
