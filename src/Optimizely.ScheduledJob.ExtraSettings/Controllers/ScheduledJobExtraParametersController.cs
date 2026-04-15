using Castle.Core.Internal;
using EPiServer.DataAbstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optimizely.ScheduledJob.ExtraSettings.Services;
using Optimizely.ScheduledJob.ExtraSettings.ViewModels;
using System.ComponentModel;

namespace Optimizely.ScheduledJob.ExtraSettings.Controllers
{

    [Authorize(Roles = "CmsAdmins")]
    [Route(RoutePath + "/[action]")]
    public class ScheduledJobExtraParametersController : Controller
    {
        public const string RoutePath = "episerver/admin/scheduledjob/extraparams";

        private readonly IScheduledJobRepository _scheduledJobRepository;
        private readonly IScheduledJobExtraParametersService _scheduledJobExtraParametersService;

        public ScheduledJobExtraParametersController(IScheduledJobRepository scheduledJobRepository, IScheduledJobExtraParametersService scheduledJobExtraParametersService)
        {
            _scheduledJobRepository = scheduledJobRepository;
            _scheduledJobExtraParametersService = scheduledJobExtraParametersService;
        }

        public IActionResult GetView(Guid scheduledJobId)
        {
            ScheduledPlugInWithExtraParametersAttribute? attribute = GetScheduledJobExtraParametersAttribute(scheduledJobId);

            if (attribute != null)
            {
                var extraParameterDefType = attribute.ExtraParameterDefinition;

                var extraValue = _scheduledJobExtraParametersService.Get(scheduledJobId, extraParameterDefType) ?? extraParameterDefType.CreateInstance<ScheduledJobExtraParametersBase>();

                var model = new ExtraParametersViewModel() { ExtraParameterSettings = new List<ExtraParameterSetting>() };

                foreach (var prop in extraValue.GetType().GetProperties())
                {
                    model.ExtraParameterSettings.Add(new ExtraParameterSetting() { Name = prop.Name, Value = prop.GetValue(extraValue) });
                }

                return PartialView("ExtraParameters", model);
            }

            return Content(string.Empty);

        }

        [HttpPost]
        public IActionResult Save(Guid scheduledJobId, IFormCollection form)
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

                                TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);

                                if (!string.IsNullOrWhiteSpace(singleValue) && converter != null && converter.CanConvertFrom(typeof(string)))
                                {

                                    var value = converter.ConvertFromString(singleValue);

                                    property.SetValue(extraValue, value);
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }

                _scheduledJobExtraParametersService.Save(scheduledJobId, extraValue);
            }

            return Content(string.Empty);
        }

        public IActionResult HasSupportForExtraParameters(Guid scheduledJobId)
        {
            ScheduledPlugInWithExtraParametersAttribute? attribute = GetScheduledJobExtraParametersAttribute(scheduledJobId);

            if (attribute != null)
            {
                return Json(true);
            }
            return Json(false);
        }

        private ScheduledPlugInWithExtraParametersAttribute? GetScheduledJobExtraParametersAttribute(Guid scheduledJobId)
        {
            var scheduledJob = _scheduledJobRepository.Get(scheduledJobId);
            var attribute = Type.GetType($"{scheduledJob.TypeName}, {scheduledJob.AssemblyName}")?.GetCustomAttributes(false)?.OfType<ScheduledPlugInWithExtraParametersAttribute>()?.FirstOrDefault();
            return attribute;
        }
    }
}
