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
    public partial class TaskGroupController : BaseController
    {
        protected readonly AdsGoFastContext _context;
        

        public TaskGroupController(AdsGoFastContext context, SecurityAccessProvider securityAccessProvider) : base(securityAccessProvider)
        {
            Name = "TaskGroup";
            _context = context;
        }

        // GET: TaskGroup
        public async Task<IActionResult> Index()
        {
            var adsGoFastContext = _context.TaskGroup.Include(t => t.SubjectArea);
            return View(await adsGoFastContext.ToListAsync());
        }

        // GET: TaskGroup/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskGroup = await _context.TaskGroup
                .Include(t => t.SubjectArea)
                .FirstOrDefaultAsync(m => m.TaskGroupId == id);
            if (taskGroup == null)
            {
                return NotFound();
            }

            return View(taskGroup);
        }

        // GET: TaskGroup/Create
        public IActionResult Create()
        {
            ViewData["SubjectAreaId"] = new SelectList(_context.SubjectArea.OrderBy(x=>x.SubjectAreaId), "SubjectAreaId", "SubjectAreaId");
     TaskGroup taskGroup = new TaskGroup();
            taskGroup.ActiveYn = true;
            return View(taskGroup);
        }

        // POST: TaskGroup/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaskGroupId,TaskGroupName,TaskGroupPriority,TaskGroupConcurrency,TaskGroupJson,SubjectAreaId,ActiveYn")] TaskGroup taskGroup)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taskGroup);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexDataTable));
            }
        ViewData["SubjectAreaId"] = new SelectList(_context.SubjectArea.OrderBy(x=>x.SubjectAreaId), "SubjectAreaId", "SubjectAreaId", taskGroup.SubjectAreaId);
            return View(taskGroup);
        }

        // GET: TaskGroup/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskGroup = await _context.TaskGroup.FindAsync(id);
            if (taskGroup == null)
            {
                return NotFound();
            }
        ViewData["SubjectAreaId"] = new SelectList(_context.SubjectArea.OrderBy(x=>x.SubjectAreaId), "SubjectAreaId", "SubjectAreaId", taskGroup.SubjectAreaId);
            return View(taskGroup);
        }

        // POST: TaskGroup/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("TaskGroupId,TaskGroupName,TaskGroupPriority,TaskGroupConcurrency,TaskGroupJson,SubjectAreaId,ActiveYn")] TaskGroup taskGroup)
        {
            if (id != taskGroup.TaskGroupId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskGroup);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskGroupExists(taskGroup.TaskGroupId))
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
        ViewData["SubjectAreaId"] = new SelectList(_context.SubjectArea.OrderBy(x=>x.SubjectAreaId), "SubjectAreaId", "SubjectAreaId", taskGroup.SubjectAreaId);
            return View(taskGroup);
        }

        // GET: TaskGroup/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskGroup = await _context.TaskGroup
                .Include(t => t.SubjectArea)
                .FirstOrDefaultAsync(m => m.TaskGroupId == id);
            if (taskGroup == null)
            {
                return NotFound();
            }

            return View(taskGroup);
        }

        // POST: TaskGroup/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var taskGroup = await _context.TaskGroup.FindAsync(id);
            _context.TaskGroup.Remove(taskGroup);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexDataTable));
        }

        private bool TaskGroupExists(long id)
        {
            return _context.TaskGroup.Any(e => e.TaskGroupId == id);
        }
    }
}
