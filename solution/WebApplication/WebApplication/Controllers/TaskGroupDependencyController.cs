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
    public partial class TaskGroupDependencyController : BaseController
    {
        protected readonly AdsGoFastContext _context;
        

        public TaskGroupDependencyController(AdsGoFastContext context, SecurityAccessProvider securityAccessProvider) : base(securityAccessProvider)
        {
            Name = "TaskGroupDependency";
            _context = context;
        }

        // GET: TaskGroupDependency
        public async Task<IActionResult> Index()
        {
            var adsGoFastContext = _context.TaskGroupDependency.Include(t => t.AncestorTaskGroup).Include(t => t.DescendantTaskGroup);
            return View(await adsGoFastContext.ToListAsync());
        }

        // GET: TaskGroupDependency/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskGroupDependency = await _context.TaskGroupDependency
                .Include(t => t.AncestorTaskGroup)
                .Include(t => t.DescendantTaskGroup)
                .FirstOrDefaultAsync(m => m.AncestorTaskGroupId == id);
            if (taskGroupDependency == null)
            {
                return NotFound();
            }

            return View(taskGroupDependency);
        }

        // GET: TaskGroupDependency/Create
        public IActionResult Create()
        {
            ViewData["AncestorTaskGroupId"] = new SelectList(_context.TaskGroup.OrderBy(x=>x.TaskGroupName), "TaskGroupId", "TaskGroupName");
            ViewData["DescendantTaskGroupId"] = new SelectList(_context.TaskGroup.OrderBy(x=>x.TaskGroupName), "TaskGroupId", "TaskGroupName");
     TaskGroupDependency taskGroupDependency = new TaskGroupDependency();
            return View(taskGroupDependency);
        }

        // POST: TaskGroupDependency/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AncestorTaskGroupId,DescendantTaskGroupId,DependencyType")] TaskGroupDependency taskGroupDependency)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taskGroupDependency);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexDataTable));
            }
        ViewData["AncestorTaskGroupId"] = new SelectList(_context.TaskGroup.OrderBy(x=>x.TaskGroupName), "TaskGroupId", "TaskGroupName", taskGroupDependency.AncestorTaskGroupId);
        ViewData["DescendantTaskGroupId"] = new SelectList(_context.TaskGroup.OrderBy(x=>x.TaskGroupName), "TaskGroupId", "TaskGroupName", taskGroupDependency.DescendantTaskGroupId);
            return View(taskGroupDependency);
        }

        // GET: TaskGroupDependency/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskGroupDependency = await _context.TaskGroupDependency.FindAsync(id);
            if (taskGroupDependency == null)
            {
                return NotFound();
            }
        ViewData["AncestorTaskGroupId"] = new SelectList(_context.TaskGroup.OrderBy(x=>x.TaskGroupName), "TaskGroupId", "TaskGroupName", taskGroupDependency.AncestorTaskGroupId);
        ViewData["DescendantTaskGroupId"] = new SelectList(_context.TaskGroup.OrderBy(x=>x.TaskGroupName), "TaskGroupId", "TaskGroupName", taskGroupDependency.DescendantTaskGroupId);
            return View(taskGroupDependency);
        }

        // POST: TaskGroupDependency/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("AncestorTaskGroupId,DescendantTaskGroupId,DependencyType")] TaskGroupDependency taskGroupDependency)
        {
            if (id != taskGroupDependency.AncestorTaskGroupId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskGroupDependency);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskGroupDependencyExists(taskGroupDependency.AncestorTaskGroupId))
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
        ViewData["AncestorTaskGroupId"] = new SelectList(_context.TaskGroup.OrderBy(x=>x.TaskGroupName), "TaskGroupId", "TaskGroupName", taskGroupDependency.AncestorTaskGroupId);
        ViewData["DescendantTaskGroupId"] = new SelectList(_context.TaskGroup.OrderBy(x=>x.TaskGroupName), "TaskGroupId", "TaskGroupName", taskGroupDependency.DescendantTaskGroupId);
            return View(taskGroupDependency);
        }

        // GET: TaskGroupDependency/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskGroupDependency = await _context.TaskGroupDependency
                .Include(t => t.AncestorTaskGroup)
                .Include(t => t.DescendantTaskGroup)
                .FirstOrDefaultAsync(m => m.AncestorTaskGroupId == id);
            if (taskGroupDependency == null)
            {
                return NotFound();
            }

            return View(taskGroupDependency);
        }

        // POST: TaskGroupDependency/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var taskGroupDependency = await _context.TaskGroupDependency.FindAsync(id);
            _context.TaskGroupDependency.Remove(taskGroupDependency);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexDataTable));
        }

        private bool TaskGroupDependencyExists(long id)
        {
            return _context.TaskGroupDependency.Any(e => e.AncestorTaskGroupId == id);
        }
    }
}
