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
    public partial class SourceAndTargetSystemsController : BaseController
    {
        protected readonly AdsGoFastContext _context;
        

        public SourceAndTargetSystemsController(AdsGoFastContext context, SecurityAccessProvider securityAccessProvider) : base(securityAccessProvider)
        {
            Name = "SourceAndTargetSystems";
            _context = context;
        }

        // GET: SourceAndTargetSystems
        public async Task<IActionResult> Index()
        {
            return View(await _context.SourceAndTargetSystems.ToListAsync());
        }

        // GET: SourceAndTargetSystems/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceAndTargetSystems = await _context.SourceAndTargetSystems
                .FirstOrDefaultAsync(m => m.SystemId == id);
            if (sourceAndTargetSystems == null)
            {
                return NotFound();
            }

            return View(sourceAndTargetSystems);
        }

        // GET: SourceAndTargetSystems/Create
        public IActionResult Create()
        {
     SourceAndTargetSystems sourceAndTargetSystems = new SourceAndTargetSystems();
            sourceAndTargetSystems.ActiveYn = true;
            return View(sourceAndTargetSystems);
        }

        // POST: SourceAndTargetSystems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SystemId,SystemName,SystemType,SystemDescription,SystemServer,SystemAuthType,SystemUserName,SystemSecretName,SystemKeyVaultBaseUrl,SystemJson,ActiveYn")] SourceAndTargetSystems sourceAndTargetSystems)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sourceAndTargetSystems);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexDataTable));
            }
            return View(sourceAndTargetSystems);
        }

        // GET: SourceAndTargetSystems/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceAndTargetSystems = await _context.SourceAndTargetSystems.FindAsync(id);
            if (sourceAndTargetSystems == null)
            {
                return NotFound();
            }
            return View(sourceAndTargetSystems);
        }

        // POST: SourceAndTargetSystems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("SystemId,SystemName,SystemType,SystemDescription,SystemServer,SystemAuthType,SystemUserName,SystemSecretName,SystemKeyVaultBaseUrl,SystemJson,ActiveYn")] SourceAndTargetSystems sourceAndTargetSystems)
        {
            if (id != sourceAndTargetSystems.SystemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sourceAndTargetSystems);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SourceAndTargetSystemsExists(sourceAndTargetSystems.SystemId))
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
            return View(sourceAndTargetSystems);
        }

        // GET: SourceAndTargetSystems/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceAndTargetSystems = await _context.SourceAndTargetSystems
                .FirstOrDefaultAsync(m => m.SystemId == id);
            if (sourceAndTargetSystems == null)
            {
                return NotFound();
            }

            return View(sourceAndTargetSystems);
        }

        // POST: SourceAndTargetSystems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var sourceAndTargetSystems = await _context.SourceAndTargetSystems.FindAsync(id);
            _context.SourceAndTargetSystems.Remove(sourceAndTargetSystems);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexDataTable));
        }

        private bool SourceAndTargetSystemsExists(long id)
        {
            return _context.SourceAndTargetSystems.Any(e => e.SystemId == id);
        }
    }
}
