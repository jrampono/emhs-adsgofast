using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using WebApplication.Services;
using WebApplication.Framework;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public partial class TaskMasterController : BaseController
    {
        protected readonly AdsGoFastContext _context;
        

        public TaskMasterController(AdsGoFastContext context, ISecurityAccessProvider securityAccessProvider, IEntityRoleProvider roleProvider) : base(securityAccessProvider, roleProvider)
        {
            Name = "TaskMaster";
            _context = context;
        }

        // GET: TaskMaster
        public async Task<IActionResult> Index()
        {
            var adsGoFastContext = _context.TaskMaster.Include(t => t.ScheduleMaster).Include(t => t.SourceSystem).Include(t => t.TargetSystem).Include(t => t.TaskGroup).Include(t => t.TaskType);
            return View(await adsGoFastContext.ToListAsync());
        }

        // GET: TaskMaster/Details/5
        [ChecksUserAccess]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskMaster = await _context.TaskMaster
                .Include(t => t.ScheduleMaster)
                .Include(t => t.SourceSystem)
                .Include(t => t.TargetSystem)
                .Include(t => t.TaskGroup)
                .Include(t => t.TaskType)
                .FirstOrDefaultAsync(m => m.TaskMasterId == id);
            if (taskMaster == null)
                return NotFound();
            if (!await CanPerformCurrentActionOnRecord(taskMaster))
                return new ForbidResult();


            return View(taskMaster);
        }

        // GET: TaskMaster/Create
        public IActionResult Create(int? TaskGroupId)
        {
            ViewData["ScheduleMasterId"] = new SelectList(_context.ScheduleMaster.OrderBy(x=>x.ScheduleDesciption), "ScheduleMasterId", "ScheduleDesciption");
            ViewData["SourceSystemId"] = new SelectList(_context.SourceAndTargetSystems.OrderBy(x=>x.SystemName), "SystemId", "SystemName");
            ViewData["TargetSystemId"] = new SelectList(_context.SourceAndTargetSystems.OrderBy(x=>x.SystemName), "SystemId", "SystemName");

            if (TaskGroupId != null)
            {
                ViewData["TaskGroupId"] = new SelectList(_context.TaskGroup.Where(x => x.TaskGroupId == TaskGroupId), "TaskGroupId", "TaskGroupName");
            }
            else
            {
                ViewData["TaskGroupId"] = new SelectList(_context.TaskGroup.OrderBy(x => x.TaskGroupName), "TaskGroupId", "TaskGroupName");
            }
            ViewData["TaskTypeId"] = new SelectList(_context.TaskType.OrderBy(x=>x.TaskTypeName), "TaskTypeId", "TaskTypeName");
     TaskMaster taskMaster = new TaskMaster();
            taskMaster.ActiveYn = true;
            return View(taskMaster);
        }


        // POST: TaskMaster/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ChecksUserAccess]
        public async Task<IActionResult> Create([Bind("TaskMasterId,TaskMasterName,TaskTypeId,TaskGroupId,ScheduleMasterId,SourceSystemId,TargetSystemId,DegreeOfCopyParallelism,AllowMultipleActiveInstances,TaskDatafactoryIr,TaskMasterJson,ActiveYn,DependencyChainTag,DataFactoryId")] TaskMaster taskMaster)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taskMaster);
                if (!await CanPerformCurrentActionOnRecord(taskMaster))
                {
                    return new ForbidResult();
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexDataTable));
            }
        ViewData["ScheduleMasterId"] = new SelectList(_context.ScheduleMaster.OrderBy(x=>x.ScheduleDesciption), "ScheduleMasterId", "ScheduleDesciption", taskMaster.ScheduleMasterId);
        ViewData["SourceSystemId"] = new SelectList(_context.SourceAndTargetSystems.OrderBy(x=>x.SystemName), "SystemId", "SystemName", taskMaster.SourceSystemId);
        ViewData["TargetSystemId"] = new SelectList(_context.SourceAndTargetSystems.OrderBy(x=>x.SystemName), "SystemId", "SystemName", taskMaster.TargetSystemId);
        ViewData["TaskGroupId"] = new SelectList(_context.TaskGroup.OrderBy(x=>x.TaskGroupName), "TaskGroupId", "TaskGroupName", taskMaster.TaskGroupId);
        ViewData["TaskTypeId"] = new SelectList(_context.TaskType.OrderBy(x=>x.TaskTypeName), "TaskTypeId", "TaskTypeName", taskMaster.TaskTypeId);
            return View(taskMaster);
        }

        // GET: TaskMaster/Edit/5
        [ChecksUserAccess()]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskMaster = await _context.TaskMaster.FindAsync(id);
            if (taskMaster == null)
                return NotFound();

            if (!await CanPerformCurrentActionOnRecord(taskMaster))
                return new ForbidResult();
        ViewData["ScheduleMasterId"] = new SelectList(_context.ScheduleMaster.OrderBy(x=>x.ScheduleDesciption), "ScheduleMasterId", "ScheduleDesciption", taskMaster.ScheduleMasterId);
        ViewData["SourceSystemId"] = new SelectList(_context.SourceAndTargetSystems.OrderBy(x=>x.SystemName), "SystemId", "SystemName", taskMaster.SourceSystemId);
        ViewData["TargetSystemId"] = new SelectList(_context.SourceAndTargetSystems.OrderBy(x=>x.SystemName), "SystemId", "SystemName", taskMaster.TargetSystemId);
        ViewData["TaskGroupId"] = new SelectList(_context.TaskGroup.OrderBy(x=>x.TaskGroupName), "TaskGroupId", "TaskGroupName", taskMaster.TaskGroupId);
        ViewData["TaskTypeId"] = new SelectList(_context.TaskType.OrderBy(x=>x.TaskTypeName), "TaskTypeId", "TaskTypeName", taskMaster.TaskTypeId);
            return View(taskMaster);
        }

        // POST: TaskMaster/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ChecksUserAccess]
        public async Task<IActionResult> Edit(long id, [Bind("TaskMasterId,TaskMasterName,TaskTypeId,TaskGroupId,ScheduleMasterId,SourceSystemId,TargetSystemId,DegreeOfCopyParallelism,AllowMultipleActiveInstances,TaskDatafactoryIr,TaskMasterJson,ActiveYn,DependencyChainTag,DataFactoryId")] TaskMaster taskMaster)
        {
            if (id != taskMaster.TaskMasterId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskMaster);

                    if (!await CanPerformCurrentActionOnRecord(taskMaster))
                        return new ForbidResult();
			
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskMasterExists(taskMaster.TaskMasterId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(IndexDataTable));
            }
        ViewData["ScheduleMasterId"] = new SelectList(_context.ScheduleMaster.OrderBy(x=>x.ScheduleDesciption), "ScheduleMasterId", "ScheduleDesciption", taskMaster.ScheduleMasterId);
        ViewData["SourceSystemId"] = new SelectList(_context.SourceAndTargetSystems.OrderBy(x=>x.SystemName), "SystemId", "SystemName", taskMaster.SourceSystemId);
        ViewData["TargetSystemId"] = new SelectList(_context.SourceAndTargetSystems.OrderBy(x=>x.SystemName), "SystemId", "SystemName", taskMaster.TargetSystemId);
        ViewData["TaskGroupId"] = new SelectList(_context.TaskGroup.OrderBy(x=>x.TaskGroupName), "TaskGroupId", "TaskGroupName", taskMaster.TaskGroupId);
        ViewData["TaskTypeId"] = new SelectList(_context.TaskType.OrderBy(x=>x.TaskTypeName), "TaskTypeId", "TaskTypeName", taskMaster.TaskTypeId);
            return View(taskMaster);
        }

        // GET: TaskMaster/Delete/5
        [ChecksUserAccess]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskMaster = await _context.TaskMaster
                .Include(t => t.ScheduleMaster)
                .Include(t => t.SourceSystem)
                .Include(t => t.TargetSystem)
                .Include(t => t.TaskGroup)
                .Include(t => t.TaskType)
                .FirstOrDefaultAsync(m => m.TaskMasterId == id);
            if (taskMaster == null)
                return NotFound();
		
            if (!await CanPerformCurrentActionOnRecord(taskMaster))
                return new ForbidResult();

            return View(taskMaster);
        }

        // POST: TaskMaster/Delete/5
        [HttpPost, ActionName("Delete")]
        [ChecksUserAccess()]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var taskMaster = await _context.TaskMaster.FindAsync(id);

            if (!await CanPerformCurrentActionOnRecord(taskMaster))
                return new ForbidResult();
		
            _context.TaskMaster.Remove(taskMaster);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexDataTable));
        }

        private bool TaskMasterExists(long id)
        {
            return _context.TaskMaster.Any(e => e.TaskMasterId == id);
        }
    }
}
