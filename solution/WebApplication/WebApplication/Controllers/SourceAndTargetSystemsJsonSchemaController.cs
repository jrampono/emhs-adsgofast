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
    public partial class SourceAndTargetSystemsJsonSchemaController : BaseController
    {
        protected readonly AdsGoFastContext _context;
        

        public SourceAndTargetSystemsJsonSchemaController(AdsGoFastContext context, SecurityAccessProvider securityAccessProvider) : base(securityAccessProvider)
        {
            Name = "SourceAndTargetSystemsJsonSchema";
            _context = context;
        }

        // GET: SourceAndTargetSystemsJsonSchema
        public async Task<IActionResult> Index()
        {
            return View(await _context.SourceAndTargetSystemsJsonSchema.ToListAsync());
        }

        // GET: SourceAndTargetSystemsJsonSchema/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceAndTargetSystemsJsonSchema = await _context.SourceAndTargetSystemsJsonSchema
                .FirstOrDefaultAsync(m => m.SystemType == id);
            if (sourceAndTargetSystemsJsonSchema == null)
            {
                return NotFound();
            }

            return View(sourceAndTargetSystemsJsonSchema);
        }

        // GET: SourceAndTargetSystemsJsonSchema/Create
        public IActionResult Create()
        {
     SourceAndTargetSystemsJsonSchema sourceAndTargetSystemsJsonSchema = new SourceAndTargetSystemsJsonSchema();
            return View(sourceAndTargetSystemsJsonSchema);
        }

        // POST: SourceAndTargetSystemsJsonSchema/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SystemType,JsonSchema")] SourceAndTargetSystemsJsonSchema sourceAndTargetSystemsJsonSchema)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sourceAndTargetSystemsJsonSchema);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexDataTable));
            }
            return View(sourceAndTargetSystemsJsonSchema);
        }

        // GET: SourceAndTargetSystemsJsonSchema/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceAndTargetSystemsJsonSchema = await _context.SourceAndTargetSystemsJsonSchema.FindAsync(id);
            if (sourceAndTargetSystemsJsonSchema == null)
            {
                return NotFound();
            }
            return View(sourceAndTargetSystemsJsonSchema);
        }

        // POST: SourceAndTargetSystemsJsonSchema/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("SystemType,JsonSchema")] SourceAndTargetSystemsJsonSchema sourceAndTargetSystemsJsonSchema)
        {
            if (id != sourceAndTargetSystemsJsonSchema.SystemType)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sourceAndTargetSystemsJsonSchema);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SourceAndTargetSystemsJsonSchemaExists(sourceAndTargetSystemsJsonSchema.SystemType))
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
            return View(sourceAndTargetSystemsJsonSchema);
        }

        // GET: SourceAndTargetSystemsJsonSchema/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceAndTargetSystemsJsonSchema = await _context.SourceAndTargetSystemsJsonSchema
                .FirstOrDefaultAsync(m => m.SystemType == id);
            if (sourceAndTargetSystemsJsonSchema == null)
            {
                return NotFound();
            }

            return View(sourceAndTargetSystemsJsonSchema);
        }

        // POST: SourceAndTargetSystemsJsonSchema/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var sourceAndTargetSystemsJsonSchema = await _context.SourceAndTargetSystemsJsonSchema.FindAsync(id);
            _context.SourceAndTargetSystemsJsonSchema.Remove(sourceAndTargetSystemsJsonSchema);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexDataTable));
        }

        private bool SourceAndTargetSystemsJsonSchemaExists(string id)
        {
            return _context.SourceAndTargetSystemsJsonSchema.Any(e => e.SystemType == id);
        }
    }
}
