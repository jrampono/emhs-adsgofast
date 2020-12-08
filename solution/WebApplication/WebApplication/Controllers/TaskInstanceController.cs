using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using WebApplication.Services;

using WebApplication.Models;

namespace WebApplication.Controllers
{
    public partial class TaskInstanceController : BaseController
    {
        protected readonly AdsGoFastContext _context;
        

        public TaskInstanceController(AdsGoFastContext context, SecurityAccessProvider securityAccessProvider) : base(securityAccessProvider)
        {
            Name = "TaskInstance";
            _context = context;
        }

        // GET: TaskInstance
        public async Task<IActionResult> Index()
        {
            var adsGoFastContext = _context.TaskInstance.Include(t => t.ScheduleInstance).Include(t => t.TaskMaster);
            return View(await adsGoFastContext.ToListAsync());
        }

        // GET: TaskInstance/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskInstance = await _context.TaskInstance
                .Include(t => t.ScheduleInstance)
                .Include(t => t.TaskMaster)
                .FirstOrDefaultAsync(m => m.TaskInstanceId == id);
            if (taskInstance == null)
            {
                return NotFound();
            }

            return View(taskInstance);
        }

        // GET: TaskInstance/Create
        public IActionResult Create()
        {
            ViewData["ScheduleInstanceId"] = new SelectList(_context.ScheduleInstance.OrderBy(x=>x.ScheduleInstanceId), "ScheduleInstanceId", "ScheduleInstanceId");
            ViewData["TaskMasterId"] = new SelectList(_context.TaskMaster.OrderBy(x=>x.TaskMasterName), "TaskMasterId", "TaskMasterName");
     TaskInstance taskInstance = new TaskInstance();
            taskInstance.ActiveYn = true;
            return View(taskInstance);
        }

        // POST: TaskInstance/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LastExecutionStatus,TaskInstanceId,TaskMasterId,ScheduleInstanceId,ExecutionUid,Adfpipeline,TaskInstanceJson,LastExecutionComment,NumberOfRetries,ActiveYn,CreatedOn,TaskRunnerId,UpdatedOn")] TaskInstance taskInstance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taskInstance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexDataTable));
            }
        ViewData["ScheduleInstanceId"] = new SelectList(_context.ScheduleInstance.OrderBy(x=>x.ScheduleInstanceId), "ScheduleInstanceId", "ScheduleInstanceId", taskInstance.ScheduleInstanceId);
        ViewData["TaskMasterId"] = new SelectList(_context.TaskMaster.OrderBy(x=>x.TaskMasterName), "TaskMasterId", "TaskMasterName", taskInstance.TaskMasterId);
            return View(taskInstance);
        }

        // GET: TaskInstance/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskInstance = await _context.TaskInstance.FindAsync(id);
            if (taskInstance == null)
            {
                return NotFound();
            }
        ViewData["ScheduleInstanceId"] = new SelectList(_context.ScheduleInstance.OrderBy(x=>x.ScheduleInstanceId), "ScheduleInstanceId", "ScheduleInstanceId", taskInstance.ScheduleInstanceId);
        ViewData["TaskMasterId"] = new SelectList(_context.TaskMaster.OrderBy(x=>x.TaskMasterName), "TaskMasterId", "TaskMasterName", taskInstance.TaskMasterId);
            return View(taskInstance);
        }

        // POST: TaskInstance/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("LastExecutionStatus,TaskInstanceId,TaskMasterId,ScheduleInstanceId,ExecutionUid,Adfpipeline,TaskInstanceJson,LastExecutionComment,NumberOfRetries,ActiveYn,CreatedOn,TaskRunnerId,UpdatedOn")] TaskInstance taskInstance)
        {
            if (id != taskInstance.TaskInstanceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskInstance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskInstanceExists(taskInstance.TaskInstanceId))
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
        ViewData["ScheduleInstanceId"] = new SelectList(_context.ScheduleInstance.OrderBy(x=>x.ScheduleInstanceId), "ScheduleInstanceId", "ScheduleInstanceId", taskInstance.ScheduleInstanceId);
        ViewData["TaskMasterId"] = new SelectList(_context.TaskMaster.OrderBy(x=>x.TaskMasterName), "TaskMasterId", "TaskMasterName", taskInstance.TaskMasterId);
            return View(taskInstance);
        }

        // GET: TaskInstance/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskInstance = await _context.TaskInstance
                .Include(t => t.ScheduleInstance)
                .Include(t => t.TaskMaster)
                .FirstOrDefaultAsync(m => m.TaskInstanceId == id);
            if (taskInstance == null)
            {
                return NotFound();
            }

            return View(taskInstance);
        }

        // POST: TaskInstance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var taskInstance = await _context.TaskInstance.FindAsync(id);
            _context.TaskInstance.Remove(taskInstance);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexDataTable));
        }

        private bool TaskInstanceExists(long id)
        {
            return _context.TaskInstance.Any(e => e.TaskInstanceId == id);
        }
    }
}
