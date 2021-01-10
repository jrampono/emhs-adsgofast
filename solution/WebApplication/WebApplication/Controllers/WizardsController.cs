using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication.Models;


namespace WebApplication.Controllers.Customisations
{
    public class WizardsController : Controller
    {
        private readonly AdsGoFastContext _context;
        public WizardsController(AdsGoFastContext context)
        {
            _context = context;
        }

        public IActionResult ExternalFileUpload()
        {
            
            ViewData["UploadSystemId"] = new SelectList(_context.SourceAndTargetSystems.Where(t=> t.SystemType.ToLower() == "azure blob").OrderBy(t => t.SystemName), "SystemId", "SystemName");
            ViewData["EmailSystemId"] = new SelectList(_context.SourceAndTargetSystems.Where(t => t.SystemType.ToLower() == "sendgrid").OrderBy(t => t.SystemName), "SystemId", "SystemName");
            ViewData["TargetSystemId"] = new SelectList(_context.SourceAndTargetSystems.Where(t => t.SystemType.ToLower() == "azure blob" || t.SystemType.ToLower() == "adls").OrderBy(t => t.SystemName), "SystemId", "SystemName");
            ViewData["TaskGroupId"] = new SelectList(_context.TaskGroup.OrderBy(t => t.TaskGroupName), "TaskGroupId", "TaskGroupName");
            ViewData["ScheduleMasterId"] = new SelectList(_context.ScheduleMaster.OrderBy(t => t.ScheduleDesciption), "ScheduleMasterId", "ScheduleDesciption");
            ViewData["ExternalParties"] = ""; 


            return View();
        }
    }
}
