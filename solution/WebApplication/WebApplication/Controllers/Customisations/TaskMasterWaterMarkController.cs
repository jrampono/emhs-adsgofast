using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication.Framework;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public partial class TaskMasterWaterMarkController : BaseController
    {
        public async Task<IActionResult> IndexDataTable()
        {
            var adsGoFastContext = _context.TaskMaster.Take(1);
            return View(await adsGoFastContext.ToListAsync());
        }

        public JObject GridCols()
        {
            JObject GridOptions = new JObject();

            JArray cols = new JArray();
            cols.Add(JObject.Parse("{ 'data':'TaskMasterId', 'name':'Id', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'TaskMaster.TaskMasterName', name:'Task Master Name', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'TaskMasterWaterMarkColumn', 'name':'Watermark Column Name', 'autoWidth':true, 'width':'30%' }"));
            cols.Add(JObject.Parse("{ 'data':'TaskMasterWaterMarkColumnType', 'name':'Column Type', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'TaskMasterWaterMarkDateTime', 'name':'Watermark Date', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'TaskMasterWaterMarkBigInt', 'name':'Watermark Value', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'TaskWaterMarkJson', 'name':'Json', 'autoWidth':true }"));
            cols.Add(JObject.Parse("{ 'data':'ActiveYn', 'name':'Active', 'autoWidth':true, 'ads_format': 'bool' }"));

            HumanizeColumns(cols);

            JArray pkeycols = new JArray();
            pkeycols.Add("TaskMasterId");

            JArray Navigations = new JArray();
            
            GridOptions["GridColumns"] = cols;
            GridOptions["ModelName"] = "TaskMasterWaterMark";
            GridOptions["PrimaryKeyColumns"] = pkeycols;
            GridOptions["Navigations"] = Navigations;
            GridOptions["CrudButtons"] = GetSecurityFilteredActions("Create,Edit,Details,Delete");

            return GridOptions;
        }

        [ChecksUserAccess]
        public ActionResult GetGridOptions()
        {
            return new OkObjectResult(JsonConvert.SerializeObject(GridCols()));
        }

        [ChecksUserAccess]
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

                // Getting all Customer data    
                var modelDataAll = (from temptable in _context.TaskMasterWaterMark
                                    select temptable);

                //filter the list by permitted roles
                if (!CanPerformCurrentActionGlobally())
                {
                    var permittedRoles = GetPermittedGroupsForCurrentAction();
                    var identity = User.Identity.Name;

                    modelDataAll =
                        (from md in modelDataAll
                         join tm in _context.TaskMaster
                            on md.TaskMasterId equals tm.TaskMasterId
                         join tg in _context.TaskGroup
                            on tm.TaskGroupId equals tg.TaskGroupId
                         join rm in _context.SubjectAreaRoleMap
                            on tg.SubjectAreaId equals rm.SubjectAreaId
                         where
                             GetUserAdGroupUids().Contains(rm.AadGroupUid)
                             && permittedRoles.Contains(rm.ApplicationRoleName)
                             && rm.ExpiryDate > DateTimeOffset.Now
                             && rm.ActiveYn
                         select md).Distinct();
                }

                //Sorting    
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    modelDataAll = modelDataAll.OrderBy(sortColumn + " " + sortColumnDir);
                }
                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    modelDataAll = modelDataAll.Where(m => m.TaskWaterMarkJson.Contains(searchValue) 
                    || m.TaskMaster.TaskMasterName.Contains(searchValue)
                    || m.TaskMasterWaterMarkColumn.Contains(searchValue));
                }

                //Filter based on querystring params
                if (!(string.IsNullOrEmpty(Request.Form["QueryParams[TaskMasterId]"])))
                {
                    var taskMasterIdFilter = System.Convert.ToInt64(Request.Form["QueryParams[TaskMasterId]"]);
                    modelDataAll = modelDataAll.Where(t => t.TaskMasterId == taskMasterIdFilter);
                }

                //Custom Includes
                modelDataAll = modelDataAll
                    .Include(x=>x.TaskMaster)
                    .AsNoTracking();


                //total number of rows count     
                recordsTotal = modelDataAll.Count();


                //Paging               
                var data = modelDataAll.Skip(skip).Take(pageSize).ToList();




                //Returning Json Data    
                var jserl = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
                };

                return new OkObjectResult(JsonConvert.SerializeObject(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data }, jserl));

            }
            catch (Exception)
            {
                throw;
            }
        }


        [ChecksUserAccess]
        public async Task<IActionResult> UpdateTaskMasterActiveYN()
        {
            List<Int64> Pkeys = JsonConvert.DeserializeObject<List<Int64>>(Request.Form["Pkeys"]);
            bool Status = JsonConvert.DeserializeObject<bool>(Request.Form["Status"]);
            var entitys = await  _context.TaskMasterWaterMark.Where(ti => Pkeys.Contains(ti.TaskMasterId)).ToArrayAsync();

            foreach(var ti in entitys)
            {
                if (!await CanPerformCurrentActionOnRecord(ti))
                    return Forbid();

                ti.ActiveYn = Status;
            }
            _context.SaveChanges();

            //TODO: Add Error Handling
            return new OkObjectResult(new { });
        }

        [ChecksUserAccess]
        public async Task<IActionResult> EditPlus(long? id)
        {
            if (id == null)
                return NotFound();

            var taskMaster = await _context.TaskMaster.FindAsync(id);
            if (taskMaster == null)
                return NotFound();

            if (!await CanPerformCurrentActionOnRecord(taskMaster))
                return Forbid();

            return View(taskMaster);
        }
    }

    
}
