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
    public partial class TaskMasterWaterMarkController : BaseController
    {
        protected readonly AdsGoFastContext _context;
        

        public TaskMasterWaterMarkController(AdsGoFastContext context, SecurityAccessProvider securityAccessProvider) : base(securityAccessProvider)
        {
            Name = "TaskMasterWaterMark";
            _context = context;
        }

        // GET: TaskMasterWaterMark
        public async Task<IActionResult> Index()
        {
            var adsGoFastContext = _context.TaskMasterWaterMark.Include(t => t.TaskMaster);
            return View(await adsGoFastContext.ToListAsync());
        }

        // GET: TaskMasterWaterMark/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskMasterWaterMark = await _context.TaskMasterWaterMark
                .Include(t => t.TaskMaster)
                .FirstOrDefaultAsync(m => m.TaskMasterId == id);
            if (taskMasterWaterMark == null)
            {
                return NotFound();
            }

            return View(taskMasterWaterMark);
        }

        // GET: TaskMasterWaterMark/Create
        public IActionResult Create()
        {
            ViewData["TaskMasterId"] = new SelectList(_context.TaskMaster.OrderBy(x=>x.TaskMasterName), "TaskMasterId", "TaskMasterName");
     TaskMasterWaterMark taskMasterWaterMark = new TaskMasterWaterMark();
            taskMasterWaterMark.ActiveYn = true;
            return View(taskMasterWaterMark);
        }

        // POST: TaskMasterWaterMark/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaskMasterId,TaskMasterWaterMarkColumn,TaskMasterWaterMarkColumnType,TaskMasterWaterMarkDateTime,TaskMasterWaterMarkBigInt,TaskWaterMarkJson,ActiveYn,UpdatedOn")] TaskMasterWaterMark taskMasterWaterMark)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taskMasterWaterMark);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexDataTable));
            }
        ViewData["TaskMasterId"] = new SelectList(_context.TaskMaster.OrderBy(x=>x.TaskMasterName), "TaskMasterId", "TaskMasterName", taskMasterWaterMark.TaskMasterId);
            return View(taskMasterWaterMark);
        }

        // GET: TaskMasterWaterMark/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskMasterWaterMark = await _context.TaskMasterWaterMark.FindAsync(id);
            if (taskMasterWaterMark == null)
            {
                return NotFound();
            }
        ViewData["TaskMasterId"] = new SelectList(_context.TaskMaster.OrderBy(x=>x.TaskMasterName), "TaskMasterId", "TaskMasterName", taskMasterWaterMark.TaskMasterId);
            return View(taskMasterWaterMark);
        }

        // POST: TaskMasterWaterMark/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("TaskMasterId,TaskMasterWaterMarkColumn,TaskMasterWaterMarkColumnType,TaskMasterWaterMarkDateTime,TaskMasterWaterMarkBigInt,TaskWaterMarkJson,ActiveYn,UpdatedOn")] TaskMasterWaterMark taskMasterWaterMark)
        {
            if (id != taskMasterWaterMark.TaskMasterId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskMasterWaterMark);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskMasterWaterMarkExists(taskMasterWaterMark.TaskMasterId))
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
        ViewData["TaskMasterId"] = new SelectList(_context.TaskMaster.OrderBy(x=>x.TaskMasterName), "TaskMasterId", "TaskMasterName", taskMasterWaterMark.TaskMasterId);
            return View(taskMasterWaterMark);
        }

        // GET: TaskMasterWaterMark/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskMasterWaterMark = await _context.TaskMasterWaterMark
                .Include(t => t.TaskMaster)
                .FirstOrDefaultAsync(m => m.TaskMasterId == id);
            if (taskMasterWaterMark == null)
            {
                return NotFound();
            }

            return View(taskMasterWaterMark);
        }

        // POST: TaskMasterWaterMark/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var taskMasterWaterMark = await _context.TaskMasterWaterMark.FindAsync(id);
            _context.TaskMasterWaterMark.Remove(taskMasterWaterMark);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexDataTable));
        }

        private bool TaskMasterWaterMarkExists(long id)
        {
            return _context.TaskMasterWaterMark.Any(e => e.TaskMasterId == id);
        }
    }
}
