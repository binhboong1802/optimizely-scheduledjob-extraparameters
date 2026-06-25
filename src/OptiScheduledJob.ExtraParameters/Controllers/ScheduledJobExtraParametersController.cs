using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OptiScheduledJob.ExtraParameters.Services;

namespace OptiScheduledJob.ExtraParameters.Controllers
{

    [Authorize(Roles = "CmsAdmins")]
    public class ScheduledJobExtraParametersController : Controller
    {
        private readonly IScheduledJobExtraParametersService _scheduledJobExtraParametersService;

        public ScheduledJobExtraParametersController(IScheduledJobExtraParametersService scheduledJobExtraParametersService)
        {
            _scheduledJobExtraParametersService = scheduledJobExtraParametersService;
        }

        public IActionResult GetView(Guid scheduledJobId)
        {
            var model = _scheduledJobExtraParametersService.GetExtraParametersViewModel(scheduledJobId);
            if (model != null)
            {
                return PartialView("_EditView", model);
            }

            return NoContent();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(Guid scheduledJobId, IFormCollection form)
        {
            _scheduledJobExtraParametersService.SaveExtraParameters(scheduledJobId, form);

            return Ok();
        }

        public IActionResult HasSupportForExtraParameters(Guid scheduledJobId)
        {
            var attribute = _scheduledJobExtraParametersService.GetScheduledJobExtraParametersAttribute(scheduledJobId);

            if (attribute != null)
            {
                return Json(true);
            }
            return Json(false);
        }
    }
}
