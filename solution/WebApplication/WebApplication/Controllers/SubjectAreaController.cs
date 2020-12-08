using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication.Services;

using WebApplication.Models;

namespace WebApplication.Controllers
{
    public partial class SubjectAreaController : BaseController
    {
        protected readonly AdsGoFastContext _context;
        

        public SubjectAreaController(AdsGoFastContext context, SecurityAccessProvider securityAccessProvider) : base(securityAccessProvider)
        {
            Name = "SubjectArea";
            _context = context;
        }

        // GET: SubjectArea
        public async Task<IActionResult> Index()
        {
            return View(await _context.SubjectArea.ToListAsync());
        }

        // GET: SubjectArea/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectArea = await _context.SubjectArea
                .FirstOrDefaultAsync(m => m.SubjectAreaId == id);
            if (subjectArea == null)
            {
                return NotFound();
            }

            return View(subjectArea);
        }

        // GET: SubjectArea/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SubjectArea/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubjectAreaId,SubjectAreaName,ActiveYn,SubjectAreaFormId,DefaultTargetSchema,UpdatedBy")] SubjectArea subjectArea)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subjectArea);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subjectArea);
        }

        // GET: SubjectArea/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectArea = await _context.SubjectArea.FindAsync(id);
            if (subjectArea == null)
            {
                return NotFound();
            }
            return View(subjectArea);
        }

        // POST: SubjectArea/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SubjectAreaId,SubjectAreaName,ActiveYn,SubjectAreaFormId,DefaultTargetSchema,UpdatedBy")] SubjectArea subjectArea)
        {
            if (id != subjectArea.SubjectAreaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subjectArea);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectAreaExists(subjectArea.SubjectAreaId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(subjectArea);
        }

        // GET: SubjectArea/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectArea = await _context.SubjectArea
                .FirstOrDefaultAsync(m => m.SubjectAreaId == id);
            if (subjectArea == null)
            {
                return NotFound();
            }

            return View(subjectArea);
        }

        // POST: SubjectArea/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subjectArea = await _context.SubjectArea.FindAsync(id);
            _context.SubjectArea.Remove(subjectArea);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubjectAreaExists(int id)
        {
            return _context.SubjectArea.Any(e => e.SubjectAreaId == id);
        }
    }
}
