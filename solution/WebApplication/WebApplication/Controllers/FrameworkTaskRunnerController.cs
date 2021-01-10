using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication.Services;
using WebApplication.Framework;
using WebApplication.Models;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApplication.Controllers
{
    public partial class FrameworkTaskRunnerController : BaseController
    {
        protected readonly AdsGoFastContext _context;
        

        public FrameworkTaskRunnerController(AdsGoFastContext context, ISecurityAccessProvider securityAccessProvider, IEntityRoleProvider roleProvider) : base(securityAccessProvider, roleProvider)
        {
            Name = "FrameworkTaskRunner";
            _context = context;
        }

        // GET: FrameworkTaskRunner
        public async Task<IActionResult> Index()
        {
            return View(await _context.FrameworkTaskRunner.ToListAsync());
        }

        // GET: FrameworkTaskRunner/Details/5
        [ChecksUserAccess]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var frameworkTaskRunner = await _context.FrameworkTaskRunner
                .FirstOrDefaultAsync(m => m.TaskRunnerId == id);
            if (frameworkTaskRunner == null)
                return NotFound();
            if (!await CanPerformCurrentActionOnRecord(frameworkTaskRunner))
                return new ForbidResult();


            return View(frameworkTaskRunner);
        }

        // GET: FrameworkTaskRunner/Create
        public IActionResult Create()
        {
     FrameworkTaskRunner frameworkTaskRunner = new FrameworkTaskRunner();
            frameworkTaskRunner.ActiveYn = true;
            return View(frameworkTaskRunner);
        }

        // POST: FrameworkTaskRunner/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ChecksUserAccess]
        public async Task<IActionResult> Create([Bind("TaskRunnerId,TaskRunnerName,ActiveYn,Status,MaxConcurrentTasks")] FrameworkTaskRunner frameworkTaskRunner)
        {
            if (ModelState.IsValid)
            {
                _context.Add(frameworkTaskRunner);
                if (!await CanPerformCurrentActionOnRecord(frameworkTaskRunner))
                {
                    return new ForbidResult();
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexDataTable));
            }
            return View(frameworkTaskRunner);
        }

        // GET: FrameworkTaskRunner/Edit/5
        [ChecksUserAccess()]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var frameworkTaskRunner = await _context.FrameworkTaskRunner.FindAsync(id);
            if (frameworkTaskRunner == null)
                return NotFound();

            if (!await CanPerformCurrentActionOnRecord(frameworkTaskRunner))
                return new ForbidResult();
            return View(frameworkTaskRunner);
        }

        // POST: FrameworkTaskRunner/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ChecksUserAccess]
        public async Task<IActionResult> Edit(int id, [Bind("TaskRunnerId,TaskRunnerName,ActiveYn,Status,MaxConcurrentTasks")] FrameworkTaskRunner frameworkTaskRunner)
        {
            if (id != frameworkTaskRunner.TaskRunnerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(frameworkTaskRunner);

                    if (!await CanPerformCurrentActionOnRecord(frameworkTaskRunner))
                        return new ForbidResult();
			
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FrameworkTaskRunnerExists(frameworkTaskRunner.TaskRunnerId))
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
            return View(frameworkTaskRunner);
        }

        // GET: FrameworkTaskRunner/Delete/5
        [ChecksUserAccess]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var frameworkTaskRunner = await _context.FrameworkTaskRunner
                .FirstOrDefaultAsync(m => m.TaskRunnerId == id);
            if (frameworkTaskRunner == null)
                return NotFound();
		
            if (!await CanPerformCurrentActionOnRecord(frameworkTaskRunner))
                return new ForbidResult();

            return View(frameworkTaskRunner);
        }

        // POST: FrameworkTaskRunner/Delete/5
        [HttpPost, ActionName("Delete")]
        [ChecksUserAccess()]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var frameworkTaskRunner = await _context.FrameworkTaskRunner.FindAsync(id);

            if (!await CanPerformCurrentActionOnRecord(frameworkTaskRunner))
                return new ForbidResult();
		
            _context.FrameworkTaskRunner.Remove(frameworkTaskRunner);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexDataTable));
        }

        private bool FrameworkTaskRunnerExists(int id)
        {
            return _context.FrameworkTaskRunner.Any(e => e.TaskRunnerId == id);
        }

        public async Task<IActionResult> IndexDataTable()
        {
            return View();
        }

        public ActionResult SetFrameworkTaskRunnersBackToIdle()
        {
            List<Int64> Pkeys = JsonConvert.DeserializeObject<List<Int64>>(Request.Form["Pkeys"]);
            var entitys = _context.FrameworkTaskRunner.Where(ti => Pkeys.Contains(ti.TaskRunnerId));
            entitys.ForEachAsync(ti =>
            {
                ti.Status = "Idle";
            }).Wait();
            _context.SaveChanges();

            //TODO: Add Error Handling
            return new OkObjectResult(new { });
        }


    }

    public partial class FrameworkTaskRunnerDapperController : BaseController
    {
        private readonly AdsGoFastDapperContext _context;

        public FrameworkTaskRunnerDapperController(AdsGoFastDapperContext context, ISecurityAccessProvider securityAccessProvider, IEntityRoleProvider roleprovider) : base(securityAccessProvider, roleprovider)
        {
            _context = context;
        }


        public JObject GridCols()
        {
            JObject GridOptions = new JObject();

            JArray cols = new JArray();
            cols.Add(JObject.Parse("{ 'data':'TaskRunnerId', 'name':'TaskRunnerId', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'TaskRunnerName', 'name':'TaskRunnerName', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'Status', 'name':'Status', 'autoWidth':true, 'ads_format':'taskrunnerstatus' }"));
            cols.Add(JObject.Parse("{ 'data':'ActiveYN', 'name':'ActiveYn', 'autoWidth':true, 'ads_format':'bool' }"));
            cols.Add(JObject.Parse("{ 'data':'LastExecutionStartDateTime', 'name':'LastRunStart', 'autoWidth':true, 'ads_format':'datetime' }"));
            cols.Add(JObject.Parse("{ 'data':'LastExecutionEndDateTime', 'name':'LastRunEnd', 'autoWidth':true, 'ads_format':'datetime'  }"));
            cols.Add(JObject.Parse("{ 'data':'MaxConcurrentTasks', 'name':'Task Slots', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'TaskInstances', 'name':'Assigned Tasks', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'RunningTasks', 'name':'Running Tasks', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'RunTimeInSeconds', 'name':'Run Time In Seconds', 'autoWidth':true }"));

            HumanizeColumns(cols);

            JArray pkeycols = new JArray();
            pkeycols.Add("TaskRunnerId");

            JArray Navigations = new JArray();

            GridOptions["GridColumns"] = cols;
            GridOptions["ModelName"] = "FrameworkTaskRunner";
            GridOptions["PrimaryKeyColumns"] = pkeycols;
            GridOptions["Navigations"] = Navigations;
            GridOptions["SuppressCrudButtons"] = false;
            GridOptions["CrudController"] = "FrameworkTaskRunner";
            GridOptions["CrudButtons"] = GetSecurityFilteredActions("Create,Edit,Details,Delete");

            return GridOptions;


        }



        public ActionResult GetGridOptions()
        {
            return new OkObjectResult(JsonConvert.SerializeObject(GridCols()));
        }

        public ActionResult GetGridData()
        {
            try
            {

                string draw = Request.Form["draw"];
                string start = Request.Form["start"];
                string length = Request.Form["length"];
                string sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"] + "][data]"];
                string sortColumnDir = Request.Form["order[0][dir]"];

                string searchValue = Request.Form["search[value]"];

                //Paging Size (10,20,50,100)    
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;


                string Order = $@"  
                    ORDER BY [{sortColumn}] {sortColumnDir}
                ";

                string Paging = System.Environment.NewLine + $"OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY" + System.Environment.NewLine;

                string Having = System.Environment.NewLine + $"HAVING count(distinct tm.TaskMasterId)>0" + System.Environment.NewLine;

                string Query = @"
                Select 
	                ftr.*, 
	                TaskInstances, 
	                RunningTasks,
					RunTimeInSeconds = DATEDIFF(SECOND,ftr.LastExecutionStartDateTime, ftr.LastExecutionEndDateTime)
                from FrameworkTaskRunner ftr
                left join 
                (
	                Select ti.TaskRunnerId, 
		                   TaskInstances = count(*), 
		                   RunningTasks = sum(case when ti.LastExecutionStatus = 'InProgress' then 1 else 0 end)
						  
	                from TaskInstance ti 
	                group by ti.TaskRunnerId
                ) ti	
	                on ftr.TaskRunnerId = ti.TaskRunnerId";

                string RowCountQuery = "Select count(*) from (" + Query + ") ct";
                Query = Query + Order + Paging;

                recordsTotal = _context.GetConnection().ExecuteScalar<int>(RowCountQuery);
                // Getting all Customer data    
                var modelDataAll = (from row in _context.GetConnection().Query(Query) select (IDictionary<string, object>)row).AsList();

                //Returning Json Data    
                var jserl = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
                };

                return new OkObjectResult(JsonConvert.SerializeObject(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = modelDataAll }, jserl));

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
