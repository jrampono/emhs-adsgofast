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
    public partial class TaskTypeMappingController : BaseController
    {
        protected readonly AdsGoFastContext _context;
        

        public TaskTypeMappingController(AdsGoFastContext context, ISecurityAccessProvider securityAccessProvider, IEntityRoleProvider roleProvider) : base(securityAccessProvider, roleProvider)
        {
            Name = "TaskTypeMapping";
            _context = context;
        }

        // GET: TaskTypeMapping
        public async Task<IActionResult> Index()
        {
            var adsGoFastContext = _context.TaskTypeMapping.Include(t => t.TaskType);
            return View(await adsGoFastContext.ToListAsync());
        }

        // GET: TaskTypeMapping/Details/5
        [ChecksUserAccess]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskTypeMapping = await _context.TaskTypeMapping
                .Include(t => t.TaskType)
                .FirstOrDefaultAsync(m => m.TaskTypeMappingId == id);
            if (taskTypeMapping == null)
                return NotFound();
            if (!await CanPerformCurrentActionOnRecord(taskTypeMapping))
                return new ForbidResult();


            return View(taskTypeMapping);
        }

        // GET: TaskTypeMapping/Create
        public IActionResult Create()
        {
            ViewData["TaskTypeId"] = new SelectList(_context.TaskType.OrderBy(x=>x.TaskTypeName), "TaskTypeId", "TaskTypeName");
     TaskTypeMapping taskTypeMapping = new TaskTypeMapping();
            taskTypeMapping.ActiveYn = true;
            return View(taskTypeMapping);
        }

        // POST: TaskTypeMapping/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ChecksUserAccess]
        public async Task<IActionResult> Create([Bind("TaskTypeMappingId,TaskTypeId,MappingType,MappingName,SourceSystemType,SourceType,TargetSystemType,TargetType,TaskDatafactoryIr,TaskTypeJson,ActiveYn,TaskMasterJsonSchema,TaskInstanceJsonSchema")] TaskTypeMapping taskTypeMapping)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taskTypeMapping);
                if (!await CanPerformCurrentActionOnRecord(taskTypeMapping))
                {
                    return new ForbidResult();
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexDataTable));
            }
        ViewData["TaskTypeId"] = new SelectList(_context.TaskType.OrderBy(x=>x.TaskTypeName), "TaskTypeId", "TaskTypeName", taskTypeMapping.TaskTypeId);
            return View(taskTypeMapping);
        }

        // GET: TaskTypeMapping/Edit/5
        [ChecksUserAccess()]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskTypeMapping = await _context.TaskTypeMapping.FindAsync(id);
            if (taskTypeMapping == null)
                return NotFound();

            if (!await CanPerformCurrentActionOnRecord(taskTypeMapping))
                return new ForbidResult();
        ViewData["TaskTypeId"] = new SelectList(_context.TaskType.OrderBy(x=>x.TaskTypeName), "TaskTypeId", "TaskTypeName", taskTypeMapping.TaskTypeId);
            return View(taskTypeMapping);
        }

        // POST: TaskTypeMapping/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ChecksUserAccess]
        public async Task<IActionResult> Edit(int id, [Bind("TaskTypeMappingId,TaskTypeId,MappingType,MappingName,SourceSystemType,SourceType,TargetSystemType,TargetType,TaskDatafactoryIr,TaskTypeJson,ActiveYn,TaskMasterJsonSchema,TaskInstanceJsonSchema")] TaskTypeMapping taskTypeMapping)
        {
            if (id != taskTypeMapping.TaskTypeMappingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskTypeMapping);

                    if (!await CanPerformCurrentActionOnRecord(taskTypeMapping))
                        return new ForbidResult();
			
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskTypeMappingExists(taskTypeMapping.TaskTypeMappingId))
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
        ViewData["TaskTypeId"] = new SelectList(_context.TaskType.OrderBy(x=>x.TaskTypeName), "TaskTypeId", "TaskTypeName", taskTypeMapping.TaskTypeId);
            return View(taskTypeMapping);
        }

        // GET: TaskTypeMapping/Delete/5
        [ChecksUserAccess]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskTypeMapping = await _context.TaskTypeMapping
                .Include(t => t.TaskType)
                .FirstOrDefaultAsync(m => m.TaskTypeMappingId == id);
            if (taskTypeMapping == null)
                return NotFound();
		
            if (!await CanPerformCurrentActionOnRecord(taskTypeMapping))
                return new ForbidResult();

            return View(taskTypeMapping);
        }

        // POST: TaskTypeMapping/Delete/5
        [HttpPost, ActionName("Delete")]
        [ChecksUserAccess()]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskTypeMapping = await _context.TaskTypeMapping.FindAsync(id);

            if (!await CanPerformCurrentActionOnRecord(taskTypeMapping))
                return new ForbidResult();
		
            _context.TaskTypeMapping.Remove(taskTypeMapping);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexDataTable));
        }

        private bool TaskTypeMappingExists(int id)
        {
            return _context.TaskTypeMapping.Any(e => e.TaskTypeMappingId == id);
        }
    }
}
