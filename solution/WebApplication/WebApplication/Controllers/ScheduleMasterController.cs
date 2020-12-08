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
    public partial class ScheduleMasterController : BaseController
    {
        protected readonly AdsGoFastContext _context;
        

        public ScheduleMasterController(AdsGoFastContext context, SecurityAccessProvider securityAccessProvider) : base(securityAccessProvider)
        {
            Name = "ScheduleMaster";
            _context = context;
        }

        // GET: ScheduleMaster
        public async Task<IActionResult> Index()
        {
            return View(await _context.ScheduleMaster.ToListAsync());
        }

        // GET: ScheduleMaster/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduleMaster = await _context.ScheduleMaster
                .FirstOrDefaultAsync(m => m.ScheduleMasterId == id);
            if (scheduleMaster == null)
            {
                return NotFound();
            }

            return View(scheduleMaster);
        }

        // GET: ScheduleMaster/Create
        public IActionResult Create()
        {
     ScheduleMaster scheduleMaster = new ScheduleMaster();
            scheduleMaster.ActiveYn = true;
            return View(scheduleMaster);
        }

        // POST: ScheduleMaster/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ScheduleMasterId,ScheduleDesciption,ScheduleCronExpression,ActiveYn")] ScheduleMaster scheduleMaster)
        {
            if (ModelState.IsValid)
            {
                _context.Add(scheduleMaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexDataTable));
            }
            return View(scheduleMaster);
        }

        // GET: ScheduleMaster/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduleMaster = await _context.ScheduleMaster.FindAsync(id);
            if (scheduleMaster == null)
            {
                return NotFound();
            }
            return View(scheduleMaster);
        }

        // POST: ScheduleMaster/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("ScheduleMasterId,ScheduleDesciption,ScheduleCronExpression,ActiveYn")] ScheduleMaster scheduleMaster)
        {
            if (id != scheduleMaster.ScheduleMasterId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(scheduleMaster);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleMasterExists(scheduleMaster.ScheduleMasterId))
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
            return View(scheduleMaster);
        }

        // GET: ScheduleMaster/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduleMaster = await _context.ScheduleMaster
                .FirstOrDefaultAsync(m => m.ScheduleMasterId == id);
            if (scheduleMaster == null)
            {
                return NotFound();
            }

            return View(scheduleMaster);
        }

        // POST: ScheduleMaster/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var scheduleMaster = await _context.ScheduleMaster.FindAsync(id);
            _context.ScheduleMaster.Remove(scheduleMaster);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexDataTable));
        }

        private bool ScheduleMasterExists(long id)
        {
            return _context.ScheduleMaster.Any(e => e.ScheduleMasterId == id);
        }
    }
}
