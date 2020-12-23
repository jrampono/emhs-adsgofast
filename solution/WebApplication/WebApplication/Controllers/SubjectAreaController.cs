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
    public partial class SubjectAreaController : BaseController
    {
        protected readonly AdsGoFastContext _context;
        

        public SubjectAreaController(AdsGoFastContext context, ISecurityAccessProvider securityAccessProvider, IEntityRoleProvider roleProvider) : base(securityAccessProvider, roleProvider)
        {
            Name = "SubjectArea";
            _context = context;
        }

        // GET: SubjectArea
        public async Task<IActionResult> Index()
        {
            var adsGoFastContext = _context.SubjectArea.Include(s => s.SubjectAreaForm);
            return View(await adsGoFastContext.ToListAsync());
        }

        // GET: SubjectArea/Details/5
        [ChecksUserAccess]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectArea = await _context.SubjectArea
                .Include(s => s.SubjectAreaForm)
                .FirstOrDefaultAsync(m => m.SubjectAreaId == id);
            if (subjectArea == null)
                return NotFound();
            if (!await CanPerformCurrentActionOnRecord(subjectArea))
                return new ForbidResult();


            return View(subjectArea);
        }

        // GET: SubjectArea/Create
        public IActionResult Create()
        {
            ViewData["SubjectAreaFormId"] = new SelectList(_context.SubjectAreaForm.OrderBy(x=>x.SubjectAreaFormId), "SubjectAreaFormId", "SubjectAreaFormId");
     SubjectArea subjectArea = new SubjectArea();
            subjectArea.ActiveYn = true;
            return View(subjectArea);
        }

        // POST: SubjectArea/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ChecksUserAccess]
        public async Task<IActionResult> Create([Bind("SubjectAreaId,SubjectAreaName,ActiveYn,SubjectAreaFormId,DefaultTargetSchema,UpdatedBy")] SubjectArea subjectArea)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subjectArea);
                if (!await CanPerformCurrentActionOnRecord(subjectArea))
                {
                    return new ForbidResult();
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexDataTable));
            }
        ViewData["SubjectAreaFormId"] = new SelectList(_context.SubjectAreaForm.OrderBy(x=>x.SubjectAreaFormId), "SubjectAreaFormId", "SubjectAreaFormId", subjectArea.SubjectAreaFormId);
            return View(subjectArea);
        }

        // GET: SubjectArea/Edit/5
        [ChecksUserAccess()]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectArea = await _context.SubjectArea.FindAsync(id);
            if (subjectArea == null)
                return NotFound();

            if (!await CanPerformCurrentActionOnRecord(subjectArea))
                return new ForbidResult();
        ViewData["SubjectAreaFormId"] = new SelectList(_context.SubjectAreaForm.OrderBy(x=>x.SubjectAreaFormId), "SubjectAreaFormId", "SubjectAreaFormId", subjectArea.SubjectAreaFormId);
            return View(subjectArea);
        }

        // POST: SubjectArea/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ChecksUserAccess]
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

                    if (!await CanPerformCurrentActionOnRecord(subjectArea))
                        return new ForbidResult();
			
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
                return RedirectToAction(nameof(IndexDataTable));
            }
        ViewData["SubjectAreaFormId"] = new SelectList(_context.SubjectAreaForm.OrderBy(x=>x.SubjectAreaFormId), "SubjectAreaFormId", "SubjectAreaFormId", subjectArea.SubjectAreaFormId);
            return View(subjectArea);
        }

        // GET: SubjectArea/Delete/5
        [ChecksUserAccess]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectArea = await _context.SubjectArea
                .Include(s => s.SubjectAreaForm)
                .FirstOrDefaultAsync(m => m.SubjectAreaId == id);
            if (subjectArea == null)
                return NotFound();
		
            if (!await CanPerformCurrentActionOnRecord(subjectArea))
                return new ForbidResult();

            return View(subjectArea);
        }

        // POST: SubjectArea/Delete/5
        [HttpPost, ActionName("Delete")]
        [ChecksUserAccess()]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subjectArea = await _context.SubjectArea.FindAsync(id);

            if (!await CanPerformCurrentActionOnRecord(subjectArea))
                return new ForbidResult();
		
            _context.SubjectArea.Remove(subjectArea);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexDataTable));
        }

        private bool SubjectAreaExists(int id)
        {
            return _context.SubjectArea.Any(e => e.SubjectAreaId == id);
        }
    }
}
