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
    public partial class DataFactoryController : BaseController
    {
        protected readonly AdsGoFastContext _context;
        

        public DataFactoryController(AdsGoFastContext context, ISecurityAccessProvider securityAccessProvider, IEntityRoleProvider roleProvider) : base(securityAccessProvider, roleProvider)
        {
            Name = "DataFactory";
            _context = context;
        }

        // GET: DataFactory
        public async Task<IActionResult> Index()
        {
            return View(await _context.DataFactory.ToListAsync());
        }

        // GET: DataFactory/Details/5
        [ChecksUserAccess]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dataFactory = await _context.DataFactory
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dataFactory == null)
                return NotFound();
            if (!await CanPerformCurrentActionOnRecord(dataFactory))
                return new ForbidResult();


            return View(dataFactory);
        }

        // GET: DataFactory/Create
        public IActionResult Create()
        {
     DataFactory dataFactory = new DataFactory();
            return View(dataFactory);
        }

        // POST: DataFactory/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ChecksUserAccess]
        public async Task<IActionResult> Create([Bind("Id,Name,ResourceGroup,SubscriptionUid,DefaultKeyVaultUrl,LogAnalyticsWorkspaceId")] DataFactory dataFactory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dataFactory);
                if (!await CanPerformCurrentActionOnRecord(dataFactory))
                {
                    return new ForbidResult();
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexDataTable));
            }
            return View(dataFactory);
        }

        // GET: DataFactory/Edit/5
        [ChecksUserAccess()]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dataFactory = await _context.DataFactory.FindAsync(id);
            if (dataFactory == null)
                return NotFound();

            if (!await CanPerformCurrentActionOnRecord(dataFactory))
                return new ForbidResult();
            return View(dataFactory);
        }

        // POST: DataFactory/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ChecksUserAccess]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,ResourceGroup,SubscriptionUid,DefaultKeyVaultUrl,LogAnalyticsWorkspaceId")] DataFactory dataFactory)
        {
            if (id != dataFactory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dataFactory);

                    if (!await CanPerformCurrentActionOnRecord(dataFactory))
                        return new ForbidResult();
			
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DataFactoryExists(dataFactory.Id))
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
            return View(dataFactory);
        }

        // GET: DataFactory/Delete/5
        [ChecksUserAccess]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dataFactory = await _context.DataFactory
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dataFactory == null)
                return NotFound();
		
            if (!await CanPerformCurrentActionOnRecord(dataFactory))
                return new ForbidResult();

            return View(dataFactory);
        }

        // POST: DataFactory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ChecksUserAccess()]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var dataFactory = await _context.DataFactory.FindAsync(id);

            if (!await CanPerformCurrentActionOnRecord(dataFactory))
                return new ForbidResult();
		
            _context.DataFactory.Remove(dataFactory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexDataTable));
        }

        private bool DataFactoryExists(long id)
        {
            return _context.DataFactory.Any(e => e.Id == id);
        }
    }
}
