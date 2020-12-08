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
    public partial class ScheduleInstanceController : BaseController
    {
        protected readonly AdsGoFastContext _context;
        

        public ScheduleInstanceController(AdsGoFastContext context, SecurityAccessProvider securityAccessProvider) : base(securityAccessProvider)
        {
            Name = "ScheduleInstance";
            _context = context;
        }

        // GET: ScheduleInstance
        public async Task<IActionResult> Index()
        {
            var adsGoFastContext = _context.ScheduleInstance.Include(s => s.ScheduleMaster);
            return View(await adsGoFastContext.ToListAsync());
        }

        // GET: ScheduleInstance/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduleInstance = await _context.ScheduleInstance
                .Include(s => s.ScheduleMaster)
                .FirstOrDefaultAsync(m => m.ScheduleInstanceId == id);
            if (scheduleInstance == null)
            {
                return NotFound();
            }

            return View(scheduleInstance);
        }

        // GET: ScheduleInstance/Create
        public IActionResult Create()
        {
            ViewData["ScheduleMasterId"] = new SelectList(_context.ScheduleMaster.OrderBy(x=>x.ScheduleDesciption), "ScheduleMasterId", "ScheduleDesciption");
     ScheduleInstance scheduleInstance = new ScheduleInstance();
            scheduleInstance.ActiveYn = true;
            return View(scheduleInstance);
        }

        // POST: ScheduleInstance/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ScheduleInstanceId,ScheduleMasterId,ScheduledDateUtc,ScheduledDateTimeOffset,ActiveYn")] ScheduleInstance scheduleInstance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(scheduleInstance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexDataTable));
            }
        ViewData["ScheduleMasterId"] = new SelectList(_context.ScheduleMaster.OrderBy(x=>x.ScheduleDesciption), "ScheduleMasterId", "ScheduleDesciption", scheduleInstance.ScheduleMasterId);
            return View(scheduleInstance);
        }

        // GET: ScheduleInstance/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduleInstance = await _context.ScheduleInstance.FindAsync(id);
            if (scheduleInstance == null)
            {
                return NotFound();
            }
        ViewData["ScheduleMasterId"] = new SelectList(_context.ScheduleMaster.OrderBy(x=>x.ScheduleDesciption), "ScheduleMasterId", "ScheduleDesciption", scheduleInstance.ScheduleMasterId);
            return View(scheduleInstance);
        }

        // POST: ScheduleInstance/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("ScheduleInstanceId,ScheduleMasterId,ScheduledDateUtc,ScheduledDateTimeOffset,ActiveYn")] ScheduleInstance scheduleInstance)
        {
            if (id != scheduleInstance.ScheduleInstanceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(scheduleInstance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleInstanceExists(scheduleInstance.ScheduleInstanceId))
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
        ViewData["ScheduleMasterId"] = new SelectList(_context.ScheduleMaster.OrderBy(x=>x.ScheduleDesciption), "ScheduleMasterId", "ScheduleDesciption", scheduleInstance.ScheduleMasterId);
            return View(scheduleInstance);
        }

        // GET: ScheduleInstance/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduleInstance = await _context.ScheduleInstance
                .Include(s => s.ScheduleMaster)
                .FirstOrDefaultAsync(m => m.ScheduleInstanceId == id);
            if (scheduleInstance == null)
            {
                return NotFound();
            }

            return View(scheduleInstance);
        }

        // POST: ScheduleInstance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var scheduleInstance = await _context.ScheduleInstance.FindAsync(id);
            _context.ScheduleInstance.Remove(scheduleInstance);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexDataTable));
        }

        private bool ScheduleInstanceExists(long id)
        {
            return _context.ScheduleInstance.Any(e => e.ScheduleInstanceId == id);
        }
    }
}
